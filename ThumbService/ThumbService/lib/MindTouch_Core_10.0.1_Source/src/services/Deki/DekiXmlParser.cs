/*
 * MindTouch Core - open source enterprise collaborative networking
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using MindTouch.Deki.Data;
using MindTouch.Deki.Logic;
using MindTouch.Deki.Script;
using MindTouch.Deki.Script.Compiler;
using MindTouch.Deki.Script.Expr;
using MindTouch.Deki.Script.Runtime;
using MindTouch.Deki.Script.Runtime.Library;
using MindTouch.Dream;
using MindTouch.Tasking;
using MindTouch.Web;
using MindTouch.Xml;

namespace MindTouch.Deki {
    using Yield = IEnumerator<IYield>;

    public class ParserState {

		//--- Fields ---
        public readonly Dictionary<string, XDoc> ProcessedPages = new Dictionary<string, XDoc>();
        public readonly List<PageBE> ProcessingStack = new List<PageBE>();
    }

    public struct ParserResult {

        //--- Fields ---
        public readonly XDoc Content;
        public readonly string ContentType;
        public readonly Title RedirectsToTitle;
        public readonly XUri RedirectsToUri;
        public readonly bool HasScriptContent;
        public readonly List<Title> Links;
        public readonly List<Title> Templates;
        public readonly List<string> Tags;
        private string _bodyText;
        private string _summary;

        //--- Constructors ---
        public ParserResult(XDoc content, string contentType, Title redirectsTo, XUri redirectToUri, bool hasScriptContent, List<Title> links, List<Title> templates, List<string> tags) {
            this.Content = content;
            this.ContentType = contentType;
            this.RedirectsToTitle = redirectsTo;
            this.RedirectsToUri = redirectToUri;
            this.HasScriptContent = hasScriptContent;
            this.Links = links;
            this.Templates = templates;
            this.Tags = tags;
            _bodyText = null;
            _summary = null;
        }

        //--- Properties ---
        public XDoc MainBody { get { return Content["body[not(@target)]"]; } }
        public XDoc Bodies { get { return Content["body"]; } }
        public XDoc Head { get { return Content["head"]; } }
        public XDoc Tail { get { return Content["tail"]; } }

        public string BodyText {
            get {
                if (_bodyText == null) {
                    if (ContentType == DekiMimeType.DEKI_XML0805) {
                        StringWriter sr = new StringWriter();
                        XmlWriter writer = new EscapedXmlTextWriter(sr);
                        MainBody.AsXmlNode.WriteTo(writer);
                        writer.Close();

                        _bodyText = sr.ToString();

                        int start = _bodyText.IndexOf('>') + 1;
                        int stop = _bodyText.LastIndexOf('<');
                        if ((start > 0) && (stop >= 0) && (start <= stop)) {
                            _bodyText = _bodyText.Substring(start, stop - start);
                        } else {
                            _bodyText = string.Empty;
                        }
                    } else {
                        _bodyText = MainBody.ToInnerXHtml();
                    }
                }
                return _bodyText;
            }
        }

        public string Summary {
            get {
                if (null == _summary) {
                    
                    StringWriter sr = new StringWriter();
                    XmlWriter writer = new EscapedXmlTextWriter(sr);
                    MainBody.AsXmlNode.WriteContentTo(writer);
                    writer.Close();
                    string tempSummary = sr.ToString();

                    if (tempSummary.Length > 500) {
                        tempSummary = tempSummary.Substring(0, 497) + "...";
                    }
                    _summary = tempSummary;
                }
                return _summary;
            }
        }
    }

    public static class ExcludedTags {

        //--- Constants ---
        private static readonly Dictionary<string, string> _excludedTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> _excludedClasses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //--- Class Constructor ---
        static ExcludedTags() {
            _excludedTags["img"] = null;
            _excludedTags["pre"] = null;
            _excludedTags["embed"] = null;
            _excludedTags["script"] = null;
            _excludedTags["style"] = null;
            _excludedTags["applet"] = null;
            _excludedTags["input"] = null;
            _excludedTags["textarea"] = null;
            _excludedTags["nowiki"] = null;

            _excludedClasses["nowiki"] = null;
            _excludedClasses["urlexpansion"] = null;
            _excludedClasses["plain"] = null;
            _excludedClasses["live"] = null;
            _excludedClasses["script"] = null;
        }

        //--- Class Methods ---
        public static bool Contains(XmlNode node) {
            return Contains(node, false);
        }

        public static bool Contains(XmlNode node, bool checkParents) {
            if(node == null) {
                return false;
            }
            bool forceEval = false;
            if ((null != node.Attributes) && (null != node.Attributes["class"])) {
                foreach (String classAttr in node.Attributes["class"].Value.Split(' ')) {
                    if(_excludedClasses.ContainsKey(classAttr)) {
                        return true;
                    }
                    if (classAttr.EqualsInvariantIgnoreCase("eval")) {
                        forceEval = node.LocalName.EqualsInvariant("pre");
                    }
                }
            }

            // TODO (steveb): if class="eval" is found, shouldn't that force evaluation regardless of what parent nodes it may have?

            // if the node is an excluded tag, then exclude it unless it is a pre tag marked with class="eval"
            if(!forceEval && _excludedTags.ContainsKey(node.Name)) {
                return true;
            }
            if (checkParents && (null != node.ParentNode)) {
                return Contains(node.ParentNode, true);
            }
            return false;
        }
    }

    public class DekiXmlParser {

        //--- Constants --

        // constants used for parsing URIs
        private static readonly string MAILTO_PATTERN = String.Format(@"(mailto:/*)?{0}@{1}(:\d*)?", /* RFC 2822 */ @"[\w\.!#\$%&'\*\+\-/=\?^_`{\|}~]+", XUri.HOST_REGEX);
        private static readonly string URL_PATTERN = String.Format(@"(?<=^|\W)({0}|{1})(?<![\.!?,:';])", XUri.URI_REGEX, MAILTO_PATTERN);
        private static readonly string REDIRECT_PATTERN = String.Format(@"^#REDIRECT(\s|\:)*\[\[((?<uri>{0})|(?<page>.*))\]\]", XUri.URI_REGEX);
        private static readonly Regex MAILTO_REGEX = new Regex("^" + MAILTO_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex URL_REGEX = new Regex(URL_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex REDIRECT_REGEX = new Regex(REDIRECT_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex WHITESPACE = new Regex("[ \\n\\r\\t][ \\n\\r\\t]+", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);
        private static readonly Regex FILE_PATH_REGEX = new Regex(@"/?(index\.php\?title=)?File:(?<path>.*/)?(?<filename>[^/]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // miscellaneous constants
        private static readonly Regex SIZE_REGEX = new Regex(@"^(?<size>\d+)px$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly string HIDE_BANNED_WORD = new StringBuilder().Append('*', 128).ToString();
        private const string MAIN_BODY_XPATH = "content/body[not(@target)]";
        private const string UNWANTED_XINHA_CODE_IN_ATTRIBUTES = "if(window.top && window.top.Xinha){return false}";
        private const string ALL_HEADINGS_XPATH = ".//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//h6";
        private const string ALL_NESTED_HEADINGS_XPATH = ".//h1[.//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//h6] | .//h2[.//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//h6] | .//h3[.//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//h6] | .//h4[.//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//h6] | .//h5[.//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//h6] | .//h6[.//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//h6]";
        private const string FILE_API_PATTERN = @"^/*{0}/(?<fileid>\d+)/=(?<filename>.+)$";

        //--- Class Methods ---
        public static XDoc Include(string path, string section, bool showTitle, int? adjustHeading, DekiScriptLiteral args, int revision, bool followRedirects, bool returnEmptyDocIfNotFound, bool createAnchor) {

            // retrieve the page requested
            Title titleToInclude = Title.FromUriPath(path);
            PageBE pageToInclude = PageBL.GetPageByTitle(titleToInclude);
            if(followRedirects) {
                pageToInclude = PageBL.ResolveRedirects(pageToInclude);
            }

            // If the page was not found, create a link to the page placeholder
            if (pageToInclude.ID == 0) {
                return returnEmptyDocIfNotFound ? XDoc.Empty : new XDoc("html").Start("body").Start("a").Attr("href", path).Value(titleToInclude.AsUserFriendlyName()).End().End();
            }
            if (!PermissionsBL.IsUserAllowed(DekiContext.Current.User, pageToInclude, Permissions.READ)) {
                return new XDoc("html").Start("body").Value(DekiResources.RESTRICT_MESSAGE).End();
            }

            // load old revision of page
            if(revision != 0) {
                PageBL.ResolvePageRev(pageToInclude, revision.ToString());
            }

            // parse the page
            ParserResult result = Parse(pageToInclude, pageToInclude.ContentType, pageToInclude.Language, pageToInclude.GetText(DbUtils.CurrentSession), ParserMode.VIEW, true, -1, args, null);

            // if requested, extract the specified section
            if (section != null) {
                XmlNode startNode, endNode;
                FindSection(result.MainBody, section, out startNode, out endNode);
                XDoc body = ExtractSection(false, startNode, endNode);

                // If the section was not found, create a link to the page whose section is missing
                if (null == startNode) {
                    if(returnEmptyDocIfNotFound) {
                        return XDoc.Empty;
                    }
                    body = body.Start("a").Attr("href", path).Attr("rel", "internal").Value(titleToInclude.AsUserFriendlyName()).End();
                } else if(adjustHeading.HasValue) {

                    // header needs to be adjusted
                    int current;
                    if(int.TryParse(startNode.LocalName.Substring(1), out current)) {
                        if(showTitle) {
                            body.AddNodesInFront(new XDoc("body").Add(result.MainBody[startNode]));
                        }
                        RelabelHeadings(body, adjustHeading.Value - current);
                    }
                }
                return new XDoc("html").Add(result.Head).Add(body).Add(result.Tail);
            } else {
                XDoc doc = new XDoc("html").Add(result.Head);

                // check if header needs to be adjusted
                if(adjustHeading.HasValue) {
                    if(showTitle) {
                        result.MainBody.AddNodesInFront(new XDoc("body").Elem("h1", pageToInclude.CustomTitle ?? pageToInclude.Title.AsUserFriendlyName()));
                    }
                    RelabelHeadings(result.MainBody, adjustHeading.Value - 1);
                }

                // add optional anchor
                if(createAnchor) {
                    result.MainBody.AddNodesInFront(new XDoc("body").Start("span").Attr("pagelink", DekiContext.Current.UiUri.AtPath(pageToInclude.Title.AsUiUriPath())).Attr("id", "s" + pageToInclude.ID).End());
                }
                doc.AddAll(result.Bodies).Add(result.Tail);
                return doc;
            }
        }

        private static void RelabelHeadings(XDoc body, int offset) {
            if (offset != 0) {
                foreach (XDoc heading in body[ALL_HEADINGS_XPATH]) {
                    int current;
                    if (int.TryParse(heading.Name.Substring(1), out current)) {
                        heading.Rename("h" + Math.Max(1, Math.Min(current + offset, 6)));
                    }
                }
            }
        }

        public static ParserResult ParseGlobalTemplate(PageBE contextPage, PageBE templatePage) {

            // update the parse state to indicate the page is in the process of being parsed
            ParserState parseState = GetParseState();
            string parseStateKey = GetParserCacheKey(contextPage, ParserMode.VIEW_NO_EXECUTE);
            parseState.ProcessedPages[parseStateKey] = null;
            parseState.ProcessingStack.Add(contextPage);

            // set current page in context
            DreamContext context = DreamContext.Current;
            PageBE prevBasePage = context.GetState<PageBE>("CurrentPage");
            CultureInfo prevCulture = context.Culture;
            context.SetState("CurrentPage", contextPage);
            context.Culture = HttpUtil.GetCultureInfoFromHeader(contextPage.Language, context.Culture);

            // parse the page
            ParserResult result = Parse(templatePage, templatePage.ContentType, templatePage.Language, templatePage.GetText(DbUtils.CurrentSession), ParserMode.VIEW, true, -1, DekiScriptNil.Value, null);

            // restore previous page context
            context.SetState("CurrentPage", prevBasePage);
            context.Culture = prevCulture;

            // update the parse state to indicate the page as been successfully parsed
            parseState.ProcessedPages.Remove(parseStateKey);
            parseState.ProcessingStack.RemoveAt(parseState.ProcessingStack.Count - 1);

            // return result
            return result;
        }

        public static ParserResult ParseSave(PageBE page, string contentType, string language, string content, int section, string xpath, bool removeIllegalElements, Title relToTitle) {
            XDoc result;
            XDoc contentDoc = ParseContent(content, DekiMimeType.DEKI_TEXT);
            if (null == contentDoc) {
                throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.CONTENT_CANNOT_BE_PARSED));
            }

            // convert <nowiki> to <span class="nowiki">
            foreach (XDoc nowiki in contentDoc[".//nowiki"]) {
                nowiki.Rename("span").Attr("class", "nowiki");
            }

            // HACK HACK HACK (steveb): inspect onclick, onfocus, etc. attributes for bogus prefixes (stupid Xinha bug)
            foreach(XDoc attr in contentDoc[".//@onblur | .//@onclick | .//@ondblclick | .//@onfocus | .//@onkeydown | .//@onkeypress | .//@onkeyup | .//@onmousedown | .//@onmousemove | .//@onmouseout | .//@onmouseover | .//@onmouseup"]) {
                string value = attr.Contents;
                while(value.StartsWithInvariantIgnoreCase(UNWANTED_XINHA_CODE_IN_ATTRIBUTES)) {
                    value = value.Substring(UNWANTED_XINHA_CODE_IN_ATTRIBUTES.Length);
                }
                attr.ReplaceValue(value);
            }

            // denormalize links if needed (for import/export)
            if (null != relToTitle) {
                foreach (XDoc addr in contentDoc[".//a[@href.path] | .//img[@src.path]"]) {
                    if (!ExcludedTags.Contains(addr.AsXmlNode.ParentNode, true)) {
                        Title title;
                        if (addr.HasName("a"))  {
                            title = Title.FromRelativePath(relToTitle, addr["@href.path"].Contents);
                            addr.RemoveAttr("href.path");
                            title.Anchor = addr["@href.anchor"].AsText;
                            addr.RemoveAttr("href.anchor");
                            title.Query = addr["@href.query"].AsText;
                            addr.RemoveAttr("href.query");
                            title.Filename = addr["@href.filename"].AsText;
                            addr.RemoveAttr("href.filename");
                            addr.Attr("href", title.AsUiUriPath());
                        } else if (addr.HasName("img")) {
                            title = Title.FromRelativePath(relToTitle, addr["@src.path"].Contents);
                            addr.RemoveAttr("src.path");
                            title.Filename = addr["@src.filename"].AsText;
                            addr.RemoveAttr("src.filename");
                            addr.Attr("src", title.AsUiUriPath());
                        } else {
                            DekiContext.Current.Instance.Log.Warn("ParseSave: unexpected element");
                        }
                    }
                }
            }

            // remove local prefixes from links
            foreach (XDoc addr in contentDoc[".//a[@href] | .//img[@src]"]) {
                if (!ExcludedTags.Contains(addr.AsXmlNode.ParentNode, true)) {
                    string attrName = addr.HasName("a") ? "href" : "src";
                    string link = addr["@" + attrName].Contents.Trim();

                    bool hasLocalPrefix = true;
                    if (link.StartsWithInvariantIgnoreCase(Utils.MKS_PATH)) {
                        link = link.Substring(Utils.MKS_PATH.Length);
                    } else if (link.StartsWithInvariantIgnoreCase(DekiContext.Current.UiUri.Uri.SchemeHostPort + "/")) {
                        link = link.Substring(DekiContext.Current.UiUri.Uri.SchemeHostPort.Length + 1);
                    } else {
                        hasLocalPrefix = false;
                    }
                    if (hasLocalPrefix) {
                        if (!link.StartsWith("/")) {
                            link = "/" + link;
                        }
                        addr.Attr(attrName, link);
                    }
                }
            }

            // pass the input through the xml validator
            DekiScriptLibrary.ValidateXHtml(contentDoc["body"], DekiContext.Current.Instance.SafeHtml && !PermissionsBL.IsUserAllowed(DekiContext.Current.User, Permissions.UNSAFECONTENT), removeIllegalElements);
            XDoc validatedDoc = CreateParserDocument(page, contentType, language, string.Empty, ParserMode.SAVE);
            validatedDoc["content/@type"].ReplaceValue(DekiMimeType.DEKI_TEXT);
            validatedDoc[".//body"].Replace(contentDoc[".//body"]);

            // check if we need to remove a trailing <br /> element from <body>
            XDoc body = validatedDoc[".//body"];
            if(!body.IsEmpty) {
                XmlNode last = body.AsXmlNode.LastChild;

                // check if last node is a <table> or <pre> element
                if((last != null) && (last.NodeType == XmlNodeType.Element)) {
                    if(last.LocalName.EqualsInvariant("br")) {

                        // remove <br /> element
                        body[last].Remove();
                    } else if(
                            last.LocalName.EqualsInvariant("p") && (
                                (last.ChildNodes.Count == 0) || (
                                    (last.ChildNodes.Count == 1) && (
                                        ((last.ChildNodes[0].NodeType == XmlNodeType.Text) && (last.ChildNodes[0].Value.Trim().Length == 0)) ||
                                        (last.ChildNodes[0].NodeType == XmlNodeType.SignificantWhitespace) ||
                                        (last.ChildNodes[0].NodeType == XmlNodeType.Whitespace)
                                    )
                                )
                            )
                    ) {

                        // remove <p></p> element
                        body[last].Remove();
                    }
                }
            }

            // if section is negative, update the entire page contents
            if ((section < 0) && (null == xpath)) {
                result = validatedDoc;
            } else {

                // convert the existing page and new section contents to a consistent format
                result = CreateParserDocument(page, page.ContentType, page.Language, page.GetText(DbUtils.CurrentSession), ParserMode.RAW);
                XDoc headingDoc = validatedDoc;
                if (DekiMimeType.DEKI_TEXT == contentType) {
                    WikiConverter_TextToXml.Convert(headingDoc);
                }

                if (section < 0) {

                    // if no section was specified, replace the node identified by the xpath
                    if (xpath.StartsWith("/")) {
                        xpath = "." + xpath;
                    }
                    try {
                        result[MAIN_BODY_XPATH][xpath].ReplaceWithNodes(headingDoc[MAIN_BODY_XPATH]);
                    } catch {
                        throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.XPATH_PARAM_INVALID));
                    }
                } else if (0 == section) {

                    // append the section to the end of the document
                    result[MAIN_BODY_XPATH].AddNodes(headingDoc[MAIN_BODY_XPATH]);
                } else {

                    // find the specified section number
                    XmlNode startNode, endNode;
                    FindSection(result, section, out startNode, out endNode);
                    if (null == startNode) {
                        throw new DreamAbortException(DreamMessage.BadRequest(DekiResources.SECTION_PARAM_INVALID));
                    }

                    // replace the section contents
                    AddChildrenBefore(startNode, headingDoc[MAIN_BODY_XPATH].AsXmlNode);
                    XmlNode currentNode = startNode;
                    while (currentNode != null && currentNode != endNode) {
                        XmlNode nodeToDelete = currentNode;
                        currentNode = currentNode.NextSibling;
                        nodeToDelete.ParentNode.RemoveChild(nodeToDelete);
                    }
                }
            }
            return Parse(page, result, ParserMode.SAVE, false, null, relToTitle);
        }

        public static ParserResult Parse(PageBE page, ParserMode mode, int section, bool isInclude) {
            return Parse(page, page.ContentType, page.Language, page.GetText(DbUtils.CurrentSession), mode, isInclude, section, null, null);
        }

        public static ParserResult Parse(PageBE page, ParserMode mode) {
            return Parse(page, page.ContentType, page.Language, page.GetText(DbUtils.CurrentSession), mode, false, -1, null, null);
        }

        public static ParserResult Parse(PageBE page, string contentType, string language, string content, ParserMode mode, bool isInclude, int section, DekiScriptLiteral args, Title relToTitle) {
            XDoc xhtml = CreateParserDocument(page, contentType, language, content, mode);

            // check if we're interested only in a given section
            if(0 < section) {
                XDoc mainBody = xhtml[MAIN_BODY_XPATH];
                XmlNode startNode, endNode;
                FindSection(mainBody, section, out startNode, out endNode);
                mainBody.Replace(ExtractSection(true, startNode, endNode));
            }

            // process page contents
            ParserResult result = Parse(page, xhtml, mode, isInclude, args, relToTitle);

            // check if we need to append extra elements to document for editing
            if(mode == ParserMode.EDIT) {
                XDoc body = result.MainBody;
                if(!body.IsEmpty) {
                    XmlNode last = body.AsXmlNode.LastChild;

                    // check if last node is a <table> or <pre> element
                    if(last != null) {
                        if(last.NodeType == XmlNodeType.Element) {
                            if(last.LocalName.EqualsInvariant("table") || last.LocalName.EqualsInvariant("pre")) {

                                // add a <br /> element
                                body.Elem("br");
                            } else if(last.LocalName.EqualsInvariant("h1") ||
                                last.LocalName.EqualsInvariant("h2") ||
                                last.LocalName.EqualsInvariant("h3") ||
                                last.LocalName.EqualsInvariant("h4") ||
                                last.LocalName.EqualsInvariant("h5") ||
                                last.LocalName.EqualsInvariant("h6")
                            ) {

                                // add a <br /> element
                                body.Elem("br");
                            }
                        }
                    } else {

                        // add a <br /> element
                        body.Elem("br");
                    }
                }
            } else if(((mode == ParserMode.VIEW) || (mode == ParserMode.VIEW_NO_EXECUTE)) && isInclude) {
                XDoc body = result.MainBody;
                XmlNode para = null;
                foreach(XmlNode node in body.AsXmlNode.ChildNodes) {
                    if((node.NodeType == XmlNodeType.Element) && node.Name.EqualsInvariant("p") && (para == null)) {

                        // capture the first <p> node
                        para = node;
                        continue;
                    } else if(node.NodeType== XmlNodeType.Whitespace) {

                        // found a whitespace node; ignore it
                        continue;
                    } else if((node.NodeType == XmlNodeType.Text) && string.IsNullOrEmpty(node.Value.Trim())) {

                        // found an empty text node; ignore it
                        continue;
                    }

                    // found other content; don't strip anything
                    para = null;
                    break;
                }

                // check if a single paragraph was found; if so, remove the <p> tags, but keep the contents
                if(para != null) {
                    body.RemoveNodes();
                    body.AddNodes(body[para]);
                }
            }
            return result;
        }

        public static XDoc CreateParserDocument(PageBE page, ParserMode mode) {
            return CreateParserDocument(page, page.ContentType, page.Language, page.GetText(DbUtils.CurrentSession), mode);
        }

        public static void PostProcessParserResults(ParserResult parserResult) {
            foreach(XDoc tail in parserResult.Tail["script"]) {
                var node = tail.AsXmlNode;

                // check if script is using $() to load when DOM is ready
                if(!node.InnerText.StartsWithInvariant("$(function")) {
                    node.InsertBefore(node.OwnerDocument.CreateTextNode("$(function(){"), node.FirstChild);
                    node.InsertAfter(node.OwnerDocument.CreateTextNode("});"), node.LastChild);
                }
            }
        }

        internal static XDoc TruncateTocDepth(XDoc toc, int? depth) {

            // check if we need to limit the depth
            if(depth != null) {
                toc = toc.Clone();
                string xpath = "." + "/ol/li".RepeatPattern(Math.Max(0, depth.Value)) + "/ol";
                toc[xpath].RemoveAll();
            }
            return toc;
        }

        private static ParserResult Parse(PageBE page, XDoc result, ParserMode mode, bool isInclude, DekiScriptLiteral args, Title relToTitle) {
            Title redirectsToTitle = null;
            XUri redirectsToUri = null;
            bool hasScriptContent = false;
            List<Title> outboundLinks = null;
            List<Title> templates = null;
            List<string> tags = null;
            XDoc tableOfContents = null;
            if(ParserMode.RAW != mode) {

                // lookup the current parse state 
                ParserState parseState = GetParseState();
                string parseStateKey = GetParserCacheKey(page, mode);
                XDoc cachedResult = null;

                // if this page is included whithin another, check the state of the parse cache
                if (isInclude && parseState.ProcessedPages.TryGetValue(parseStateKey, out cachedResult)) {
                    if (null == cachedResult) {
                        if(mode == ParserMode.VIEW) {
                            mode = ParserMode.VIEW_NO_EXECUTE;
                            parseStateKey = GetParserCacheKey(page, mode);
                        } else {
                            throw new ArgumentException(DekiResources.INFINITE_PAGE_INCLUSION);
                        }
                    } else {
                        result = cachedResult;
                    }
                }
                if (null == cachedResult) {

                    // init environment for page processing
                    PageBE basePage = page;
                    if(page.Title.IsTemplate) {
                        for(int i = parseState.ProcessingStack.Count - 1; i >= 0; --i) {

                            // select the first non-template page
                            if(!parseState.ProcessingStack[i].Title.IsTemplate) {
                                basePage = parseState.ProcessingStack[i];
                                break;
                            }
                        }
                    }

                    // update the parse state to indicate the page is in the process of being parsed
                    parseState.ProcessedPages[parseStateKey] = null;
                    parseState.ProcessingStack.Add(page);

                    // set current page in context
                    DreamContext context = DreamContext.Current;
                    PageBE prevBasePage = context.GetState<PageBE>("CurrentPage");
                    CultureInfo prevCulture = context.Culture;
                    context.SetState("CurrentPage", basePage);
                    context.Culture = HttpUtil.GetCultureInfoFromHeader(basePage.Language, context.Culture);

                    // resolve the redirect if this page is marked as a redirect or new content is being processed
                    if (page.IsRedirect || (ParserMode.SAVE == mode)) {
                        object redirect = ProcessRedirect(result, mode);
                        redirectsToTitle = redirect as Title;
                        redirectsToUri = redirect as XUri;
                    }
                    if ((null != redirectsToTitle) || (null != redirectsToUri)) {
                        outboundLinks = new List<Title>();
                        if(null != redirectsToTitle) {
                            outboundLinks.Add(redirectsToTitle);
                        }
                    } else {

                        // check 'content-type' to determine if we need to convert
                        if (DekiMimeType.DEKI_TEXT.EqualsInvariant(result["content/@type"].Contents)) {
                            WikiConverter_TextToXml.Convert(result);
                            result["content/@type"].ReplaceValue(DekiMimeType.DEKI_XML0702);
                        }
                        ProcessPage(page, result, mode, isInclude, basePage, args, relToTitle, out tableOfContents, out hasScriptContent, out outboundLinks, out templates, out tags);
                    }

                    // restore previous page context
                    context.SetState("CurrentPage", prevBasePage);
                    context.Culture = prevCulture;

                    // update the parse state to indicate the page as been successfully parsed
                    if(!page.Title.IsTemplate) {
                        parseState.ProcessedPages[parseStateKey] = result;
                    } else {
                        parseState.ProcessedPages.Remove(parseStateKey);
                    }
                    parseState.ProcessingStack.RemoveAt(parseState.ProcessingStack.Count - 1);
                }
            }

            // add table of contents
            XDoc content = result["content"];
            if((tableOfContents != null) && content["body[@target='toc']"].IsEmpty) {
                content.Start("body").Attr("target", "toc").AddNodes(tableOfContents);
            }

            // get content type
            string contentType = result["content/@type"].Contents;
            result["content/@type"].Remove();

            // extract the parser results
            return new ParserResult(
                result["content"], 
                contentType,
                redirectsToTitle, 
                redirectsToUri,
                hasScriptContent, 
                outboundLinks, 
                templates,
                tags
            );
        }

        public static ParserState GetParseState() {

            // retrieve the current parse state from the dream context
            ParserState parseState = DreamContext.Current.GetState<ParserState>("ParseState");
            if(null == parseState) {
                parseState = new ParserState();
                DreamContext.Current.SetState("ParseState", parseState);
            }
            return parseState;
        }

        private static string GetParserCacheKey(PageBE page, ParserMode mode) {
            return page.ID + ":" + page.Revision + "-" + mode;
        }

        private static void AddChildrenBefore(XDoc destination, XDoc doc) {

            // invoke helper that works on xml nodes
            AddChildrenBefore(destination.AsXmlNode, doc.AsXmlNode);
        }

        private static void AddChildrenBefore(XmlNode destination, XmlNode doc) {

            // add all children of the specified doc before the specified destination node
            XmlNode nodeToAdd = destination.OwnerDocument.ImportNode(doc, true);
            if(nodeToAdd.HasChildNodes) {
                for(XmlNode child = nodeToAdd.FirstChild; child != null; child = nodeToAdd.FirstChild) {
                    destination.ParentNode.InsertBefore(child, destination);
                }
            }
        }

        private static void FindSection(XDoc xhtml, string sectionName, out XmlNode startNode, out XmlNode endNode) {
            startNode = null;
            endNode = null;
            sectionName = sectionName.Trim();

            // locate the start and end nodes of the section
            int startLevel = 0;
            foreach(XDoc heading in xhtml[ALL_HEADINGS_XPATH]) {
                if(!ExcludedTags.Contains(xhtml.AsXmlNode.ParentNode, true)) {
                    if(null != startNode) {
                        if(startLevel >= int.Parse(heading.Name.Substring(1)) - 1) {
                            endNode = heading.AsXmlNode;
                            break;
                        }
                    } else if(heading.AsXmlNode.InnerText.Trim().EqualsInvariantIgnoreCase(sectionName)) {
                        startLevel = int.Parse(heading.Name.Substring(1)) - 1;
                        startNode = heading.AsXmlNode;
                    }
                }
            }
        }

        private static void FindSection(XDoc xhtml, int sectionIndex, out XmlNode startNode, out XmlNode endNode) {
            startNode = null;
            endNode = null;
            int currentSection = 0;

            // locate the start and end nodes of the section
            int startLevel = 0;
            foreach(XDoc heading in xhtml[ALL_HEADINGS_XPATH]) {
                if(!ExcludedTags.Contains(xhtml.AsXmlNode.ParentNode, true)) {
                    currentSection++;
                    if(null != startNode) {
                        if(startLevel >= int.Parse(heading.Name.Substring(1)) - 1) {
                            endNode = heading.AsXmlNode;
                            break;
                        }
                    } else if(currentSection == sectionIndex) {
                        startLevel = int.Parse(heading.Name.Substring(1)) - 1;
                        startNode = heading.AsXmlNode;
                    }
                }
            }
        }

        private static XDoc ExtractSection(bool includeStartNode, XmlNode startNode, XmlNode endNode) {
            XDoc result = new XDoc("body");
            if(null != startNode) {

                // extract all nodes in the section range and add them to the result
                XmlNode resultNode = result.AsXmlNode;
                if(!includeStartNode) {
                    startNode = startNode.NextSibling;
                }
                for(XmlNode currentNode = startNode; currentNode != null && currentNode != endNode; currentNode = currentNode.NextSibling) {
                    resultNode.AppendChild(resultNode.OwnerDocument.ImportNode(currentNode, true));
                }
            }
            return result;
        }

        private static XDoc CreateParserDocument(PageBE page, string contentType, string language, string content, ParserMode mode) {

            // create the page xml doc used for processing
            XDoc html;
            switch(contentType) {
            case DekiMimeType.HTML_TEXT:
                html = XDocFactory.From(content, MimeType.HTML);
                break;
            case DekiMimeType.DEKI_TEXT:
            case DekiMimeType.DEKI_XML0702:
                html = XDocFactory.From(string.Format("<html><body>{0}</body></html>", content), MimeType.HTML);
                break;
            case DekiMimeType.DEKI_XML0805:
                html = XDocFactory.From(string.Format("<page><content><body xmlns:eval=\"http://mindtouch.com/2007/dekiscript\">{0}</body></content></page>", content), MimeType.XML);
                if((html != null) && !html.IsEmpty) {
                    html.Attr("id", page.ID)
                        .Elem("title", page.Title.AsUserFriendlyName())
                        .Elem("path", page.Title.AsPrefixedDbPath());
                    html["content"].Attr("type", contentType).Attr("lang", language);
                    return html;
                }
                break;
            default:
                html = new XDoc("html").Start("body").Start("pre").Value(content).End().End();
                break;
            }

            // check if we failed to parse the contents of the page
            if((html == null) || html.IsEmpty) {
                switch(mode) {
                case ParserMode.EDIT:
                case ParserMode.VIEW:
                case ParserMode.VIEW_NO_EXECUTE:
                    contentType = MimeType.TEXT.FullType;
                    html = new XDoc("html").Elem("body", content);
                    break;
                case ParserMode.RAW:
                case ParserMode.SAVE:
                default:
                    throw new FormatException(DekiResources.PAGE_FORMAT_INVALID);
                }
            }

            // convert contents if need be
            if((mode != ParserMode.RAW) && !page.IsRedirect) {
                if(contentType.EqualsInvariant(DekiMimeType.DEKI_TEXT)) {
                    WikiConverter_TextToXml.Convert(html["body"]);
                    contentType = DekiMimeType.DEKI_XML0702;
                }
            }

            // prepare page document
            XDoc result = new XDoc("page").Attr("id", page.ID)
                .Elem("title", page.Title.AsUserFriendlyName())
                .Elem("path", page.Title.AsPrefixedDbPath())
                .Start("content").Attr("type", contentType).Attr("lang", language).Add(html["body"]).End();
            return result;
        }

        private static object ProcessRedirect(XDoc xhtml, ParserMode mode) {
            Title redirectTitle = null;
            XUri redirectUri = null;
            XDoc mainBody = xhtml[MAIN_BODY_XPATH];
            Match redirectMatch = REDIRECT_REGEX.Match(mainBody.AsXmlNode.InnerText.Trim());

            // determine whether the page contents conforms to the redirect format
            if (redirectMatch.Success) {
                Group group = redirectMatch.Groups["page"];
                string pageRedirectText = group.Success ? group.Value : null;
                string siteRedirectText = redirectMatch.Groups["uri"].Value;

                // explicitly disallow the following redirect
                if(pageRedirectText != null) {
                    if(!pageRedirectText.EqualsInvariantIgnoreCase("Special:Userlogout")) {
                        redirectTitle = Title.FromUIUri(null, pageRedirectText, false);
                        xhtml["content"].Attr("type", DekiMimeType.DEKI_XML0702);
                    }
                } else if(!string.IsNullOrEmpty(siteRedirectText)) {
                    redirectUri = XUri.TryParse(siteRedirectText);
                }
            }

            // if in view mode and the page is a redirect, replace the page's content with a redirection message 
            if ((ParserMode.VIEW == mode || ParserMode.VIEW_NO_EXECUTE == mode)) {
                if(redirectTitle != null) {
                    PageBE page = PageBL.GetPageByTitle(redirectTitle);

                    // replace placeholder in redirect text with destination link
                    string link = String.Format(
                        "<a rel=\"internal\" {0} href=\"{1}\">{2}</a>", 
                        (page.ID == 0) ? "class=\"new\"" : string.Empty, 
                        Utils.AsPublicUiUri(page.Title), 
                        redirectTitle.AsPrefixedUserFriendlyPath()
                    );
                    string redirectText = string.Format((page.ID == 0) ? DekiResources.REDIRECTED_TO_BROKEN : DekiResources.REDIRECTED_TO, link);

                    // replace page contents
                    mainBody.Replace(XDocFactory.From("<html><body><div class=\"redirectedTo\"><span>" + redirectText + "</span></div></body></html>", MimeType.HTML)["body"]);
                } else if(redirectUri != null) {

                    // replace placeholder in redirect text with destination link
                    string link = String.Format("<a rel=\"external\" href=\"{0}\">{0}</a>", redirectUri);
                    string redirectText = string.Format(DekiResources.REDIRECTED_TO, link);

                    // check if redirects are allowed
                    if(DreamContext.Current.GetParam(DekiWikiService.PARAM_REDIRECTS, int.MaxValue) > 0) {

                        // add auto-refesh meta tag
                        XDoc head = xhtml["content/head"];
                        if(head.IsEmpty) {
                            xhtml["content"].Elem("head");
                            head = xhtml["content/head"];
                        }
                        head.Start("meta").Attr("http-equiv", "refresh").Attr("content", "0;url=" + redirectUri).End();
                    }

                    // replace page contents
                    mainBody.Replace(DekiScriptLibrary.WebHtml("<div class=\"redirectedTo\"><span>" + redirectText + "</span></div>", null, null, null)["body"]);
                }
            }
            return redirectTitle ?? (object)redirectUri;
        }

        private static void ProcessPage(PageBE page, XDoc content, ParserMode mode, bool isInclude, PageBE basePage, DekiScriptLiteral args, Title relToTitle, out XDoc tableOfContents, out bool hasScriptContent, out List<Title> outboundLinks, out List<Title> templates, out List<string> tags) {
            DekiXmlParser parser = new DekiXmlParser(page, content, mode, isInclude, basePage, args, relToTitle);

            // 1) process structure of document
            parser.ProcessStructure();
            tags = parser.ProcessTagsToBeInserted();

            // 2) process edit sections
            parser.ProcessEditSections();

            // 3) process links to attachments
            parser.ProcessAttachmentLinks();

            // 4) process script content (variables and functions)
            hasScriptContent = parser.ProcessScripts();

            // 5) process free-form external links
            parser.ProcessFreeLinks();

            // 6) process internal/external links
            parser.ProcessLinks(out outboundLinks, out templates);

            // 7) process images
            parser.ProcessImages();

            // 8) auto-number links
            parser.AutoNumberLinks();

            // 9) process nowiki tags
            parser.ProcessNoWiki();

            // 10) process words (banned & highlighted)
            parser.ProcessWords();

            // 11) process headings
            tableOfContents = parser.ProcessHeadings();

            // adjust content type
            if(mode == ParserMode.SAVE) {
                content["content/@type"].ReplaceValue(DekiMimeType.DEKI_XML0805);
            } else if(mode == ParserMode.EDIT) {
                content["content/@type"].ReplaceValue(DekiMimeType.DEKI_TEXT);
            } else if((mode == ParserMode.VIEW_NO_EXECUTE) || (mode == ParserMode.VIEW)) {
                content["content/@type"].ReplaceValue(DekiMimeType.HTML_TEXT);
            }
        }

        private static void ProcessWhitespace(XmlNode node) {
            switch (node.NodeType) {
                case XmlNodeType.Document:
                    ProcessWhitespace(((XmlDocument)node).DocumentElement);
                    break;
                case XmlNodeType.Element:
                    if (node.Name != "pre") {
                        foreach (XmlNode child in node.ChildNodes) {
                            ProcessWhitespace(child);
                        }
                    }
                    break;
                case XmlNodeType.Text:
                    node.Value = WHITESPACE.Replace(node.Value, delegate { return " "; });
                    break;
                case XmlNodeType.Whitespace:
                    node.Value = WHITESPACE.Replace(node.Value, delegate { return " "; });
                    break;
            }
        }

        private static Dictionary<string, string> ParseStyles(string styles) {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (styles != null) {
                foreach (string style in styles.Split(';')) {
                    if (!string.IsNullOrEmpty(style.Trim())) {
                        string[] values = style.Split(new[] { ':' }, 2);
                        result[values[0].Trim().ToLowerInvariant()] = values[1].Trim();
                    }
                }
            }
            return result;
        }

        private static bool ResolveLink(Title baseTitle, string srcLink, out Title title) {

            // check if the link is a file (FILE:<path>/<filename>)
            Title originalTitle = Title.FromUIUri(baseTitle, srcLink);
            if (originalTitle.IsFile) {

                // lookup the page and its redirects
                // TODO (brigettek):  potential vulnerability.  The title is resolved without verifying that the user has browse permission to it. 
                PageBE redirectPage = PageBL.ResolveRedirects(PageBL.GetPageByTitle(originalTitle));
                title = redirectPage.Title;
                title.Filename = originalTitle.Filename;
                return true;
            }
            title = null;
            return false;
        }

        private static Title GetTitleFromFileId(uint fileid) {
            if (0 < fileid) {
                uint resourceId = ResourceMapBL.GetResourceIdByFileId(fileid) ?? 0;
                if (0 < resourceId) {
                    AttachmentBE file = AttachmentBL.Instance.GetResource(resourceId, ResourceBE.HEADREVISION);
                    if (null != file) {
                        PageBE page = PageBL.GetPageById(file.ParentPageId);
                        if ((null != page) && (0 < page.ID)) {
                            Title result = page.Title;
                            result.Filename = file.Name;
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        private static Title[] GetFileAlternates(Title baseTitle, Title title) {
            List<Title> alternates = new List<Title>();

            // look for the file using exactly the information provided 
            alternates.Add(title);

            // if the string has an empty path, attempt to look for the file on the current page
            if (!baseTitle.IsHomepage && title.IsHomepage) {
                alternates.Add(Title.FromDbPath(baseTitle.Namespace, baseTitle.AsUnprefixedDbPath(), null, title.Filename));
            }

            return alternates.ToArray();
        }

        private static XDoc ParseContent(string content, string mime) {
            switch (mime) {
                case DekiMimeType.HTML_TEXT:
                    return XDocFactory.From(content, MimeType.HTML);
                case DekiMimeType.DEKI_TEXT:
                case DekiMimeType.DEKI_XML0702:
                    return XDocFactory.From(string.Format("<html><body xmlns:eval=\"http://mindtouch.com/2007/dekiscript\">{0}</body></html>", content), MimeType.HTML);
                case DekiMimeType.DEKI_XML0805:
                    return XDocFactory.From(string.Format("<html><body xmlns:eval=\"http://mindtouch.com/2007/dekiscript\">{0}</body></html>", content), MimeType.XML);
                default:
                    return new XDoc("html").Start("body").Start("pre").Value(content).End().End();
            }
        }

        private static bool IsHeading(XDoc doc) {
            var node = doc.AsXmlNode;
            if(node.NodeType != XmlNodeType.Element) {
                return false;
            }
            var name = node.Name;
            return (name.Length == 2) && (name[0] == 'h') && (name[1] >= '1') && (name[1] <= '6');
        }

        private static bool IsFreeLink(XDoc doc) {
            var node = doc.AsXmlNode;
            if(node.NodeType != XmlNodeType.Element) {
                return false;
            }
            var name = node.Name;
            if((name.Length != 1) || (name[0] != 'a') || !doc.HasAttr("href")) {
                return false;
            }
            XDoc rel = doc["@rel"];
            return rel.IsEmpty || rel.AsText.EqualsInvariant("freelink");
        }

        private static bool IsInclusionSpan(XDoc doc) {
            var node = doc.AsXmlNode;
            if(node.NodeType != XmlNodeType.Element) {
                return false;
            }
            var name = node.Name;
            return (name.Length == 4) && (name[0] == 's') && (name[1] == 'p') && (name[2] == 'a') && (name[3] == 'n') && doc.HasAttr("pagelink");
        }

        //--- Fields ---
        private readonly PageBE _page;
        private readonly XDoc _content; 
        private readonly ParserMode _mode;
        private readonly bool _isInclude;
        private readonly PageBE _basePage;
        private readonly DekiScriptLiteral _args;
        private readonly Title _relToTitle;

        //--- Constructors ---
        private DekiXmlParser(PageBE page, XDoc content, ParserMode mode, bool isInclude, PageBE basePage, DekiScriptLiteral args, Title relToTitle) {
            this._page = page;
            this._content = content;
            this._mode = mode;
            this._isInclude = isInclude;
            this._basePage = basePage;
            this._args = args;
            this._relToTitle = relToTitle;
        }

        //--- Methods ---
        private DekiScriptEnv ProcessEnvironment() {
            var current = DreamContext.Current;
            DekiScriptEnv env = current.GetState<DekiScriptEnv>("pageenv-" + _basePage.ID);

            // check if we already have an initialized environment
            if(env == null) {

                // create environment
                env = ExtensionBL.CreateEnvironment(_basePage);

                // add request arguments
                DekiScriptMap request = new DekiScriptMap();
                DekiScriptMap queryArgs = new DekiScriptMap();
                DreamContext context = DreamContext.CurrentOrNull;
                if(context != null) {
                    if(context.Uri.Params != null) {
                        foreach(KeyValuePair<string, string> query in context.Uri.Params) {

                            // check that query parameter doesn't have 'dream.' prefix
                            if(!query.Key.StartsWithInvariantIgnoreCase("dream.")) {
                                DekiScriptLiteral value;

                                // check if a query parameter with the same name was alreayd found
                                if(queryArgs.TryGetValue(query.Key, out value)) {
                                    DekiScriptList list = value as DekiScriptList;

                                    // check if the current value is already a list
                                    if(list == null) {
                                        list = new DekiScriptList();
                                        list.Add(value);

                                        // replace current value
                                        queryArgs.Add(query.Key, list);
                                    }

                                    // append new value to list
                                    list.Add(DekiScriptExpression.Constant(query.Value));
                                } else {

                                    // add query parameter
                                    queryArgs.Add(query.Key, DekiScriptExpression.Constant(query.Value));
                                }
                            }
                        }
                    }

                    // show User-Agent, Referer and Host information in __request
                    request.Add("referer", DekiScriptExpression.Constant(context.Request.Headers.Referer));
                    request.Add("useragent", DekiScriptExpression.Constant(context.Request.Headers.UserAgent));
                    request.Add("host", DekiScriptExpression.Constant(context.Request.Headers.Host));

                    // parse form fields from a request for requests with a mimetype of application/x-www-form-urlencoded
                    DekiScriptMap postParams;
                    if(context.Request.ContentType.Match(MimeType.FORM_URLENCODED)) {
                        KeyValuePair<string, string>[] formFields = XUri.ParseParamsAsPairs(context.Request.ToText());
                        if(!ArrayUtil.IsNullOrEmpty(formFields)) {
                            postParams = new DekiScriptMap();
                            foreach(KeyValuePair<string, string> kvp in formFields) {
                                postParams.Add(kvp.Key, DekiScriptExpression.Constant(kvp.Value));
                            }
                            request.Add("fields", postParams);
                        }
                    }
                }
                request.Add("args", queryArgs);
                env.Vars.Add("__request", request);

                // store computed environment for subsequent calls
                current.SetState("pageenv-" + _basePage.ID, env);
            }
            env = env.NewScope();

            // global processing variables
            env.Vars.Add("__include", DekiScriptExpression.Constant(_isInclude));
            env.Vars.Add("__mode", DekiScriptExpression.Constant(_mode.ToString().ToLowerInvariant()));
            env.Vars.Add(DekiScriptEnv.SAFEMODE, DekiScriptExpression.Constant(ExtensionRuntime.IsSafeMode(_page)));

            // set processing settings
            DekiScriptMap settings = new DekiScriptMap();
            settings.Add("nofollow", DekiScriptExpression.Constant(DekiContext.Current.Instance.WebLinkNoFollow));
            env.Vars.Add(DekiScriptEnv.SETTINGS, settings);

            // add callstack (specific for each page invocation)
            DekiScriptList callstack = new DekiScriptList();
            ParserState parseState = GetParseState();
            if(parseState != null) {
                foreach(PageBE includingPage in parseState.ProcessingStack) {
                    callstack.Add(DekiScriptExpression.Constant(includingPage.Title.AsPrefixedDbPath()));
                }
                env.Vars.Add(DekiScriptEnv.CALLSTACK, callstack);
            }

            // add arguments environment vars
            env.Vars.Add("args", _args ?? DekiScriptNil.Value);
            env.Vars.Add("$", _args ?? DekiScriptNil.Value);
            return env.NewScope();
        }

        private void ProcessStructure() {
            XDoc mainBody = _content[MAIN_BODY_XPATH];
            bool processComments = ((!_page.Title.IsTemplate || _isInclude) && (ParserMode.VIEW == _mode || ParserMode.VIEW_NO_EXECUTE == _mode));
            bool processInclude = _isInclude || (_mode == ParserMode.VIEW) || (_mode == ParserMode.VIEW_NO_EXECUTE);
            bool processLanguage = (_page.Title.IsTemplate && _isInclude);

            // TODO (steveb): why are we fetching the main culture from the current page, but the complete culture info from the base page?
            string primaryLanguage = _content["content/@lang"].Contents.Split(new[] { '-' }, 2)[0];
            string fullLanguage = _basePage.Language;

            bool hasAnyLanguage = false;
            bool hasLanguageMatch = false;
            List<XmlNode> wildcardLangNodes = null;

            // loop over document to adjust structure
            Stack<XmlNode> stack = new Stack<XmlNode>(16);
            XmlNode current = mainBody.AsXmlNode.FirstChild;
            while(current != null) {
                XmlNode next = null;

                // check children
                if(current.NodeType == XmlNodeType.Element) {
                    XmlElement elem = (XmlElement)current;

                    // check attributes
                    string cls = elem.GetAttribute("class");
                    if(cls.Length > 0) {
                        if(processComments && cls.EqualsInvariant("comment")) {

                            // remove comment blocks in view mode
                            next = current.NextSibling;
                            current.ParentNode.RemoveChild(current);
                            current = null;
                        } else if(processInclude) {
                            if(_isInclude) {

                                // check if a 'onlyinclude' section exists
                                if(cls.EqualsInvariant("onlyinclude")) {

                                    // replace main body with the contents of the 'onlyinclude' section
                                    XDoc newBody = _content[current];
                                    mainBody.RemoveNodes();
                                    mainBody.AddNodes(newBody);
                                    stack.Clear();
                                    next = mainBody.AsXmlNode.FirstChild;
                                    current = null;
                                } else if(cls.EqualsInvariant("noinclude")) {

                                    // remove all 'noinclude' sections
                                    next = current.NextSibling;
                                    current.ParentNode.RemoveChild(current);
                                    current = null;
                                } else if(cls.EqualsInvariant("includeonly")) {
                                    XmlNode parent = current.ParentNode;
                                    XmlNode prev = current.PreviousSibling;

                                    // remove all 'includeonly' wrappers
                                    XDoc includeonly = _content[current];
                                    includeonly.ReplaceWithNodes(includeonly);
                                    next = (prev != null) ? prev.NextSibling : parent.FirstChild;
                                    current = null;
                                }
                            } else {

                                // check if a 'onlyinclude' section exists
                                if(cls.EqualsInvariant("includeonly")) {

                                    // remove all 'includeonly' sections
                                    next = current.NextSibling;
                                    current.ParentNode.RemoveChild(current);
                                    current = null;
                                } else if(cls.EqualsInvariant("noinclude") || cls.EqualsInvariant("onlyinclude")) {
                                    XmlNode parent = current.ParentNode;
                                    XmlNode prev = current.PreviousSibling;

                                    // remove all 'noinclude' and 'onlyinclude' wrappers
                                    XDoc noinclude = _content[current];
                                    noinclude.ReplaceWithNodes(noinclude);
                                    next = (prev != null) ? prev.NextSibling : parent.FirstChild;
                                    current = null;
                                }
                            }
                        }
                    }

                    // process language information
                    if((current != null) && processLanguage) {

                        // check if element has a 'lang' attribute
                        XmlAttribute langAttribute = elem.GetAttributeNode("lang");
                        if(langAttribute != null) {
                            string lang = langAttribute.Value;
                            elem.RemoveAttributeNode(langAttribute);
                            hasAnyLanguage = true;
                            if(lang.EqualsInvariantIgnoreCase("*")) {
                                if(hasLanguageMatch) {

                                    // remove all wildcard lang-nodes since we already found a matching language element
                                    next = current.NextSibling;
                                    current.ParentNode.RemoveChild(current);
                                    current = null;
                                } else {

                                    // keep reference for later, in case we need to delete the wildcard nodes
                                    if(wildcardLangNodes == null) {
                                        wildcardLangNodes = new List<XmlNode>(32);
                                    }
                                    wildcardLangNodes.Add(current);
                                }
                            } else {
                                if(!lang.EqualsInvariantIgnoreCase(primaryLanguage) && !lang.EqualsInvariantIgnoreCase(fullLanguage)) {

                                    // remove all non-matching language nodes
                                    next = current.NextSibling;
                                    current.ParentNode.RemoveChild(current);
                                    current = null;
                                } else {

                                    // we found a language match (remove wildcard nodes!)
                                    hasLanguageMatch = true;
                                }
                            }
                        }
                    }

                    // explore children nodes, if any
                    if(current != null) {

                        // determine next node
                        next = current.FirstChild;
                        if(next != null) {

                            // found a child node, safe keep next sibling of current node for when we're done with the children nodes
                            stack.Push(current.NextSibling);
                        } else {

                            // no children, go on with the next sibling node
                            next = current.NextSibling;
                        }
                    }
                } else {
                    next = current.NextSibling;
                }

                // go to next node
                current = next;
                while((current == null) && (stack.Count > 0)) {
                    current = stack.Pop();
                }
            }

            // remove language elements that don't fit the current language; process the page only under the followind conditions
            // 1) for templates, if it's included and being viewed
            // 2) for non-templates, if it's being saved
            if(processLanguage && hasAnyLanguage && hasLanguageMatch) {

                // remove all wildcard language nodes
                if(wildcardLangNodes != null) {
                    for(int i = 0; i < wildcardLangNodes.Count; ++i) {
                        XmlNode node = wildcardLangNodes[i];
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }

            // adjust content
            if((_mode == ParserMode.SAVE) || (((_mode == ParserMode.VIEW) || (_mode == ParserMode.VIEW_NO_EXECUTE)) && (_content["content/@type"].Contents == DekiMimeType.DEKI_XML0702))) {

                // remove dangling <br/> from <hX><br /></hX> blocks
                foreach(XDoc heading in mainBody[ALL_HEADINGS_XPATH]) {
                    XDoc br = heading["br[last()]"];
                    if(!br.IsEmpty) {

                        // check if any text is following the <br/> element
                        XmlNode node = br.AsXmlNode.NextSibling;
                        bool keep = false;
                        while(node != null) {
                            if(!((node is XmlText) || (node is XmlWhitespace)) || !string.IsNullOrEmpty(node.Value.Trim())) {
                                keep = true;
                                break;
                            }
                            node = node.NextSibling;
                        }
                        if(!keep) {
                            br.Remove();
                        }
                    }
                }

                // remove <p><br/></p> nodes
                foreach(XDoc paragraph in mainBody[".//p[count(br)=1]"]) {
                    XmlElement node = (XmlElement)paragraph.AsXmlNode;
                    if((node.Attributes.Count == 0) && (node.ChildNodes.Count == 0)) {
                        paragraph.Remove();
                    }
                }

                // convert class 'live' to 'script'
                foreach(XDoc node in mainBody[".//*[@class='live']"]) {
                    node["@class"].ReplaceValue("script");
                }
            }

            // remove unncessary whitespace
            if(ParserMode.EDIT == _mode || ParserMode.SAVE == _mode) {
                ProcessWhitespace(_content.AsXmlNode);
            }
        }

        private List<string> ProcessTagsToBeInserted() {
            List<string> tags = null;
            if(ParserMode.SAVE == _mode) {
                foreach(var tagP in _content[".//p[@class='template:tag-insert']"]) {
                    foreach(var tag in tagP["a"]) {
                        if(tags == null) {
                            tags = new List<string>();
                        }
                        tags.Add(tag.Contents);
                    }
                    tagP.Remove();
                }
            }
            return tags;
        }

        private void ProcessEditSections() {
            if (!_isInclude) {
                XDoc mainBody = _content[MAIN_BODY_XPATH];
                if (ParserMode.VIEW == _mode) {
                    XmlDocument owner = mainBody.AsXmlNode.OwnerDocument;

                    // process headings by layer (all h1's, then all h2's, etc)
                    for (int currentHeadingLevel = 1; currentHeadingLevel < 7; currentHeadingLevel++) {

                        // process each heading in the current layer
                        XDoc headings = mainBody[".//h" + currentHeadingLevel];
                        foreach (XDoc heading in headings) {
                            if (!ExcludedTags.Contains(heading.AsXmlNode.ParentNode, true)) {
                                heading.Attr("class", "editable");

                                // create the div node 
                                XmlNode divNode = owner.CreateNode(XmlNodeType.Element, "div", null);
                                heading.AsXmlNode.ParentNode.InsertBefore(divNode, heading.AsXmlNode);

                                // move the heading and all subsequent sibling nodes into the div tag until the next heading is reached
                                for (XmlNode currentNode = heading.AsXmlNode; currentNode != null && currentNode != heading.Next.AsXmlNode; ) {
                                    XmlNode nextSibling = currentNode.NextSibling;
                                    divNode.AppendChild(currentNode);
                                    currentNode = nextSibling;
                                }
                            }
                        }
                    }

                    // set @id for all headings
                    int sectionCounter = 1;
                    foreach (XDoc heading in mainBody[ALL_HEADINGS_XPATH]) {
                        if (!ExcludedTags.Contains(heading.AsXmlNode.ParentNode, true)) {
                            XmlNode divNode = heading.AsXmlNode.ParentNode;
                            XmlAttribute attributeNode = owner.CreateAttribute("id");
                            attributeNode.Value = "section_" + (sectionCounter++);
                            divNode.Attributes.Append(attributeNode);
                        }
                    }
                }
            }
        }

        private bool ProcessScripts() {
            DekiScriptEnv env = ProcessEnvironment();
            bool passive = _page.Title.IsTemplate && !_isInclude;
            bool result = false;
            DekiScriptEnv prevEnv = DreamContext.Current.GetState<DekiScriptEnv>();
            try {
                DreamContext.Current.SetState(env);
                ExtensionBL.InitializeCustomDekiScriptHeaders(_basePage);
                if(_mode == ParserMode.SAVE) {
                    EvaluateContent(new XDekiScript(_content["content"]), env, passive ? DekiScriptEvalMode.None : DekiScriptEvalMode.EvaluateSaveOnly, out result);
                } else if(_mode == ParserMode.VIEW) {
                    if(!passive) {
                        DekiScriptEvalMode eval = env.IsSafeMode ? DekiScriptEvalMode.EvaluateSafeMode : DekiScriptEvalMode.Evaluate;
                        EvaluateContent(new XDekiScript(_content["content"]), env, eval, out result);
                    } else {
                        EvaluateContent(new XDekiScript(_content["content"]), env, DekiScriptEvalMode.Verify, out result);
                    }
                } else if((_mode == ParserMode.EDIT) && !passive) {
                    EvaluateContent(new XDekiScript(_content["content"]), env, DekiScriptEvalMode.EvaluateEditOnly, out result);
                }
            } finally {
                DreamContext.Current.SetState(prevEnv);
            }

            // convert <span class="script"> if needed
            if((ParserMode.VIEW_NO_EXECUTE == _mode) || (ParserMode.VIEW == _mode && _page.Title.IsTemplate && !_isInclude)) {

                // TODO (brigettek):  for now, the editor needs to display script blocks in brackets, but this should be migrated to the new script block format
                foreach(XDoc code in _content["content/body//span[@class='script']"]) {
                    code.Replace(new XDoc("span").Attr("class", "plain").Value("{{").AddNodes(code).Value("}}"));
                }
            } else if(ParserMode.EDIT == _mode) {

                // TODO (brigettek):  for now, the editor needs to display script blocks in brackets, but this should be migrated to the new script block format
                foreach(XDoc code in _content["content/body//span[@class='script']"]) {
                    code.ReplaceWithNodes(new XDoc("span").Value("{{").AddNodes(code).Value("}}"));
                }
            }
            return result;
        }

        public void EvaluateContent(XDoc script, DekiScriptEnv env, DekiScriptEvalMode mode, out bool scripted) {
            scripted = false;
            switch(mode) {
            case DekiScriptEvalMode.None:
                break;
            case DekiScriptEvalMode.Evaluate:
            case DekiScriptEvalMode.EvaluateSafeMode: {
                    var expr = DekiScriptParser.Parse(script);
                    try {
                        expr = DekiContext.Current.Deki.InternalScriptRuntime.Evaluate(expr, mode, env);
                    } catch(Exception e) {
                        expr = DekiScriptExpression.Constant(DekiScriptLibrary.WebShowError((Hashtable)DekiScriptLibrary.MakeErrorObject(e, env).NativeValue));
                    }

                    // check if outcome is an XML document
                    var xml = expr as DekiScriptXml;
                    if(xml != null) {

                        // check if outcome is the expected content document
                        if(xml.Value.HasName("content")) {
                            script.Replace(((DekiScriptXml)expr).Value);
                        } else {

                            // remove all contents from existing document and append new document
                            script.RemoveNodes();
                            script.Start("body").Add(xml.Value).End();
                        }
                    } else if(expr is DekiScriptString) {

                        // remove all contents from existing document and append new document
                        script.RemoveNodes();
                        script.Start("body").Value(((DekiScriptString)expr).Value).End();
                    } else {

                        // remove all contents from existing document and append new document
                        script.RemoveNodes();
                        script.Start("body").Value(expr.ToString()).End();
                    }
                }
                break;
            case DekiScriptEvalMode.EvaluateEditOnly:
            case DekiScriptEvalMode.EvaluateSaveOnly:
            case DekiScriptEvalMode.Verify: {
                    DekiScriptEvalContext context = new DekiScriptEvalContext(script, mode, false);

                    // add <head> and <tail> sections
                    context.AddHeadElements(script);
                    context.AddTailElements(script);
                    script["head"].RemoveNodes();
                    script["tail"].RemoveNodes();

                    // evaluate the script
                    bool error = false;
                    DekiScriptInterpreter.Evaluate(script, (XmlElement)script.Root.AsXmlNode, context, env, DekiContext.Current.Deki.InternalScriptRuntime, out scripted, ref error);
                    if((mode == DekiScriptEvalMode.Verify) || !error) {
                        context.MergeContextIntoDocument(script.AsXmlNode.OwnerDocument);
                    }
                }
                break;
            default:
                throw new InvalidOperationException(string.Format("unrecognized evaluation mode '{0}'", mode));
            }
        }

        private void ProcessAttachmentLinks() {
            List<Title[]> files = new List<Title[]>();
            List<XDoc> linkNodes = new List<XDoc>();
            Title baseTitle = Title.FromPrefixedDbPath(_content["path"].Contents, null);

            // examine each image tag to indentify internal file links - needs to be url encoded to db name
            foreach(XDoc img in _content[".//img[@src]"]) {
                uint fileid = img["@fileid"].AsUInt ?? 0;
                if(fileid == 0) {
                    Title title;
                    if(ResolveLink(baseTitle, img["@src"].Contents.Split('&')[0], out title)) {

                        // if image was not found, make sure it has an '@alt' attribute
                        if(string.IsNullOrEmpty(img["@alt"].AsText)) {
                            img.Attr("alt", title.AsPrefixedUserFriendlyPath());
                        }
                        files.Add(GetFileAlternates(baseTitle, title));
                        linkNodes.Add(img);
                    }
                }

                // check if we need to prepend the public uri
                if((_mode == ParserMode.VIEW) || (_mode == ParserMode.VIEW_NO_EXECUTE)) {
                    string src = img["@src"].AsText;
                    if(src.StartsWithInvariantIgnoreCase("/@api/")) {
                        img.Attr("src", DreamContext.Current.PublicUri.AtAbsolutePath(src).ToString());
                    }
                }
            }

            // examine each link tag to identify internal file links
            foreach(XDoc fileLink in _content[".//a[@href]"]) {
                uint fileid = fileLink["@fileid"].AsUInt ?? 0;
                if(fileid == 0) {
                    Title title;
                    if(ResolveLink(baseTitle, fileLink["@href"].Contents, out title)) {
                        files.Add(GetFileAlternates(baseTitle, title));
                        linkNodes.Add(fileLink);
                    }
                }
            }

            // create a list containing no duplicates and alternate filenames
            List<Title> filesWithAlternates = new List<Title>();
            foreach(Title[] fileAlternates in files) {
                foreach(Title file in fileAlternates) {
                    if(!filesWithAlternates.Contains(file)) {
                        filesWithAlternates.Add(file);
                    }
                }
            }

            // lookup the file id for each link.
            Dictionary<Title, AttachmentBE> ids = AttachmentBL.Instance.GetFileResourcesByTitlesWithMangling(filesWithAlternates.ToArray());

            // create an alternate entry in the list of found files that ignores spaces/underscores 
            foreach(Title file in new List<Title>(ids.Keys)) {
                string filenameNoSpaces = file.Filename.Replace(' ', '_');
                Title alternateTitle = Title.FromDbPath(file.Namespace, file.AsUnprefixedDbPath(), null, filenameNoSpaces);
                if(!file.Filename.EqualsInvariant(filenameNoSpaces) && !ids.ContainsKey(alternateTitle)) {
                    ids.Add(alternateTitle, ids[file]);
                }
            }

            // lookup the fileid for each link
            for(int i = 0; i < linkNodes.Count; i++) {
                XDoc linkNode = linkNodes[i];
                uint fileId = 0;
                foreach(Title file in files[i]) {
                    AttachmentBE attachment;

                    // first attempt to find the fileid using an exact match.  If that doesn't work, attempt to match ignoring spaces/underscores
                    if(!ids.TryGetValue(file, out attachment)) {
                        string filenameNoSpaces = file.Filename.Replace(' ', '_');
                        if(!file.Filename.EqualsInvariant(filenameNoSpaces)) {
                            ids.TryGetValue(Title.FromDbPath(file.Namespace, file.AsUnprefixedDbPath(), null, filenameNoSpaces), out attachment);
                        }
                    }
                    if((attachment != null) && attachment.FileId.HasValue) {
                        fileId = attachment.FileId.Value;
                    }
                    if(0 < fileId) {

                        // if the file was found on the current page and no path was specified, update the link to a relative path
                        if(linkNode.HasName("a") && baseTitle.AsPrefixedDbPath().EqualsInvariantIgnoreCase(file.AsPrefixedDbPath())) {
                            String href = linkNode["@href"].Contents;
                            Match match = FILE_PATH_REGEX.Match(href);
                            if(!match.Groups["path"].Success) {
                                int index = match.Groups["filename"].Index;
                                linkNode.Attr("href", href.Insert(index, "./"));
                            }
                        }
                        break;
                    }
                }
                linkNodes[i].Attr("fileid", fileId);
            }
        }

        private void ProcessLinks(out List<Title> outboundLinks, out List<Title> templates) {
            Title baseTitle = Title.FromPrefixedDbPath(_content["path"].Contents, null);
            IList<Title> linksKnown = new List<Title>();
            IList<string> linksBroken = new List<string>();
            outboundLinks = new List<Title>();
            templates = new List<Title>();

            // process each link
            List<XDoc> lookupLinkDocs = new List<XDoc>();
            List<Title> lookupTitles = new List<Title>();
            Regex fileRegex = null;
            DekiContext deki = DekiContext.Current;

            var selector = from x in _content.VisitOnly(e => !ExcludedTags.Contains(e.AsXmlNode))
                           where IsFreeLink(x)
                           select x;
            foreach(XDoc addr in selector.ToList()) {

                // remove local prefixes from links
                if (!ProcessExternalLink(addr, _mode, deki)) {
                    Title lookupLink;

                    // create data-structures on-demand
                    if(fileRegex == null) {
                        fileRegex = new Regex(String.Format(FILE_API_PATTERN, DekiContext.Current.ApiUri.At("files").Path.TrimStart('/')), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                        // during a view, extract the link information needed to determine broken links
                        if(ParserMode.VIEW == _mode || ParserMode.VIEW_NO_EXECUTE == _mode) {
                            ulong pageid = _content["@id"].AsULong ?? 0;
                            if(pageid != 0) {
                                linksKnown = DbUtils.CurrentSession.Links_GetOutboundLinks(pageid).Select(e => e.Value).ToList();
                                linksBroken = DbUtils.CurrentSession.Links_GetBrokenLinks(pageid);
                                for(int i = 0; i < linksBroken.Count; i++) {
                                    linksBroken[i] = linksBroken[i].ToLowerInvariant();
                                }
                            }
                        }
                    }

                    ProcessInternalLink(fileRegex, addr, baseTitle, linksKnown, linksBroken, outboundLinks, templates, deki, out lookupLink);
                    if (null != lookupLink) {
                        lookupLinkDocs.Add(addr);
                        lookupTitles.Add(Title.FromDbPath(lookupLink.Namespace, lookupLink.Path, null));
                    }
                }
            }

            // for links that were not found in the known and broken links table, perform a batch lookup to determine whether they exist
            if(lookupTitles.Count > 0) {
                Dictionary<Title, PageBE> lookupPages = DbUtils.CurrentSession.Pages_GetByTitles(lookupTitles).AsHash(e => e.Title);
                for(int i = 0; i < lookupLinkDocs.Count; i++) {
                    if(!lookupPages.ContainsKey(lookupTitles[i])) {
                        lookupLinkDocs[i].Attr("class", String.Join(" ", new[] { lookupLinkDocs[i]["@class"].AsText, "new" }));
                    }
                }
            }

            // convert cross-links to in-page links where appropriate
            if((_mode == ParserMode.VIEW) || (_mode == ParserMode.VIEW_NO_EXECUTE)) {
                XDoc internalLinks = null;
                selector = from x in _content.VisitOnly(e => !ExcludedTags.Contains(e.AsXmlNode))
                           where IsInclusionSpan(x)
                           select x;
                foreach(var inclusion in selector.ToList()) {

                    // read pagelink information
                    var pagelinkAttr = inclusion["@pagelink"];
                    var pagelink = pagelinkAttr.AsText;
                    pagelinkAttr.Remove();

                    // find all links that might be pointing here
                    if(internalLinks == null) {
                        internalLinks = _content[".//a[@rel='internal']"];
                    }
                    foreach(var link in internalLinks) {
                        var hrefAttr = link["@href"];
                        var href = hrefAttr.AsText;
                        if(href != null) {
                            if(href.EqualsInvariantIgnoreCase(pagelink)) {
                                hrefAttr.ReplaceValue("#" + inclusion["@id"].AsText);
                            } else if(href.StartsWithInvariantIgnoreCase(pagelink + "#")) {
                                hrefAttr.ReplaceValue(href.Substring(pagelink.Length));
                            }
                        }
                    }
                }
            }
        }

        private bool ProcessExternalLink(XDoc addr, ParserMode mode, DekiContext deki) {

            // first check whether this link has already been marked as internal
            if (addr["@class"].Contents.ContainsInvariant("internal")) {
                return false;
            } 
            bool isExternal = false;
            string link = addr["@href"].Contents.Trim();
            XUri xuri = XUri.TryParse(link);

            // check if the link is absolute, but points back into the wiki
            if((xuri != null) && xuri.HasPrefix(deki.UiUri) && !xuri.HasPrefix(deki.ApiUri) && (xuri.UserInfo == null)) {
                addr["@href"].ReplaceValue(xuri.PathQueryFragment);
                return false;
            }

            // try to parse the link as a uri
            string classAttribute = null;
            if (xuri != null) {
                isExternal = true;
                switch (xuri.Scheme.ToLowerInvariant()) {
                    case "https":
                        classAttribute = "link-https";
                        break;
                    case "news":
                        classAttribute = "link-news";
                        break;
                    case "mailto":
                        classAttribute = "link-mailto";
                        break;
                    case "ftp":
                        classAttribute = "link-ftp";
                        break;
                    case "irc":
                        classAttribute = "link-irc";
                        break;
                    default:
                        classAttribute = "external";
                        break;
                }
            } else {

                // check if the link is an email address 
                Match match = MAILTO_REGEX.Match(link);
                if (match.Success) {
                    int index = match.Value.IndexOf('@');
                    if ((0 < index) && (-1 < match.Value.IndexOf('.', index))) {
                        isExternal = true;
                        if (!match.Value.StartsWithInvariantIgnoreCase("mailto:")) {
                            addr["@href"].ReplaceValue("mailto:" + match.Value);
                        }
                        classAttribute = "link-mailto";
                    }
                }
            }

            // if the link is external, set its attributes accordingly
            if (isExternal) {
                addr.RemoveAttr("fileid");
                if (ParserMode.EDIT == mode) {
                    classAttribute = "external";
                } else if (ParserMode.VIEW == mode || ParserMode.VIEW_NO_EXECUTE == mode) {
                    if(deki.Instance.WebLinkNoFollow) {
                        addr.Attr("rel", "external nofollow");
                    } else {
                        addr.Attr("rel", "external");
                    }
                    addr.Attr("href", link);
                    if(addr["@title"].IsEmpty) {
                        addr.Attr("title", link);
                    }
                    string target = (deki.Instance.ExternalLinkTarget ?? "_blank").Trim();
                    if(addr["@target"].IsEmpty && (target.Length > 0)) {
                        addr.Attr("target", target);
                    }
                }

                // remove known class attribute and add new class attribute if needed
                List<string> attributes = new List<string>(addr["@class"].Contents.Split(' '));
                attributes.Remove("link-https");
                attributes.Remove("link-news");
                attributes.Remove("link-mailto");
                attributes.Remove("link-ftp");
                attributes.Remove("link-irc");
                attributes.Remove("external");
                if (!attributes.Contains(classAttribute)) {
                    attributes.Add(classAttribute);
                }
                XDoc cls = addr["@class"];
                if (cls.IsEmpty) {
                    addr.Attr("class", string.Join(" ", attributes.ToArray()));
                } else {
                    cls.ReplaceValue(string.Join(" ", attributes.ToArray()));
                }
            }
            return isExternal;
        }

        private void ProcessInternalLink(Regex fileRegex, XDoc addr, Title baseTitle, IList<Title> linksKnown, IList<string> linksBroken, List<Title> linksFound, List<Title> templates, DekiContext deki, out Title lookupLink) {
            lookupLink = null;
            XmlElement current = (XmlElement)addr.AsXmlNode;
            string template = current.GetAttribute("template");
            bool isTemplate = !String.IsNullOrEmpty(template);

            // if this is a link to an anchor on the current page, stop processing
            string href = current.GetAttribute("href").Trim();
            if ((href.StartsWithInvariant("#") && !isTemplate) || (href.StartsWithInvariant("{{") && href.EndsWithInvariant("}}"))) {
                return;
            }

            // if this is a link to a subpage from an imported template, process it
            if ((ParserMode.SAVE == _mode) && isTemplate) {
                addr.RemoveAttr("template");
                addr.RemoveAttr("class");
                Title templateTitle = Title.FromUIUri(baseTitle, template, false);
                templates.Add(templateTitle);

                // update the link to be relative to the current page instead of the template
                string[] segments = templateTitle.AsUnprefixedDbSegments();
                segments[0] = ".";
                template = String.Join("/", segments);
                addr.Attr("href", template);
                href = template;
            }
            List<string> css = new List<string>();
            bool containsImage = !addr[".//img"].IsEmpty;
            uint fileid;
            Title title;

            // Extract the fileid and title information
            // If the link is of the form "@api/deki/files/{fileid}/={filename}, extract the fileid and filename from it.
            // Otherwise, use the full URL as the title.
            Match m = fileRegex.Match(XUri.Decode(href));
            if (m.Success) {
                UInt32.TryParse(m.Groups["fileid"].Value, out fileid);
                title = Title.FromUIUri(null, "File:/" + m.Groups["filename"].Value);
            } else {
                uint.TryParse(current.GetAttribute("fileid"), out fileid);
                title = Title.FromUIUri(baseTitle, href);
            }

            // If the link has no text provide a default value
            if (addr.Contents.Length == 0) {
                addr.Value(title.IsFile ? title.Filename : Title.FromUIUri(null, href).AsUserFriendlyName());
            }

            // add the db encoded title to the list of links to lookup
            if (!title.IsFile && title.IsEditable) {
                linksFound.Add(title);
            }
            switch (_mode) {
                case ParserMode.EDIT:

                    // if the title is a file, return a link in the form @api/deki/files/{fileid}/={filename}
                    // TODO (brigettek):  Append the query param
                    if (title.IsFile) {
                        if (null != _relToTitle) {

                            // Normalize file link if needed (for import/export)
                            Title fileTitle = GetTitleFromFileId(fileid);
                            if (null != fileTitle) {
                                addr.RemoveAttr("href");
                                addr.Attr("href.path", fileTitle.AsRelativePath(_relToTitle));
                                addr.Attr("href.filename", fileTitle.Filename);
                            }
                        } else {
                            if (0 == fileid) {
                                addr.Attr("href", title.AsEditorUriPath());
                            } else {
                                addr.Attr("href", deki.ApiUri.At("files", fileid.ToString(), Title.AsApiParam(title.Filename)).ToString());
                            }
                        }
                    } else {

                        // TODO (brigettek):  Prevent resolving redirects for pages with many links to improve performance
                        // TODO (brigettek):  potential vulnerability.  The title is resolved without verifying that the user has browse permission to it. 
                        PageBE redirectedPage = PageBL.ResolveRedirects(PageBL.GetPageByTitle(title));
                        redirectedPage.Title.Anchor = title.Anchor;
                        redirectedPage.Title.Query = title.Query;
                        title = redirectedPage.Title;
                        if (null != _relToTitle) {

                            // Normalize link if needed (for import/export)
                            addr.RemoveAttr("href");
                            addr.Attr("href.path", title.AsRelativePath(_relToTitle));
                            addr.Attr("href.anchor", title.Anchor);
                            addr.Attr("href.query", title.Query);
                        } else {
                            addr.Attr("href", title.AsEditorUriPath());
                        }
                    }
                    if (string.IsNullOrEmpty(current.GetAttribute("title"))) {
                        addr.Attr("title", title.IsFile ? title.Filename : title.AsPrefixedUserFriendlyPath());
                    }
                    addr.RemoveAttr("fileid");
                    break;
                case ParserMode.VIEW_NO_EXECUTE:
                case ParserMode.VIEW: {
                        string rel = addr["@rel"].AsText;
                        addr.Attr("rel", "internal");

                        // check if path was generated, if so, keep it as it is
                        if (string.IsNullOrEmpty(rel)) {

                            // check if path is a reference to current page and, if so, make it bold
                            ParserState parseState = GetParseState();
                            foreach (PageBE includingPage in parseState.ProcessingStack) {
                                XDoc item;
                                if((includingPage.Title == title && parseState.ProcessedPages.TryGetValue(GetParserCacheKey(includingPage, _mode), out item) && (item == null))) {
                                    XDoc strong = new XDoc("strong");
                                    foreach(XmlNode node in addr.AsXmlNode.ChildNodes) {
                                        strong.AsXmlNode.AppendChild(strong.AsXmlNode.OwnerDocument.ImportNode(node, true));
                                    }
                                    addr.Replace(strong);
                                    return;
                                }
                            }
                        }
                    }

                    // check if link goes to a file attachment
                    if (title.IsFile) {

                        // if the file does not exist, display a message accordingly
                        if (0 == fileid) {
                            css.Add("new");
                            if (!containsImage) {
                                addr.AddNodesBefore(DekiScriptRuntime.CreateWarningElement(null, "(" + string.Format(DekiResources.MISSING_FILE, title.AsPrefixedUserFriendlyPath()) + ")", null));
                            }
                        } else {
                            if (!containsImage) {
                                css.Add("iconitext-16");
                                css.Add("ext-" + title.Extension);
                            }
                            addr.Attr("href", deki.ApiUri.At("files", fileid.ToString(), Title.AsApiParam(title.Filename)));
                        }
                    } else {

                        // check if page exists by first inspecting the links table and, if not found, searching for the page title
                        if (title.IsEditable && !linksKnown.Contains(Title.FromDbPath(title.Namespace, title.Path, null))) {
                            if (linksBroken.Contains(title.AsPrefixedDbPath().ToLowerInvariant())) {
                                css.Add("new");
                            } else {
                                lookupLink = title;
                            }
                        }

                        // check if link goes to a user's page
                        if ((title.IsUser) && (1 == title.AsUnprefixedDbSegments().Length)) {
                            css.Add("link-user");
                        }

                        // Update the link to use the site uri
                        addr.Attr("href", Utils.AsPublicUiUri(title));
                    }

                    if (css.Count > 0 && !containsImage) {
                        css.Add(current.GetAttribute("class"));
                        addr.Attr("class", string.Join(" ", css.ToArray()));
                    }
                    if(string.IsNullOrEmpty(current.GetAttribute("title"))) {
                        addr.Attr("title", title.IsFile ? title.Filename : title.AsPrefixedUserFriendlyPath());
                    }
                    addr.RemoveAttr("fileid");
                    break;
            }
        }

        private void ProcessImages() {
            if (ParserMode.VIEW == _mode || ParserMode.EDIT == _mode || ParserMode.VIEW_NO_EXECUTE == _mode) {
                Regex fileRegex = null;

                // resolve each image file id into a url
                foreach (XDoc img in _content[".//img"]) {
                    if (!ExcludedTags.Contains(img.AsXmlNode.ParentNode, true)) {
                        uint fileid = img["@fileid"].AsUInt ?? 0;
                        img.RemoveAttr("fileid");
                        if (0 < fileid) {

                            // convert style to inline values
                            Dictionary<string, string> styles = ParseStyles(img["@style"].AsText);
                            string width;
                            if (styles.TryGetValue("width", out width) && !width.EqualsInvariantIgnoreCase("auto")) {
                                img.Attr("width", width);
                            }
                            string height;
                            if (styles.TryGetValue("height", out height) && !height.EqualsInvariantIgnoreCase("auto")) {
                                img.Attr("height", height);
                            }

                            // create the url used to retrieve the file
                            string filename = Title.FromUIUri(Title.FromDbPath(NS.MAIN, String.Empty, null), img["@src"].Contents.Split('&')[0]).Filename;
                            XUri src = DekiContext.Current.ApiUri.At("files", fileid.ToString(), Title.AsApiParam(filename));

                            // process the image width/height settings
                            Match widthMatch = SIZE_REGEX.Match(img["@width"].Contents.Trim());
                            Match heightMatch = SIZE_REGEX.Match(img["@height"].Contents.Trim());
                            if (widthMatch.Success || heightMatch.Success) {
                                src = src.With("size", "bestfit");
                                if (widthMatch.Success) {
                                    src = src.With("width", widthMatch.Groups["size"].Value);
                                }
                                if (heightMatch.Success) {
                                    src = src.With("height", heightMatch.Groups["size"].Value);
                                }
                            }

                            // add src and class attributes
                            img.Attr("src", src.ToString());
                            string classAttr = img["@class"].Contents;
                            if(!classAttr.ContainsInvariant("internal")) {
                                img.Attr("class", String.Join(" ", new[] { classAttr, "internal" }));
                            }

                            // remove unused attributess
                            img.RemoveAttr("size_type");
                            img.RemoveAttr("ialign");
                        }

                        // Normalize file link if needed (for import/export)
                        if ( (null != _relToTitle) && (ParserMode.EDIT == _mode) ) {
                            if (0 == fileid) {
                                if (null == fileRegex) {
                                    fileRegex = new Regex(String.Format(FILE_API_PATTERN, DekiContext.Current.ApiUri.At("files").Path.TrimStart('/')), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                                }
                                Match m = fileRegex.Match(XUri.Decode(img["@src"].Contents));
                                if (m.Success) {
                                    UInt32.TryParse(m.Groups["fileid"].Value, out fileid);
                                }
                            }
                            Title fileTitle = GetTitleFromFileId(fileid);
                            if (null != fileTitle) {
                                img.RemoveAttr("src");
                                img.Attr("src.path", fileTitle.AsRelativePath(_relToTitle));
                                img.Attr("src.filename", fileTitle.Filename);
                            }
                        }
                    }
                }
            }      
        }

        private void ProcessFreeLinks() {
            XDoc mainBody = _content[MAIN_BODY_XPATH];
            string contentType = _content["content/@type"].Contents;
            DekiContext deki = DekiContext.Current;
            if((_mode == ParserMode.SAVE) || (((_mode == ParserMode.VIEW) || (_mode == ParserMode.VIEW_NO_EXECUTE)) && (contentType == DekiMimeType.DEKI_XML0702))) {
                ProcessFreeLinks(mainBody.AsXmlNode, deki);
            } else if(_mode == ParserMode.EDIT) {

                // convert <a class="freelink"> back to it's "free" form
                foreach(XDoc a in mainBody[".//a[@rel='freelink']"]) {
                    string link = a["@href"].Contents.Trim();
                    if(link.StartsWithInvariant("mailto:")) {
                        link = a.AsText;
                    }
                    a.Replace(link);
                }
            }
        }

        private void ProcessFreeLinks(XmlNode root, DekiContext deki) {

            // check whether the root is an excluded tag
            if(!ExcludedTags.Contains(root) && !root.Name.EqualsInvariantIgnoreCase("a")) {
                for (XmlNode node = root.FirstChild; node != null; node = node.NextSibling) {

                    // seach for free links within text nodes
                    if (XmlNodeType.Text == node.NodeType) {

                        // concatenate adjacent text nodes together
                        while ((null != node.NextSibling) && (XmlNodeType.Text == node.NextSibling.NodeType)) {
                            node.Value += node.NextSibling.Value;
                            root.RemoveChild(node.NextSibling);
                        }

                        MatchCollection matches = URL_REGEX.Matches(node.Value);
                        foreach(Match m in matches) {
                            String link = m.Value;

                            // if the url contains an ending parenthesis without a balanced number of starting parenthesis, exclude the remainder of the url
                            int paranthesisCount = 0;
                            int parenthesisIndex = -1;
                            for (int j = 0; j < link.Length; j++) {
                                if ('(' == link[j]) {
                                    paranthesisCount++;
                                } else if (')' == link[j]) {
                                    if (0 == paranthesisCount) {
                                        parenthesisIndex = j;
                                        link = link.Substring(0, j);
                                    } else {
                                        paranthesisCount--;
                                    }
                                }
                            }

                            // (bug 7134) avoid making links out of incomplete links
                            XUri uri = XUri.TryParse(link);
                            if((uri != null) && string.IsNullOrEmpty(uri.Authority)) {
                                string pathQueryFragment = uri.PathQueryFragment;
                                if(string.IsNullOrEmpty(pathQueryFragment) || (pathQueryFragment == "/")) {
                                    continue;
                                }
                            }

                            // check if link text needs to be truncated
                            string text = link;
                            if(text.Length >= 54) {
                                text = link.Substring(0, 36) + "..." + link.Substring(link.Length - 14, 14);
                            }

                            // attempt to construct an external link and add it to the document
                            XDoc linkDoc = new XDoc("link").Start("a").Attr("rel", "freelink").Attr("href", link).Value(text).End();
                            if (ProcessExternalLink(linkDoc["a"], _mode, deki)) {
                                XmlText linkAndRemainingText = ((XmlText)node).SplitText(m.Index);
                                XmlText remainingText = linkAndRemainingText.SplitText(link.Length);
                                AddChildrenBefore(remainingText, linkDoc.AsXmlNode);
                                root.RemoveChild(linkAndRemainingText);
                                break;
                            } else if (0 <= parenthesisIndex) {
                                ((XmlText)node).SplitText(m.Index + parenthesisIndex + 1);
                                break;
                            }
                        }
                    } else if (XmlNodeType.Element == node.NodeType) {

                        // recursively process non-text nodes
                        ProcessFreeLinks(node, deki);
                    }
                }
            }
        }

        private void AutoNumberLinks() {
            if (!_isInclude) {
                XDoc mainBody = _content[MAIN_BODY_XPATH];

                // do auto-numbering
                int counter = 0;
                foreach (XDoc addr in mainBody[".//a"]) {
                    if (!ExcludedTags.Contains(addr.AsXmlNode.ParentNode, true)) {
                        string href = addr["@href"].Contents;
                        bool needsAutoNumbering = href.StartsWithInvariantIgnoreCase("http://") || href.StartsWithInvariantIgnoreCase("https://");

                        // save needs to undo the placeholder character added in edit mode
                        if (ParserMode.SAVE == _mode) {
                            if (needsAutoNumbering && (addr.Contents == "#")) {
                                addr.ReplaceValue(String.Empty);
                            }
                        } else {

                            // check if link has a label
                            if (addr.Contents.Length == 0) {
                                // If the link starts with http:// or https://, create auto-numbered reference tag (e.g. [1], [2], etc.)
                                if (needsAutoNumbering) {
                                    if (ParserMode.EDIT == _mode) {
                                        addr.Value("#");
                                    } else {
                                        // create auto-numbered reference tag (e.g. [1], [2], etc.)
                                        addr.Value(string.Format("[{0}]", ++counter));
                                    }
                                } else {
                                    addr.Value(href);
                                }
                            }
                        }
                    }
                }
            }
        }

        private XDoc ProcessHeadings() {
            XDoc result = null;
            if (!_isInclude) {
                XDoc mainBody = _content[MAIN_BODY_XPATH];
                if (ParserMode.VIEW == _mode || ParserMode.VIEW_NO_EXECUTE == _mode) {

                    // collect all headings from document
                    int minHeadingLevel = int.MaxValue;
                    List<KeyValuePair<int, XDoc>> headings = new List<KeyValuePair<int, XDoc>>();
                    Dictionary<String, int> anchorCounters = new Dictionary<string, int>();

                    // select all headings
                    var selector = from x in mainBody.VisitOnly(e => !ExcludedTags.Contains(e.AsXmlNode)) 
                                   where IsHeading(x) 
                                   select x;
                    foreach (XDoc heading in selector) {
                        int level = int.Parse(heading.Name.Substring(1));
                        minHeadingLevel = Math.Min(level, minHeadingLevel);
                        headings.Add(new KeyValuePair<int, XDoc>(level, heading));
                    }

                    // initialize array for heading numbering
                    int[] headingNumbering = new int[6] { 0, 0, 0, 0, 0, 0 };
                    int maxHeading = DekiContext.Current.Instance.MaxHeadingLevelForTableOfContents;

                    // create <div> document
                    result = new XDoc("toc");
                    if (headings.Count > 0) {
                        int prevLevel = minHeadingLevel;

                        // start the containing list here, api is generating some style
                        result.Start("ol").Attr("style", "list-style-type:none; margin-left:0px; padding-left:0px;");
                        foreach (KeyValuePair<int, XDoc> heading in headings) {
                            int anchorCount;
                            String anchorName = Title.AnchorEncode(heading.Value.AsXmlNode.InnerText);
                            String existingAnchorName = heading.Value["@name"].AsText;
                            heading.Value.RemoveAttr("name");

                            // if an existing anchor name exists that is different from the default anchor name, add it
                            if (!String.IsNullOrEmpty(existingAnchorName) && !existingAnchorName.EqualsInvariant(anchorName)) {
                                heading.Value.AddBefore(new XDoc("span").Attr("id", existingAnchorName));
                            }
                            if (anchorCounters.TryGetValue(anchorName, out anchorCount)) {
                                anchorCounters[anchorName] = ++anchorCount;
                                anchorName = anchorName + "_" + anchorCount;
                            } else {
                                anchorCounters[anchorName] = 1;
                            }

                            // Create the anchor on the page
                            heading.Value.AddBefore(new XDoc("span").Attr("id", anchorName));

                            // check if heading within the toc max heading limit
                            if (heading.Key <= maxHeading) {

                                // check how many <ol> elements we need to create
                                for (int level = prevLevel; level < heading.Key; ++level) {
                                    if (result.HasName("ol")) {
                                        result.Start("li");
                                    }
                                    result.Start("ol").Attr("style", "list-style-type:none; margin-left:0px; padding-left:15px;"); // api styles
                                }

                                // check how many <ol> elements we need to close
                                for (int level = prevLevel; level > heading.Key; --level) {
                                    headingNumbering[level - 1] = 0;

                                    result.End(); // li
                                    result.End(); // ol
                                }

                                // check if there was no hierarchy change
                                if (result.HasName("li")) {
                                    result.End();
                                }

                                // create new list item
                                ++headingNumbering[heading.Key - 1];
                                string numbering = string.Empty;
                                for (int i = minHeadingLevel; i <= heading.Key; ++i) {
                                    if (0 == headingNumbering[i - 1]) {
                                        headingNumbering[i - 1] = 1;
                                    }
                                    numbering += headingNumbering[i - 1] + ".";
                                }

                                result.Start("li");
                                result.Start("span").Value(numbering).End();
                                result.Value(" "); // add a space between the span node and anchor
                                result.Start("a").Attr("href", "#" + anchorName).Attr("rel", "internal").Value(heading.Value.AsXmlNode.InnerText).End();

                                // NOTE (steveb): leave the li tag open so we can insert sub headings

                                prevLevel = heading.Key;
                            }
                        }

                        // close all open <ol> elements, including first element
                        for (int level = prevLevel; level >= minHeadingLevel; --level) {
                            if (result.HasName("li")) {
                                result.End(); // li
                            }
                            if (result.HasName("ol")) {
                                result.End(); // ol
                            }
                        }
                    } else {
                        result.Start("em").Value(DekiResources.NO_HEADINGS).End();
                    }

                    // replace all <span id="page.toc" /> place holders
                    XDoc tocDiv = null;
                    foreach (XDoc pageToc in _content["content/body//span[@id='page.toc']"]) {
                        if (tocDiv == null) {
                            tocDiv = new XDoc("div").Attr("class", "wiki-toc").AddNodes(result);
                        }
                        pageToc.Replace(TruncateTocDepth(tocDiv, pageToc["@depth"].AsInt));
                    }
                } else if(ParserMode.SAVE == _mode) {

                    // remove all nested headings (e.g. <h2>foo<h3>bar</h3></h2> -> <h2>foobar</h2>
                    for (XDoc heading = mainBody[ALL_NESTED_HEADINGS_XPATH]; !heading.IsEmpty; heading = mainBody[ALL_NESTED_HEADINGS_XPATH]) {
                        for (XDoc nestedHeading = heading[ALL_HEADINGS_XPATH]; !nestedHeading.IsEmpty; nestedHeading = heading[ALL_HEADINGS_XPATH]) {
                            nestedHeading.ReplaceWithNodes(nestedHeading);
                        }
                    }
                }
            }
            return result;
        }

        private void ProcessNoWiki() {
            if (!_isInclude) {
                if (ParserMode.VIEW == _mode || ParserMode.EDIT == _mode || ParserMode.VIEW_NO_EXECUTE == _mode) {
                    foreach (XDoc noWikiContent in _content[".//span[@class='nowiki']"]) {
                        if (!ExcludedTags.Contains(noWikiContent.AsXmlNode.ParentNode, true)) {

                            // in view mode, remove nowiki tags
                            if (ParserMode.VIEW == _mode || ParserMode.VIEW_NO_EXECUTE == _mode) {
                                AddChildrenBefore(noWikiContent, noWikiContent);
                                noWikiContent.Remove();

                                // in edit mode, update the class from nowiki to plain so that it is rendered properly
                            } else if (ParserMode.EDIT == _mode) {
                                noWikiContent.Attr("class", "plain");
                            }
                        }
                    }
                }
            }
        }

        private void ProcessWords() {
            if (!_isInclude) {
                if (_mode == ParserMode.SAVE) {
                    if (DekiContext.CurrentOrNull != null) {

                        // parse for banned words
                        Regex bannedWords = DekiContext.Current.Instance.BannedWords;
                        if (bannedWords != null) {
                            Stack<XmlNode> stack = new Stack<XmlNode>();
                            XmlNode current = _content[MAIN_BODY_XPATH].AsXmlNode;
                            while (current != null) {
                                if (current is XmlText) {

                                    // replace banned words in text
                                    current.Value = bannedWords.Replace(current.Value, m => HIDE_BANNED_WORD.Substring(0, m.Groups[0].Value.Length));
                                } else if ((current is XmlElement) && !ExcludedTags.Contains(current)) {

                                    // explore child nodes
                                    stack.Push(current.NextSibling);
                                    current = current.FirstChild;
                                    continue;
                                }

                                // move to next node
                                current = current.NextSibling;
                                while ((current == null) && (stack.Count > 0)) {
                                    current = stack.Pop();
                                }
                            }
                        }
                    }
                } else if (_mode == ParserMode.VIEW || ParserMode.VIEW_NO_EXECUTE == _mode) {
                    if (DreamContext.CurrentOrNull != null) {

                        // check if a list of words to be highlighted was provided
                        string highlight = DreamContext.Current.GetParam("highlight", null);
                        string[] highlightedWords = (highlight != null) ? highlight.Split(new[] { ' ', ',', '+' }, StringSplitOptions.RemoveEmptyEntries) : null;
                        if (!ArrayUtil.IsNullOrEmpty(highlightedWords)) {
                            XDocWord[] contentWords = XDocWord.ConvertToWordList(_content[MAIN_BODY_XPATH]);
                            for (int i = 0; i < highlightedWords.Length; ++i) {
                                highlightedWords[i] = highlightedWords[i].ToLowerInvariant();
                            }
                            Array.Sort(highlightedWords, StringComparer.Ordinal.Compare);

                            // loop over all words in document
                            XDocWord.ReplaceText(contentWords, delegate(XmlDocument doc, XDocWord word) {
                                XmlElement span = null;
                                if (word.IsWord) {
                                    int index = Array.BinarySearch(highlightedWords, word.Value.ToLowerInvariant());
                                    if ((index >= 0) && (index < highlightedWords.Length) && !ExcludedTags.Contains(word.Node, true)) {
                                        span = doc.CreateElement("span");
                                        span.SetAttribute("class", "highlight");
                                        span.SetAttribute("style", "background-color: yellow;");
                                        span.AppendChild(doc.CreateTextNode(word.Value));
                                    }
                                }
                                return span;
                            });
                        }
                    }
                }
            }
        }

        public static  bool PageAuthorCanExecute() {

            // NOTE (steveb): this method checks if the last person who edited the page being processed (incl. templates) has the UNSAFECONTENT permission
            // NOTE (arnec): This function may return a false negative if the parseState doesn't exist at the time of the call
            ParserState parseState = GetParseState();
            if(parseState == null) {
                return false;
            }
            PageBE page = parseState.ProcessingStack[parseState.ProcessingStack.Count - 1];
            UserBE user = UserBL.GetUserById(page.UserID);
            return PermissionsBL.IsUserAllowed(user, page, Permissions.UNSAFECONTENT);
        }

        public static void PostProcessTemplateInsertBody(ParserResult result, PageBE page) {

            // add nested template links
            XDoc body = result.MainBody;
            XDoc tree = PageSiteMapBL.BuildHtmlSiteMap(page, null, int.MaxValue, false);
            tree = tree[".//ul"];
            if(!tree.IsEmpty) {
                foreach(XDoc a in tree[".//a"]) {
                    a["@href"].ReplaceValue("#");
                    a.Attr("template", a["@title"].AsText);
                    a.Attr("class", "site");
                    a.RemoveAttr("rel");
                    a.RemoveAttr("pageid");
                }
                body.Add(tree);
            }

            // add tag inclusion hints
            IList<TagBE> tags = TagBL.GetTagsForPage(page);
            if(tags.Count > 0) {
                body.Start("p")
                    .Attr("class", "template:tag-insert")
                    .Elem("em", DekiResources.GetString("Page.Tags.tags-inserted-from-template") + " ");
                bool first = true;
                foreach(TagBE tag in tags) {
                    if(!first) {
                        body.Elem("span", ", ");
                    }
                    first = false;
                    body.Start("a").Attr("href", "#").Value(tag.PrefixedName).End();
                }
                body.End();
            }
        }
    }

    internal static class WikiConverter_TextToXml {

        //--- Constants ---
        private const string DATE_SCRIPT = "{{save:date.format(date.now, \"dd MMMM yyyy\")}}";
        private const string USER_SCRIPT = "{{save:web.link(user.uri, user.name)}}";

        //--- Types ---
        private enum ParserState {
            Text,           // regular text, no special processing
            Link,           // processing [[ *link* ]]
            Code            // processing {{ *code* }}
        }

        //--- Class Fields ---
        private static readonly Regex SIGNATURE_REGEX = new Regex("~{3,5}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        //--- Class Methods ---
        internal static void Convert(XDoc doc) {

            // convert <span class="plain"> and <nowiki> to <span class="nowiki">
            ConvertNoWiki(doc);

            // convert [[link]] to <a> elements
            ConvertWikiText(doc.AsXmlNode);
        }

        private static void ConvertNoWiki(XDoc xhtml) {

            // TODO (steveb): why not standardize on class="plain" since that's what the editor 
            // already uses anyway and it's also recognized as an excluded CSS class?

            // process each no-wiki block
            XDoc noWikiContents = xhtml[".//nowiki | .//span[@class='plain']"];
            foreach (XDoc noWikiContent in noWikiContents) {
                if (!ExcludedTags.Contains(noWikiContent.AsXmlNode.ParentNode, true)) {
                    noWikiContent.Rename("span");
                    noWikiContent.Attr("class", "nowiki");
                }
            }
        }

        private static void ConvertWikiText(XmlNode node) {
            Stack<XmlNode> stack = new Stack<XmlNode>();
            ParserState state = ParserState.Text;
            XmlText start = null;
            XmlNode current = node;
            while(current != null) {
                if(current is XmlText) {

                    // handle signatures
                    current.Value = SIGNATURE_REGEX.Replace(current.Value, delegate(Match m) {
                        switch(m.Length) {
                        case 5:
                            return DATE_SCRIPT;
                        case 4:
                            return USER_SCRIPT + " " + DATE_SCRIPT;
                        case 3:
                            return USER_SCRIPT;
                        default:
                            return m.Value;
                        }

                        // NOTE (PeteE): following return is required to compile under mono 1.2.6
                        return null;
                    });

                    // parse characters
                    string text = current.Value;
                    for(int i = 0; i < text.Length; ++i) {
                        switch(text[i]) {
                        case '[':
                            switch(state) {
                            case ParserState.Link:

                            // restart internal link

                            case ParserState.Text:
                                if((StringAt(text, i + 1) == '[') && (StringAt(text, i + 2) != '[')) {
                                    state = ParserState.Link;

                                    // split the current text node
                                    current = ((XmlText)current).SplitText(i + 2);
                                    start = (XmlText)current;
                                    goto continue_while_loop;
                                } else {

                                    // reset state
                                    state = ParserState.Text;
                                    start = null;
                                }
                                break;
                            }
                            break;
                        case ']':
                            switch(state) {
                            case ParserState.Link:

                                // check if link is structurally sound
                                if(ReferenceEquals(current.ParentNode, start.ParentNode) && (StringAt(text, i + 1) == ']')) {
                                    XmlText next = ((XmlText)current).SplitText(i);
                                    ConvertNodes(state, start, next);

                                    // reset state
                                    state = ParserState.Text;
                                    start = null;
                                    current = next;
                                    goto continue_while_loop;
                                }

                                // reset state
                                state = ParserState.Text;
                                start = null;
                                break;
                            }
                            break;
                        case '{':
                            switch(state) {
                            case ParserState.Text:
                                if(StringAt(text, i + 1) == '{') {
                                    state = ParserState.Code;

                                    // split the current text node
                                    current = ((XmlText)current).SplitText(i + 2);
                                    start = (XmlText)current;
                                    goto continue_while_loop;
                                }
                                break;
                            }
                            break;
                        case '}':
                            switch(state) {
                            case ParserState.Code:

                                // check if link is structurally sound
                                if(object.ReferenceEquals(current.ParentNode, start.ParentNode) && (StringAt(text, i + 1) == '}')) {
                                    XmlText next = ((XmlText)current).SplitText(i);
                                    if(!ConvertNodes(state, start, next)) {
                                        current.Value = text;
                                        current.ParentNode.RemoveChild(next);
                                        break;
                                    }

                                    // reset state
                                    state = ParserState.Text;
                                    start = null;
                                    current = next;
                                    goto continue_while_loop;
                                }
                                break;
                            }
                            break;
                        }
                    }
                } else {

                    // check if we are on an element that doesn't require conversion
                    if(!IsExcluded(current) && current.HasChildNodes) {
                        stack.Push(current);
                        current = current.FirstChild;
                        continue;
                    }
                }

                // move to next node
                current = current.NextSibling;
                while((current == null) && (stack.Count > 0)) {
                    current = stack.Pop();
                    if((start != null) && ReferenceEquals(start.ParentNode, current)) {
                        start = null;
                        state = ParserState.Text;
                    }
                    current = current.NextSibling;
                }
            continue_while_loop:
                continue;
            }
        }

        private static bool ConvertNodes(ParserState state, XmlText start, XmlText end) {

            // convert contents
            if(state == ParserState.Code) {

                // NOTE (steveb): convert wiki code {{ }}

                // convert nodes to text
                StringBuilder code = new StringBuilder();
                for(XmlNode current = start; current != end; current = current.NextSibling) {
                    code.Append(current.InnerText);
                }

                // check if this is a valid code block
                int mode = 0;    // mode: 0 = none, 1 = quote, 2 = double-quote
                int count = 0;
                for(int i = 0; i < code.Length; ++i) {
                    switch(code[i]) {
                    case '\\':

                        // skip one character
                        ++i;
                        break;
                    case '{':
                        if(mode == 0) {

                            // new open curly brace
                            ++count;
                        }
                        break;
                    case '}':
                        if(mode == 0) {

                            // closed existing curly brace
                            --count;
                        }
                        break;
                    case '"':
                        switch(mode) {
                        case 0:

                            // found opening double-quote
                            mode = 2;
                            break;
                        case 1:
                            break;
                        case 2:

                            // found closing double-quote
                            mode = 0;
                            break;
                        }
                        break;
                    case '\'':
                        switch(mode) {
                        case 0:

                            // found opening quote
                            mode = 1;
                            break;
                        case 1:

                            // found closing quote
                            mode = 0;
                            break;
                        case 2:
                            break;
                        }
                        break;
                    }
                }

                // NOTE (steveb): we could be stricter here, but it would work against us; the only thing we care about
                //  is that we don't expect more closing curly-braces; if the code is syntactically invalid, we want the
                //  parser to report the error instead.

                if((count > 0) || (mode != 0)) {
                    return false;
                }

                // trim beginning and end nodes
                start.PreviousSibling.Value = start.PreviousSibling.Value.Substring(0, start.PreviousSibling.Value.Length - 2);
                end.Value = end.Value.Substring(2);

                // code node
                XmlElement wrapper = start.OwnerDocument.CreateElement("span");
                wrapper.SetAttribute("class", "script");
                start.ParentNode.InsertBefore(wrapper, start);

                // remove nodes
                for(XmlNode current = start; current != end; ) {
                    XmlNode tmp = current;
                    current = current.NextSibling;
                    wrapper.AppendChild(tmp);
                }
            } else {

                // NOTE (steveb): convert wiki link [[ ]]

                // merge text nodes
                for(XmlNode current = start.NextSibling; (current != end) && (current.NodeType == XmlNodeType.Text); ) {
                    start.Value += current.Value;
                    XmlNode tmp = current;
                    current = current.NextSibling;
                    tmp.ParentNode.RemoveChild(tmp);
                }

                // find link separator
                string link;
                int separator = start.Value.IndexOf('|');
                if(separator < 0) {
                    if(start.NextSibling != end) {

                        // link is not a simple text node, ignore it
                        return false;
                    }
                    link = start.Value.Trim();
                    if(link.Length == 0) {

                        // no link, ignore it and move on
                        return false;
                    }
                    start.ParentNode.RemoveChild(start);
                    start = end;
                } else {
                    link = start.Value.Substring(0, separator).Trim();
                    start.Value = start.Value.Substring(separator + 1);
                }

                // trim beginning and end nodes
                start.PreviousSibling.Value = start.PreviousSibling.Value.Substring(0, start.PreviousSibling.Value.Length - 2);
                end.Value = end.Value.Substring(2);

                // link node
                XmlElement wrapper = start.OwnerDocument.CreateElement("a");
                wrapper.SetAttribute("href", Utils.EncodeUriCharacters(link));
                wrapper.SetAttribute("title", link);
                start.ParentNode.InsertBefore(wrapper, start);

                // move nodes
                for(XmlNode current = start; current != end; ) {
                    XmlNode tmp = current;
                    current = current.NextSibling;
                    wrapper.AppendChild(tmp);
                }
            }
            return true;
        }

        private static bool IsExcluded(XmlNode node) {
            return (node.NodeType != XmlNodeType.Element) || ExcludedTags.Contains(node);
        }

        private static char StringAt(string text, int index) {
            if (index >= text.Length) {
                return char.MinValue;
            }
            return text[index];
        }
    }

    public class EscapedXmlTextWriter : XmlTextWriter {

        //--- Constructors ---
        public EscapedXmlTextWriter(string filename, Encoding encoding) : base(filename, encoding) { }
        public EscapedXmlTextWriter(Stream w, Encoding encoding) : base(w, encoding) { }
        public EscapedXmlTextWriter(TextWriter w) : base(w) { }

        //--- Methods ---
        public override void WriteString(string text) {

            // NOTE (steveb): technicaly '\'' should be converted to &apos; when doing XML serialization, 
            // but &apos; is not in the HTML DTD, causing issues for HTML readers; so we don't use it.

            if(string.IsNullOrEmpty(text)) {
                return;
            }
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < text.Length; i++) {
                char c = text[i];

                switch(c) {
                case '&':
                    sb.Append("&amp;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                case '>':
                    sb.Append("&gt;");
                    break;
                case '"':
                    sb.Append("&quot;");
                    break;
                case '\n':
                case '\r':
                case '\t':

                    // don't escape these control characters
                    sb.Append(c);
                    break;
                default:
                    if(char.IsControl(c)) {
                        sb.AppendFormat("&#{0};", (uint)c);
                    } else if(!char.IsSurrogate(c)) {
                        sb.Append(c);
                    } else {
                        if(text.Length >= i + 1) {
                            if(char.IsHighSurrogate(c) && char.IsLowSurrogate(text[i + 1])) {
                                sb.AppendFormat("&#{0};", char.ConvertToUtf32(c, text[i + 1]));
                                i++;
                            }
                        }
                    }
                    break;
                }
            }

            base.WriteRaw(sb.ToString());
        }
    }
}
