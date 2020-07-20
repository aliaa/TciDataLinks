$(".nav-link[href='" + window.location.pathname + "'").addClass("active");

function ConvertInputsToBeAsArrayItem(elem, arrayName, index, changeId = true) {
    $(elem).find("input, select, textarea").each(function (i, e) {
        if (changeId)
            $(e).attr("id", arrayName + "_" + index + "__" + $(e).attr("id"));
        var name = $(e).attr("name");
        $(e).attr("name", arrayName + "[" + index + "]." + name);
        $(e).siblings("span[data-valmsg-for='" + name+"']").attr("data-valmsg-for", arrayName + "[" + index + "]." + name);
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

function removeOptionsOfDropDown(ddl, { initMsg = "انتخاب کنید", disabledInitMsg = true } = {}) {
    ddl.find("option").remove().end();
    if (initMsg) {
        var option = "<option " + (disabledInitMsg ? "disabled" : "") + " value='' selected>" + initMsg + "</option>";
        ddl.append(option);
    }
}