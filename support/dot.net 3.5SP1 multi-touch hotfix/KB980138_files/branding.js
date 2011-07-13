///***********
///DEPENDANCIES: gsfx/Common.js
///***********

//Resizes the content area to fill the screen if the content is not big enough to do it on its own
var gsfx_brnd_surroundingheight = 0;
var gsfx_brnd_resizeContentArea = function()
{
    var ca = $('#contentArea').get(0);
    var pc = $('#gsfx_brnd_PageContainer').get(0);

    if (ca && pc)
    {
        if (!gsfx_brnd_surroundingheight)
        {
            gsfx_brnd_surroundingheight = 0;
            for (var i = 0; i < pc.childNodes.length; i++)
            {
                //we dont want the height of the content container because we only
                //want the height of the elements surrounding the content
                if (pc.childNodes[i].nodeType === 1 && pc.childNodes[i].id != 'gsfx_brnd_ContentContainer' && pc.childNodes[i].id != 'gsfx_brnd_PageHeaderImage')
                {
                    gsfx_brnd_surroundingheight += pc.childNodes[i].offsetHeight;
                }
            }
        }

        var bh = $(window).height();

        if (bh > gsfx_brnd_surroundingheight)
        {
            bh = (bh > 450) ? bh : 450;
            pc.style.height = bh;
            ca.style.height = bh - gsfx_brnd_surroundingheight;
        }
    }
};
//we are using a timer here, because its the only reliable way to ensure the page
//resizes properly, load and resize events are not reliable and/or will not fire off at the right time
//interval length of 250 equates to approximately 4 frames per second
var gsfx_brnd_resizetimer = setInterval(gsfx_brnd_resizeContentArea, 250);

//Left Nav flyout functionality; appends events to left nav submenus
function gsfx_brnd_CreateLeftNavFlyout(el, doMatte)
{
    if (typeof (el) === 'string') //if an element name was passed, convert to element reference
    {
        el = $('#' + el).get(0);
    }

    if (!el) //if item doesnt exist, end
    {
        return;
    }

    if (/MSIE ([1-6])/.test(navigator.userAgent)) //determin if lte ie6, for iframe matte
    {
        doMatte = true;
    }

    var menuItems = el.getElementsByTagName('li');
    for (var i = 0; i < menuItems.length; i++)
    {
        var hasUL = menuItems[i].getElementsByTagName('UL')[0];
        if (hasUL)
        {
            menuItems[i].className += ' gsfx_lnav_upmenu gsfx_lnav_submenu';

            var mdiv = document.createElement('div');
            mdiv.className = 'gsfx_lnav_menulink';
            var linknode = menuItems[i].getElementsByTagName('a')[0];
            var linknodechildren = linknode.childNodes;
            linknode.insertBefore(mdiv, linknodechildren[0]);

            var mover = function(event) //mouseover/mouseenter function
            {
                var e = ($.browser.msie) ? srcEl(event) : this;
                e.className = e.className.replace(/( ?|^)gsfx_lnav_upmenu\b/gi, 'gsfx_lnav_dropmenu');
                gsfx_brnd_MoveFlyout(e);
            };

            var mout = function(event) //mouseout/mouseleave function
            {
                var e = ($.browser.msie) ? srcEl(event) : this;
                e.className = e.className.replace(/( ?|^)gsfx_lnav_dropmenu\b/gi, 'gsfx_lnav_upmenu');
            };

            $(menuItems[i]).hover(mover, mout);

            if (doMatte === true) //iframe matteing for lte IE6 form element z-index bug
            {
                var ifMatte = document.createElement('IFRAME');
                ifMatte.className = 'gsfx_lnav_iframeMatte';
                ifMatte.style.height = (hasUL.offsetHeight + 2) + 'px';
                ifMatte.style.width = (hasUL.offsetWidth + 2) + 'px';
                hasUL.insertBefore(ifMatte, hasUL.firstChild);
            }
        }
        else
        {
            menuItems[i].className += ' gsfx_lnav_nomenu';
        }
    }
}
$(document).ready(function() { gsfx_brnd_CreateLeftNavFlyout('gsfx_lnav_LeftNav'); }); //append the create menu event to the window load

