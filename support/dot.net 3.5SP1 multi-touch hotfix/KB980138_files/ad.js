var gsfx_brnd_ad_pushstartdate = new Date('September 8, 2010');
var gsfx_brnd_ad_pushenddate = new Date('July 1, 2017');
var isload = false;

var webad = false;
var webadparam="&AP=1089&PG=T4LV32"

$(document).ready(function () {
	if (webad){
		gsfx_brnd_flyoutad_download();
	}
	gsfx_brnd_adspaceload();
	$($('#ad_brnd_corpmarketing_space a').get(0)).click(gsfx_brnd_flyoutad_open);
	$('#ad_brnd_corpflyoutad_dontshow').click(function () { gsfx_brnd_flyoutad_close(true); });
	$('#ad_brnd_corpflyoutad_close').click(gsfx_brnd_flyoutad_close);
});

function gsfx_brnd_adspaceload() {
	if($('#ad_brnd_corpflyoutad_frame').length<=0){
		return;
	}

	$('#ad_brnd_corpflyoutad_frame').css({ opacity: 0 });
	$('#ad_brnd_corpmarketing_space a').get(0).href = 'javascript:void(0);';
	var hasseen = fetchcookieval('mcI');
	var nevershow = fetchcookieval('mcUA');
	var ct1 = (hasseen) ? new Date(hasseen) : null;
	var ct2 = (nevershow) ? new Date(nevershow) : null;
	var startdate = (gsfx_brnd_ad_pushstartdate) ? new Date(gsfx_brnd_ad_pushstartdate) : null;
	var enddate = (gsfx_brnd_ad_pushenddate) ? new Date(gsfx_brnd_ad_pushenddate) : null;
	var curdt = new Date();
	
	if (curdt > enddate) {
		return null;
	}
	if (ct1 < startdate || ct2 < startdate) {
		hasseen == null;
		nevershow == null;
	}
	if (!nevershow) {
		if (hasseen) {
			var dt = new Date(hasseen);
			if (curdt > dt) {
				gsfx_brnd_flyoutad_open(true);
			}
		}
		else {
			gsfx_brnd_flyoutad_open(true);
		}
	}
}

function gsfx_brnd_flyoutad_open(autoclose) {
	$('#ad_brnd_corpflyoutad_frame').css('display', 'block');
	if (webad){
		gsfx_brnd_flyoutad_download();
	}
	
	$('#ad_brnd_corpflyoutad_frame').animate({ width: $('#ad_brnd_corpflyoutad').css('width'), height: $('#ad_brnd_corpflyoutad').css('height'), opacity: 1 }, 750, function () {
		$('#ad_brnd_corpflyoutad_frame').css('display', 'block');
		var d = new Date();
		var cs = 'mcI=' + d.toUTCString(d.setDate(d.getDate() + 7)) + ';expires=' + d.toGMTString(d.setFullYear(d.getFullYear() + 1)) + '; Domain=microsoft.com;path=/';
		document.cookie = cs;
		if (autoclose === true) {
			setTimeout(gsfx_brnd_flyoutad_close, 4000);
		}
	});
}

function gsfx_brnd_flyoutad_download() {
	if (isload == false) {
		dapMgr.enableACB("ad_brnd_corpflyoutad", false);
		dapMgr.renderAd("ad_brnd_corpflyoutad", webadparam, 300, 250);
		isload = true;
	}
}

function gsfx_brnd_flyoutad_close(perm) {
	$('#ad_brnd_corpflyoutad_frame').animate({ width: 0, height: 0, opacity: 0 }, 750, null, function () {
		$('#ad_brnd_corpflyoutad_frame').css('display', 'none');
		if (perm === true) {
			gsfx_brnd_dontshowad();
		}
	});
}

function gsfx_brnd_dontshowad() {
	var d = new Date();
	var cs = 'mcUA=' + d.toUTCString() + ';expires=' + d.toGMTString(d.setFullYear(d.getFullYear() + 1)) + '; Domain=microsoft.com;path=/';
	document.cookie = cs;
}