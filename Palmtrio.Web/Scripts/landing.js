function landingIndex_loadPage() {
    handleViewContact();
    handleViewTeamDetail();
    handleViewTeam();
}

function landingIndex_loadContactView(result) {
    landingIndex_swapPanelThree(result);

    $('.main-button').replaceWith("<a href=\"javascript:void(0);\" class=\"btn-team-view main-button button special\">View Team</a>");

    $('#main-btn-team-view').hide();

    handleViewTeam();
    handleContactSubmit();
}

function landingIndex_loadTeamDetailView(result) {
    landingIndex_swapPanelThree(result);

    $('#main-btn-team-view').show();

    handleRefreshTeam();
    handleRefreshContact();

    $('video').on("ended", function () {
        handleViewTeam();
        handleViewContact();
    });
}

function landingIndex_loadTeamView(result) {
    landingIndex_swapPanelThree(result);

    $('.main-button').replaceWith("<a href=\"javascript:void(0);\" class=\"btn-contact-view main-button button special icon fa-shopping-cart\">View Contact</a>");

    $('#main-btn-team-view').hide();

    handleViewTeamDetail();
    handleViewContact();
}

function landingIndex_swapPanelThree(result) {
    $('#three').attr('id', 'three-old');

    var $resultBuf = $(result).css('display', 'none');
    $('#three-old').after($resultBuf);

    $('#three, #three-old').slideToggle({
        duration: 400,
        queue: false,
        complete: function () {
            if ($('#three-old').length) {
                $('#three-old').remove();
            }
        }
    });

    $('html, body').animate({
        scrollTop: $('#three-old').offset().top,
        duration: 2000,
        queue: false
    });
}


function viewContact_verifyGoDaddySeal() {
    var bgHeight = "460";
    var bgWidth = "593";
    var goDaddyUrl = "https://seal.godaddy.com/verifySeal?sealID=";
    window.open(goDaddyUrl, 'SealVerification', 'menubar=no,toolbar=no,personalbar=no,location=yes,status=no,resizable=yes,fullscreen=no,scrollbars=no,width=' + bgWidth + ',height=' + bgHeight);
}

function viewContact_verifyAuthorizeNetSeal() {
    var authNetUrl = "https://verify.authorize.net/anetseal/?pid=";
    var currentUrl = "";
    if (window.location.href) {
        currentUrl = window.location.href;
    }
    else if (document.URL) {
        currentUrl = document.URL;
    }
    if (currentUrl) {
        authNetUrl += "&rurl=" + escape(currentUrl);
    }
    window.open(authNetUrl, 'AuthorizeNetVerification', 'width=600,height=400,dependent=yes,resizable=yes,scrollbars=yes,menubar=no,toolbar=no,status=no,directories=no,location=yes');
}

function viewContact_verifyMcAfeeCertified() {
    var mcAfeeUrl = "https://www.mcafeesecure.com/verify?host=Palmtrio.com";
    window.open(mcAfeeUrl, 'Certified Site mcafeesecure.com', 'width=600,height=400,dependent=yes,resizable=yes,scrollbars=yes,menubar=no,toolbar=no,status=no,directories=no,location=yes');
}


$(document).ready(function(){
    landingIndex_loadPage();
});