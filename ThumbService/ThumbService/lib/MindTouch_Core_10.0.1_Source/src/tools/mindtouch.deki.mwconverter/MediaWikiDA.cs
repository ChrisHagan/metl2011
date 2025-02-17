/*
 * MindTouch MediaWiki Converter
 * Copyright (C) 2006-2008 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 * http://www.gnu.org/copyleft/lesser.html
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;

using MindTouch.Data;
using MindTouch.Deki;
using MindTouch.Deki.Data;
using MindTouch.Deki.Data.MySql;
using MindTouch.Deki.Logic;
using MindTouch.Dream;
using MindTouch.Tools;

namespace Mindtouch.Tools {

    class MediaWikiDA {

        public static bool IsSupportedNamespace(Title title) {

            switch ((int)title.Namespace) {

                // check whether this is a supported namespace
                case (int)NS.MAIN:
                case (int)NS.MAIN_TALK:
                case (int)NS.USER:
                case (int)NS.USER_TALK:
                case (int)NS.PROJECT:
                case (int)NS.PROJECT_TALK:
                case (int)NS.TEMPLATE:
                case (int)NS.TEMPLATE_TALK:
                case (int)NS.HELP:
                case (int)NS.HELP_TALK:
                    return true;

                // the image, image_talk, native, native_talk, category, category_talk, special namespaces are not migrated into the page table
                default:
                    return false;
            }
        }

        public static string Latin1ToUTF8(string latin1Text) {
            if (null != latin1Text) {
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                Encoding unicode = Encoding.UTF8;
                byte[] isoBytes = iso.GetBytes(latin1Text);
                return unicode.GetString(isoBytes);
            } else {
                return null;
            }
        }

        public static UserBE[] GetUsers() {
            List<UserBE> users = new List<UserBE>();
            MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("SELECT user_id, user_name, user_real_name, user_password, user_newpassword, user_email, user_options, user_touched, user_token, (select count(*) from {0}user_groups where ug_user=user_id and ug_group='sysop') as user_sysop from {0}user order by user_touched DESC", MediaWikiConverterContext.Current.MWUserPrefix)).Execute(delegate(IDataReader dr) {
                while (dr.Read()) {
                    users.Add(PopulateUser(dr));
                }
            });
            return users.ToArray();
        }

        public static IPBlockBE[] GetIPBlocks() {
            List<IPBlockBE> ipBlocks = new List<IPBlockBE>();
            MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("SELECT * from {0}ipblocks", MediaWikiConverterContext.Current.MWUserPrefix)).Execute(delegate(IDataReader dr) {
                while (dr.Read()) {
                    ipBlocks.Add(PopulateIPBlock(dr));
                }
            });
            return ipBlocks.ToArray();
        }

        public static Dictionary<Site, NameValueCollection> GetInterWikiBySite() {
            Dictionary<Site, NameValueCollection> interWikiBySite = new Dictionary<Site, NameValueCollection>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                NameValueCollection interWiki = new NameValueCollection();
                interWikiBySite[site] = interWiki;
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("SELECT * from {0}interwiki", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        interWiki.Add(GetUTF8String(dr, "iw_prefix"), GetUTF8String(dr, "iw_url"));
                    }
                });
            }
            return interWikiBySite;
        }

        public static Dictionary<Site, Title> GetMainPageBySite() {
            Dictionary<Site, Title> mainPageBySite = new Dictionary<Site, Title>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format(
                     @"SELECT page_title 
                      FROM {0}page WHERE page_namespace = 0 ORDER BY page_id LIMIT 1", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        Title title = Title.FromDbPath(NS.MAIN, GetUTF8String(dr, "page_title"), null);
                        if (IsSupportedNamespace(title)) {
                            mainPageBySite[site] = title;
                        }
                    }
                });
            }
            return mainPageBySite;
        }

        public static Dictionary<Site, List<PageBE>> GetPagesBySite() {
            Dictionary<Site, List<PageBE>> pagesBySite = new Dictionary<Site, List<PageBE>>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                List<PageBE> pages = new List<PageBE>();
                pagesBySite[site] = pages;
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format(
                    @"SELECT page_id, page_namespace, page_title, page_restrictions, page_counter, 
                             page_is_redirect, page_is_new, page_touched, rev_user, rev_timestamp, 
                             rev_minor_edit, rev_comment, old_text, rev_user_text
                    FROM {0}page 
                    JOIN {0}revision on {0}revision.rev_id = {0}page.page_latest 
                    JOIN {0}text on {0}revision.rev_text_id = {0}text.old_id", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        PageBE page = PopulatePage(dr);
                        if (IsSupportedNamespace(page.Title)) {
                            page.Language = site.Language;
                            pages.Add(page);
                        }
                    }
                });
            }
            return pagesBySite;
        }

        public static Dictionary<Site, List<PageBE>>  GetRevisionsBySite() {
            Dictionary<Site, List<PageBE>> revisionsBySite = new Dictionary<Site, List<PageBE>>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                List<PageBE> revisions = new List<PageBE>();
                revisionsBySite[site] = revisions;
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format(
                    @"SELECT rev_id, page_namespace, page_title, rev_timestamp
                      FROM {0}revision 
                      JOIN {0}page on rev_page = page_id
                      JOIN {0}text on rev_text_id = old_id 
                      WHERE page_latest!=rev_id", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        PageBE revision = PopulatePartialOld(dr);
                        if (IsSupportedNamespace(revision.Title)) {
                            revision.Language = site.Language;
                            revisions.Add(revision);
                        }
                    }
                });
            }
            return revisionsBySite;
        }

        public static PageBE GetPopulatedRevision(Site site, PageBE revision) {
            PageBE result = null;
            MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format(
                @"SELECT rev_id, page_namespace, page_title, old_text, rev_comment, rev_user, rev_user_text, rev_timestamp, rev_minor_edit, old_flags
                      FROM {0}revision 
                      JOIN {0}page on rev_page = page_id
                      JOIN {0}text on rev_text_id = old_id 
                      WHERE rev_id={1}", site.DbPrefix, revision.ID)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        result = PopulateOld(dr);
                        result.Language = site.Language;
                    }
                });
            return result;
        }

        public static List<string> GetParentCategoryNames(Site site, string category, List<string> previousCategories) {
            List<string> parentCategories = new List<string>();
            previousCategories.Add(category);
            MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("SELECT cl_from, cl_to from {0}categorylinks where cl_from=(select page_id from {0}page where page_namespace=14 and page_title=\"{1}\")", site.DbPrefix, category)).Execute(delegate(IDataReader dr) {
                while (dr.Read()) {
                    string parentCategory = GetUTF8String(dr, "cl_to");
                    if (parentCategory != category && parentCategory != "All_Categories" && !previousCategories.Contains(parentCategory)) {
                        parentCategories.Add(GetUTF8String(dr, "cl_to"));
                        GetParentCategoryNames(site, parentCategory, previousCategories);
                    }
                }
            });
            return parentCategories;
        }

        public static Dictionary<string, List<string>> GetCategoryNamesByPage() {
            Dictionary<string, List<string>> pageToCategoryMap = new Dictionary<string, List<string>>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("SELECT cl_from, cl_to from {0}categorylinks", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        List<string> categories;
                        ulong pageID = DbUtils.Convert.To<ulong>(dr["cl_from"]).Value;
                        string category = GetUTF8String(dr, "cl_to");
                        if (!pageToCategoryMap.TryGetValue(site.Language + pageID, out categories)) {
                            categories = new List<string>();
                            pageToCategoryMap.Add(site.Language + pageID, categories);
                        }
                        categories.Add(category);
                        categories.AddRange(GetParentCategoryNames(site, category, new List<string>()));
                    }
                });
            }
            return pageToCategoryMap;
        }

        public static Dictionary<Site, List<AttachmentBE>> GetFilesBySite() {
            Dictionary<Site, List<AttachmentBE>> filesBySite = new Dictionary<Site, List<AttachmentBE>>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                List<AttachmentBE> images = new List<AttachmentBE>();
                filesBySite[site] = images;
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("(SELECT oi_archive_name as img_filename, oi_name as img_name, oi_size as img_size, img_major_mime, img_minor_mime, oi_description as img_description, oi_user as img_user, oi_user_text as img_user_text, oi_timestamp as img_timestamp from {0}oldimage join {0}image on oi_name=img_name) UNION (SELECT img_name as image_filename, img_name, img_size, img_major_mime, img_minor_mime, img_description, img_user, img_user_text, img_timestamp from {0}image) order by img_name, img_timestamp ASC", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        AttachmentBE file = PopulateFile(dr);
                        images.Add(file);
                    }
                });
            }
            return filesBySite;
        }

        public static Dictionary<Site, List<RecentChangeBE>> GetRecentChangesBySite() {
            Dictionary<Site, List<RecentChangeBE>> recentChangesBySite = new Dictionary<Site, List<RecentChangeBE>>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                List<RecentChangeBE> recentChanges = new List<RecentChangeBE>();
                recentChangesBySite[site] = recentChanges;
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("SELECT * from {0}recentchanges where rc_namespace >= 0 order by rc_timestamp ASC", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        RecentChangeBE recentChange = PopulateRecentChange(dr);
                        if ((null != recentChange) && IsSupportedNamespace(recentChange.Page.Title)) {
                            recentChanges.Add(recentChange);
                        }
                    }
                });
            }
            return recentChangesBySite;
        }

        public static Dictionary<Site, List<WatchlistBE>> GetWatchlistBySite() {
            Dictionary<Site, List<WatchlistBE>> watchlistBySite = new Dictionary<Site, List<WatchlistBE>>();
            foreach (Site site in MediaWikiConverterContext.Current.MWSites) {
                List<WatchlistBE> watchlist = new List<WatchlistBE>();
                watchlistBySite[site] = watchlist;
                MediaWikiConverterContext.Current.MWCatalog.NewQuery(String.Format("SELECT wl_user, wl_namespace, wl_title FROM {0}watchlist", site.DbPrefix)).Execute(delegate(IDataReader dr) {
                    while (dr.Read()) {
                        WatchlistBE watch = PopulateWatchlist(dr);
                        if (IsSupportedNamespace(watch.Title)) {
                            watchlist.Add(watch);
                        }
                    }
                });
            }
            return watchlistBySite; 
        }

        public static void DeleteDWUsers() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM users").Execute();
        }

        public static void UpdateDWUserID(uint oldID, uint newID) {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("UPDATE users SET user_id={0} WHERE user_id={1}", newID, oldID)).Execute();
        }

        public static void DeleteDWIPBlocks() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM banips; DELETE FROM banusers; DELETE FROM bans;").Execute();
        }

        public static void InsertDWIPBlock(IPBlockBE ipBlock) {
            uint banID = 0;
            MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("INSERT into bans (ban_by_user_id, ban_reason, ban_revokemask, ban_last_edit) VALUES ('{0}', '{1}', '{2}', '{3}');  SELECT LAST_INSERT_ID() as banid;",
               ipBlock.ByUserID, DataCommand.MakeSqlSafe(ipBlock.Reason), 9223372036854779902, ipBlock.Timestamp)).Execute(delegate(IDataReader dr) {
                while (dr.Read()) {
                    banID = DbUtils.Convert.To<uint>(dr["banid"], 0);
                }
            });
            if (0 == ipBlock.UserID) {
                MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("INSERT into banips (banip_ipaddress, banip_ban_id) VALUES ('{0}', '{1}')",
                   DataCommand.MakeSqlSafe(ipBlock.Address), banID)).Execute();
            } else {
                MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("INSERT into banusers (banuser_user_id, banuser_ban_id) VALUES ('{0}', '{1}')",
                   ipBlock.UserID, banID)).Execute();
            }
        }

        public static void DeleteDWServiceById(uint id) {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM services where service_id=" + id + ";DELETE FROM service_prefs where service_id=" + id + ";DELETE FROM service_config where service_id=" + id + ";").Execute();
        }

        public static void InsertDWService(ServiceBE service) {
            DataCommand cmd = MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("INSERT into services (service_type, service_sid, service_description, service_local, service_enabled) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}'); SELECT LAST_INSERT_ID() as serviceid;",
                service.Type.ToString().ToLowerInvariant(), DataCommand.MakeSqlSafe(service.SID), DataCommand.MakeSqlSafe(service.Description), service._ServiceLocal, service._ServiceEnabled));
            cmd.Execute(delegate(IDataReader dr) {
                while (dr.Read()) {
                    service.Id = DbUtils.Convert.To<uint>(dr["serviceid"], 0);
                }
            });
            foreach (String key in service.Config.Keys) {
                MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("INSERT into service_config (service_id, config_name, config_value) VALUES ('{0}', '{1}', '{2}')",
                    service.Id, DataCommand.MakeSqlSafe(key), DataCommand.MakeSqlSafe(service.Config[key]))).Execute();

            }
        }

        public static void DeleteDWPages() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM pages where (page_namespace < 100 and page_title != '') OR (page_namespace=0 AND page_title='')").Execute();
        }

        public static void DeleteDWLinks() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM links; DELETE from brokenlinks").Execute();
        }

        public static void UpdateDWPageData(PageBE page) {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("UPDATE pages set page_usecache={0}, page_tip='{1}', page_content_type='{2}', page_text='{3}' where page_id={4}", page.UseCache, DataCommand.MakeSqlSafe(page.TIP), DataCommand.MakeSqlSafe(page.ContentType), DataCommand.MakeSqlSafe(page.GetText(DbUtils.CurrentSession)), page.ID)).Execute();
        }

        public static void DeleteDWFiles() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery(@"
delete from resourcecontents
where rescontent_id in (
	select resrev_content_id from resourcerevs where resrev_res_id in (
		select res_id from resources where res_type = 2
	)
);
delete from resourcerevs
where resrev_res_id in (
		select res_id from resources where res_type = 2
	);

delete from resources where res_type = 2;
").Execute();
        }

        public static void DeleteDWRevisions() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM old").Execute();
        }

        public static void DeleteDWRecentChanges() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM recentchanges").Execute();
        }

        public static void InsertDWRecentChange(RecentChangeBE recentChange) {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("INSERT into recentchanges (rc_timestamp, rc_cur_time, rc_user, rc_namespace, rc_title, rc_comment, rc_cur_id, rc_this_oldid, rc_last_oldid, rc_type, rc_ip, rc_minor, rc_bot, rc_new, rc_patrolled) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}')",
                recentChange.Timestamp, recentChange.Timestamp, recentChange.User.ID, recentChange.Page._Namespace, DataCommand.MakeSqlSafe(recentChange.Page.Title.AsUnprefixedDbPath()),
                DataCommand.MakeSqlSafe(recentChange.Comment), recentChange.Page.ID, recentChange.ThisOldID, recentChange.LastOldID, (uint)recentChange.Type, recentChange.IP, recentChange.IsMinor, recentChange.IsBot, recentChange.IsNew, recentChange.IsPatrolled)).Execute();
        }

        public static void DeleteDWWatch() {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery("DELETE FROM watchlist").Execute();
        }

        public static void InsertDWWatch(WatchlistBE watch) {
            MediaWikiConverterContext.Current.DWCatalog.NewQuery(String.Format("REPLACE into watchlist (wl_user, wl_namespace, wl_title) VALUES ({0}, '{1}', '{2}')",
                watch.UserID, (uint)watch.Title.Namespace, DataCommand.MakeSqlSafe(watch.Title.AsUnprefixedDbPath()))).Execute();
        }

        private static String GetUTF8String(IDataRecord dr, string field) {
            if (typeof(string) == dr.GetFieldType(dr.GetOrdinal(field))) {
                return MediaWikiDA.Latin1ToUTF8(DbUtils.Convert.To<string>(dr[field], null));
            } else {
                return Encoding.UTF8.GetString(DbUtils.Convert.To<byte[]>(dr[field], null));
            }
        }

        private static UserBE PopulateUser(IDataRecord dr) {
            UserBE user = new UserBE();
            user.ID = DbUtils.Convert.To<uint>(dr["user_id"]).Value;
            user.Name = GetUTF8String(dr, "user_name");
            user.RealName = GetUTF8String(dr, "user_real_name");
            user.Password = GetUTF8String(dr, "user_password");
            user.NewPassword = GetUTF8String(dr, "user_newpassword");
            user.Email = GetUTF8String(dr, "user_email");
            user.Touched = DbUtils.ToDateTime(GetUTF8String(dr, "user_touched"));
            bool sysop = DbUtils.Convert.To<bool>(dr["user_sysop"]).Value;
            if (sysop) {
                user.RoleId = 5;
            } else {
                user.RoleId = 4;
            }
            user.UserActive = true;
            user.ServiceId = 1;
            return user;
        }

        private static IPBlockBE PopulateIPBlock(IDataRecord dr) {
            IPBlockBE ipBlock = new IPBlockBE();
            ipBlock.Address = GetUTF8String(dr, "ipb_address");
            ipBlock.UserID = DbUtils.Convert.To<uint>(dr["ipb_user"]).Value;
            ipBlock.ByUserID = DbUtils.Convert.To<uint>(dr["ipb_by"]).Value;
            ipBlock.Reason = GetUTF8String(dr, "ipb_reason");
            ipBlock.Timestamp = GetUTF8String(dr, "ipb_timestamp");
            ipBlock.Auto = DbUtils.Convert.To<uint>(dr["ipb_auto"]).Value;
            ipBlock.AnonymousOnly = DbUtils.Convert.To<uint>(dr["ipb_anon_only"]).Value;
            ipBlock.CreateAccount = DbUtils.Convert.To<uint>(dr["ipb_create_account"]).Value;
            ipBlock.EnableAutoBlock = DbUtils.Convert.To<uint>(dr["ipb_enable_autoblock"]).Value;
            ipBlock.Expiry = GetUTF8String(dr, "ipb_expiry");
            return ipBlock;
        }

        private static PageBE PopulatePage(IDataRecord dr) {
            PageBE page = new PageBE();
            page.ID = DbUtils.Convert.To<ulong>(dr["page_id"]).Value;
            page._Namespace = DbUtils.Convert.To<ushort>(dr["page_namespace"]).Value;
            page._Title = GetUTF8String(dr, "page_title");
            string restrictions = GetUTF8String(dr, "page_restrictions");
            if ((restrictions == "sysop") || (restrictions == "move=sysop:edit=sysop")) {
                page.RestrictionID = 2;
            }
            page.Counter = DbUtils.Convert.To<uint>(dr["page_counter"]).Value;
            page.IsRedirect = DbUtils.Convert.To<bool>(dr["page_is_redirect"]).Value;
            page.IsNew = DbUtils.Convert.To<bool>(dr["page_is_new"]).Value;
            page.Touched = DbUtils.ToDateTime(GetUTF8String(dr, "page_touched"));

            if (MediaWikiConverterContext.Current.Merge) {
                page.UserID = MediaWikiConverterContext.Current.MergeUserId;
            } else {
                page.UserID = DbUtils.Convert.To<uint>(dr["rev_user"]).Value;
            }
            page.TimeStamp = DbUtils.ToDateTime(GetUTF8String(dr, "rev_timestamp"));
            page.MinorEdit = DbUtils.Convert.To<bool>(dr["rev_minor_edit"]).Value;
            page.Comment = GetUTF8String(dr, "rev_comment");
            page.SetText(GetUTF8String(dr, "old_text"));
            page.ContentType = DekiMimeType.MEDIAWIKI_TEXT;  
            page.TIP = String.Empty;

            if(MediaWikiConverterContext.Current.AttributeViaPageRevComment) {

                //Add the original revision username to the comment
                string username = GetUTF8String(dr, "rev_user_text");
                if(!string.IsNullOrEmpty(username)) {
                    page.Comment = string.Format(MediaWikiConverterContext.Current.AttributeViaPageRevCommentPattern, page.Comment, username);
                }
            }

            return page;
        }

        private static PageBE PopulatePartialOld(IDataRecord dr) {
            PageBE old = new PageBE();
            old.ID = DbUtils.Convert.To<ulong>(dr["rev_id"]).Value;
            old._Namespace = DbUtils.Convert.To<ushort>(dr["page_namespace"]).Value;
            old._Title = GetUTF8String(dr, "page_title");
            old.TimeStamp = DbUtils.ToDateTime(GetUTF8String(dr, "rev_timestamp"));
            return old;
        }

        private static PageBE PopulateOld(IDataRecord dr) {
            PageBE old = new PageBE();
            old.ID = DbUtils.Convert.To<ulong>(dr["rev_id"]).Value;
            old._Namespace = DbUtils.Convert.To<ushort>(dr["page_namespace"]).Value;
            old._Title = GetUTF8String(dr, "page_title");
            old.SetText(GetUTF8String(dr, "old_text"));
            old.Comment = GetUTF8String(dr, "rev_comment");
            if (MediaWikiConverterContext.Current.Merge) {
                old.UserID = MediaWikiConverterContext.Current.MergeUserId;
            } else {
                old.UserID = DbUtils.Convert.To<uint>(dr["rev_user"]).Value;
            }
            old.TimeStamp = DbUtils.ToDateTime(GetUTF8String(dr, "rev_timestamp"));
            old.MinorEdit = DbUtils.Convert.To<bool>(dr["rev_minor_edit"]).Value;
            old.ContentType = DekiMimeType.MEDIAWIKI_TEXT;
            if(MediaWikiConverterContext.Current.AttributeViaPageRevComment) {

                //Add the original revision username to the comment
                string username = GetUTF8String(dr, "rev_user_text");
                if(!string.IsNullOrEmpty(username)) {
                    old.Comment = string.Format(MediaWikiConverterContext.Current.AttributeViaPageRevCommentPattern, old.Comment, username);
                }
            }

            return old;
        }

        private static AttachmentBE PopulateFile(IDataRecord dr) {
            
            string name = GetUTF8String(dr, "img_name");
            uint size = DbUtils.Convert.To<uint>(dr["img_size"], 0);
            MimeType mimetype = new MimeType(GetUTF8String(dr, "img_major_mime") +"/" + GetUTF8String(dr, "img_minor_mime"));
            string changedescription = GetUTF8String(dr, "img_description");
            uint userId;
            if (MediaWikiConverterContext.Current.Merge) {
                userId = MediaWikiConverterContext.Current.MergeUserId;
            } else {
                userId = DbUtils.Convert.To<uint>(dr["img_user"], 0);
            }
            DateTime timestamp = DbUtils.ToDateTime(GetUTF8String(dr, "img_timestamp"));

            ResourceContentBE rc = new ResourceContentBE(true);
            rc.Size = size;
            rc.MimeType = mimetype;

            AttachmentBE file = new ResourceBL<AttachmentBE>(ResourceBE.Type.FILE).BuildRevForNewResource(0/*parent page defined later*/, ResourceBE.Type.PAGE, name, mimetype, size, changedescription, ResourceBE.Type.FILE, userId, rc);
            file.MetaXml.Elem("physicalfilename", GetUTF8String(dr, "img_filename"));
            return file;
        }

        private static RecentChangeBE PopulateRecentChange(IDataRecord dr) {
            RecentChangeBE recentChange = new RecentChangeBE();
            recentChange.Timestamp = GetUTF8String(dr, "rc_timestamp");
            recentChange.User = new UserBE();
            if (MediaWikiConverterContext.Current.Merge) {
                recentChange.User.ID = MediaWikiConverterContext.Current.MergeUserId;
            } else {
                recentChange.User.ID = DbUtils.Convert.To<uint>(dr["rc_user"], 0);
            }
            recentChange.User.Name = GetUTF8String(dr, "rc_user_text");
            recentChange.Page = new PageBE();
            recentChange.Page.ID = DbUtils.Convert.To<ulong>(dr["rc_cur_id"]).Value;
            recentChange.Page._Namespace = DbUtils.Convert.To<ushort>(dr["rc_namespace"]).Value;
            recentChange.Page._Title = GetUTF8String(dr, "rc_title");
            recentChange.Comment = GetUTF8String(dr, "rc_comment");
            recentChange.ThisOldID = DbUtils.Convert.To<ulong>(dr["rc_this_oldid"], 0);
            recentChange.LastOldID = DbUtils.Convert.To<ulong>(dr["rc_last_oldid"], 0);
            recentChange.Type = (RC)DbUtils.Convert.To<uint>(dr["rc_type"], 0);
            recentChange.IP = GetUTF8String(dr, "rc_ip");
            recentChange.IsMinor = DbUtils.Convert.To<uint>(dr["rc_minor"]).Value;
            recentChange.IsBot = DbUtils.Convert.To<uint>(dr["rc_bot"]).Value;
            recentChange.IsNew = DbUtils.Convert.To<uint>(dr["rc_new"]).Value;
            recentChange.IsPatrolled = DbUtils.Convert.To<uint>(dr["rc_patrolled"]).Value;
            return recentChange;
        }

        private static WatchlistBE PopulateWatchlist(IDataRecord dr) {
            WatchlistBE watch = new WatchlistBE();
            watch.UserID = DbUtils.Convert.To<uint>(dr["wl_user"], 0);
            watch.Title = Title.FromDbPath((NS)DbUtils.Convert.To<uint>(dr["wl_namespace"]).Value, GetUTF8String(dr, "wl_title"), null);
            return watch;
        }

    }
}
