$(".nav-link[href='" + window.location.pathname + "'").addClass("active");

function ConvertInputsToBeAsArrayItem(elem, arrayName, index) {
    $(elem).find("input, select").each(function (i, e) {
        $(e).attr("id", arrayName + "_" + index + "__" + $(e).attr("id"));
        $(e).attr("name", arrayName + "[" + index + "]." + $(e).attr("name"));
    });
    return elem;
}
