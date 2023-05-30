function onBegin() {
    $("#preloader").removeClass('loaded');
    $("#preloader").addClass('loaderActive');
}

function onComplete() {
    $("#preloader").removeClass('loaderActive');
    $("#preloader").addClass('loaded');
}

function onFailed() {
    $("#preloader").removeClass('loaderActive');
    $("#preloader").addClass('loaded');
}

function onPaySuccess(response) {
    $("#response").html(response);
    $("#preloader").removeClass('loaderActive');
    $("#preloader").addClass('loaded');
}


function ActiveLink() {
    $(".rd-nav-item").each(function () { $(this).removeClass('active'); });
    var pathname = window.location.pathname.split("/")[1];
    if (pathname.toLowerCase() === "page") {
        pathname = window.location.pathname.split("/")[2];
    }
    switch (pathname.toLowerCase()) {
        case "":
        case "conteststandings":
            $(".home").addClass("active");
            break;
        case "about-us":
            $(".about_us").addClass("active");
            break;
        
    }
}