//This function moves the leftnav flyout window up in the event that it would drop below the bottom of the screen
function gsfx_brnd_MoveFlyout(e)
{

    var flyout = srcEl(e);
    if (flyout)
    {
        var hasUL = flyout.getElementsByTagName('UL')[0];
        if (hasUL)
        {
            var fp = AbsPos(flyout);
            var y = (window.event) ? document.documentElement.scrollTop + document.body.scrollTop : window.scrollY;
            if ((fp.top + hasUL.offsetHeight - y) > $(window).height())
            {
                hasUL.style.top = -((fp.top + hasUL.offsetHeight) - ($(window).height() + y)) + 'px';
            }
            else
            {
                hasUL.style.top = "0px";
            }
        }
    }
}

var gsfx_bsrch_InitCatSelection = function(targetid, charstr)
{
    var cval = unescape(fetchcookieval('adcatalog'));
    var a = 0;
    var highlight = false;
    if (cval)
    {
        var catcon = $('#gsfx_bsrch_catsel').get(0);
        if (catcon)
        {
            for (var i = 0; i < catcon.childNodes.length; i++)
            {
                var el = catcon.childNodes[i];
                if (el && el.tagName && el.getAttribute('catalog'))
                {
                    if (el.getAttribute('catalog') == cval)
                    {
                        el.className += ' gsfx_bsrch_highlight';
                        try
                        {
                            MS.Support.AC.ACChangeCharStart(targetid, charstr.split(':')[a]);
                            MS.Support.AC.ACSetLcid(targetid, cval.split('=')[1]);
                        } catch (e) { }
                        highlight = true;
                    }
                    else
                    {
                        el.className = el.className.replace(/( ?|^)gsfx_bsrch_highlight\b/gi, '');
                    }

                    a++;
                }
            }

            if (!highlight)
            {
                for (var i = 0; i < catcon.childNodes.length; i++)
                {
                    var el = catcon.childNodes[i];
                    if (el && el.tagName && el.getAttribute('catalog'))
                    {
                        el.className += ' gsfx_bsrch_highlight';
                        return;
                    }
                }
            }
        }
    }
}

var gsfx_brnd_ToolbarSelection = function()
{
    if (!$('#gsfx_brnd_LocalLinks').get(0))
    {
        return false;
    }
    // First do a exact match
    var baseurl = document.location.href;
    var links = $('#gsfx_brnd_LocalLinks a');
    var tstr;
    for (var i = 0; i < links.length; i++)
    {
        tstr = links[i].href
        if (tstr.toLowerCase() == baseurl.toLowerCase())
        {
            links[i].parentNode.className += ' gsfx_brnd_LocalLinkSelected';
            return;
        }
    }

    //url_exactMatch contains the url which need not do partial match 
    if ((typeof (url_exactMatch) != 'undefined') && (url_exactMatch != null) && (url_exactMatch != '') && (url_exactMatch.toLowerCase() == (location.pathname + location.search + location.hash).toLowerCase()))
        return;
    

    // If no exact match found try to do a partial match.

    baseurl = baseurl.split('?')[0].split('#')[0].replace(/( ?|^)default.aspx\b/gi, '');
    for (var i = 0; i < links.length; i++)
    {
        tstr = links[i].href.split('?')[0].split('#')[0].replace(/( ?|^)default.aspx\b/gi, ''); ;
        if (tstr.toLowerCase() == baseurl.toLowerCase())
        {
            links[i].parentNode.className += ' gsfx_brnd_LocalLinkSelected';
            return;
        }
    }
}
$(document).ready(gsfx_brnd_ToolbarSelection);
