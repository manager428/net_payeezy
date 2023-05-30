var jsonFilterRequest = {};
var __pageIndex = 1;
var __pageCount = 10000000;
var requestUrl = actionURL("filtercontest", "contest");
jsonFilterRequest.priceFilter = 1;
jsonFilterRequest.eventId = window.location.href.substring(window.location.href.lastIndexOf("/") + 1);
function CatLoader() { };
CatLoader.prototype.Show = function () {
    $("#contestlist").append('<div class="loader-overlay"><div class="overlay-rel"><div class="overlay-div custom-shadow">' + loaderHtml() + '</div></div></div>');
}
CatLoader.prototype.Hide = function () {
    $("#ProductListContainer").find(".loader-overlay").remove();
}
var Loader = new CatLoader();
$(function () {
    $("#_price").slider({
        range: true,
        orientation: "horizontal",
        min: 0,
        max: 300,
        values: [0, 300],

        slide: function (event, ui) {
            $(event.target).find('#min_price').html();
            $(event.target).find('#max_price').html();
        },
        stop: function (event, ui) {
            var data = { from: $('#min_price').html(), to: $('#max_price').html() };
            jsonFilterRequest.PriceTo = data.to;
            jsonFilterRequest.PriceFrom = data.from;
            __pageIndex = 1;
            jsonFilterRequest.CurrentPageID = 1;
            console.log(jsonFilterRequest);
            updateURL(updateQueryString("priceto", data.to, updateQueryString("pricefrom", data.from)));
            GetRecords(true);
        }
    });
    $("#_price1").slider({
        range: true,
        orientation: "horizontal",
        min: 0,
        max: 50000,
        values: [0, 50000],

        slide: function (event, ui) {
            $(event.target).find('#min_price').html();
            $(event.target).find('#max_price').html();
        },
        stop: function (event, ui) {
            var data = { from: $('#min_price').html(), to: $('#max_price').html() };
            jsonFilterRequest.PriceTo = data.to;
            jsonFilterRequest.PriceFrom = data.from;
            __pageIndex = 1;
            jsonFilterRequest.CurrentPageID = 1;
            updateURL(updateQueryString("priceto", data.to, updateQueryString("pricefrom", data.from)));
            GetRecords(true);
        }
    });
});

function updateURL(newurl) {
    if (history.pushState) {
        window.history.pushState({ path: newurl }, '', newurl);
    }
}

function updateQueryString(key, value, uri) {
    value = encodeURIComponent(value);
    // Use window URL if no query string is provided
    if (!uri) { uri = window.location.href; }

    // Create a dummy element to parse the URI with
    var a = document.createElement('a'),

        // match the key, optional square bracktes, an equals sign or end of string, the optional value
        reg_ex = new RegExp(key + '((?:\\[[^\\]]*\\])?)(=|$)(.*)'),

        // Setup some additional variables
        qs,
        qs_len,
        key_found = false;

    // Use the JS API to parse the URI
    a.href = uri;

    // If the URI doesn't have a query string, add it and return
    if (!a.search) {

        a.search = '?' + key + '=' + value;

        return a.href;
    }

    // Split the query string by ampersands
    qs = a.search.replace(/^\?/, '').split(/&(?:amp;)?/);
    qs_len = qs.length;

    // Loop through each query string part
    while (qs_len > 0) {

        qs_len--;

        // Check if the current part matches our key
        if (reg_ex.test(qs[qs_len])) {

            // Replace the current value
            if (value != '') {
                qs[qs_len] = qs[qs_len].replace(reg_ex, key + '$1') + '=' + value;
            }
            else {
                qs[qs_len] = '';
            }

            key_found = true;
        }
    }

    // If we haven't replaced any occurences above, add the new parameter and value
    if (!key_found) { qs.push(key + '=' + value); }

    // Set the new query string
    qs = qs.filter(function (x) { return x != '' });
    a.search = '?' + qs.join('&');

    return a.href;
}

function GetRecords(isFilter) {
    requestAjax(isFilter);
}

function requestAjax(isFilter) {
    if (jsonFilterRequest.CurrentPageID == 1) {
        jsonFilterRequest.IsFirstLoad = true;
    }
    else {
        jsonFilterRequest.IsFirstLoad = false;
    }
    var data = { from: $('#min_price').html(), to: $('#max_price').html() };
    jsonFilterRequest.PriceTo = data.to;
    jsonFilterRequest.PriceFrom = data.from;
    console.log(jsonFilterRequest);
    $.ajax({
        type: "POST",
        url: requestUrl,
        data: jsonFilterRequest,
        success: function (response) {
            var x = $(document.querySelector("#contestlist"));
            var html = "";
            if (isFilter) {
                x.html("");
            }
            if (response.indexOf("unit-md-horizontal") > -1) {
                x.append(response);
            }
            else {
                x.append("<article class='heading-component mt - 20'>\
                    <div class= 'heading-component-inner1'>\
                    <h5 style='text-align:center'>Contest Not Open Yet\
                        </h5>\
                    </div>\
                </article><a href='/events' class='button button-primary mt-20' style='left:43%'>Back</a>");
            }
            $(".loader").hide();
            $('html, body').scrollTop(0);
        },
        error: function (e) {
            Loader.Hide();
        }
    });
}

function priceFilter(th) {
    var x = $(th);
    jsonFilterRequest.priceFilter = th.options[th.selectedIndex].getAttribute("value");
    GetRecords(true);
}
