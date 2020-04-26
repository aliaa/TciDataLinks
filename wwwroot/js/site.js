$(".nav-link[href='" + window.location.pathname + "'").addClass("active");

function ConvertInputsToBeAsArrayItem(elem, arrayName, index, changeId = true) {
    $(elem).find("input, select").each(function (i, e) {
        if (changeId)
            $(e).attr("id", arrayName + "_" + index + "__" + $(e).attr("id"));
        $(e).attr("name", arrayName + "[" + index + "]." + $(e).attr("name"));
    });
    return elem;
}

function showToast(text, type) {
    $("#toast-body").text(text);
    if (type) {
        $("#toast-body").addClass("alert-" + type);
    }
    $("#toast").toast({ delay: 1000 });
    $("#toast").toast("show");
}

function resetValidation() {
    $("form").removeData("validator");
    $("form").removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse("form");
}

var select2BaseSettings = {
    theme: "bootstrap4",
    language: "fa"
};
