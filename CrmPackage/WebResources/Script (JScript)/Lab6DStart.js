QALabd = function() {

    var onLoad = function() {
        addPhoneCheck("telephone1");
        addPhoneCheck("mobilephone");
        addPhoneCheck("fax");    
    }

    var formatPhoneNumber = function (context) {
        var oField = context.getEventSource().getValue();
        var sTmp = oField;
        if (typeof (oField) != "undefined" && oField != null) {
            sTmp = oField.replace(/[^0-9]/g, "");
            switch (sTmp.length) {
            case 10:
                sTmp = "(" + sTmp.substr(0, 3) + ") " + sTmp.substr(3, 3) + "-" + sTmp.substr(6, 4);
                break;
            default:
                alert("Phone must contain 10 numbers.");
                break;
            }
        }
        context.getEventSource().setValue(sTmp);
    }

    var addPhoneCheck = function (attribute)
    {
        var attr = Xrm.Page.getAttribute(attribute);
        if (!!attr) {
            attr.addOnChange(formatPhoneNumber);
        }
    }

    var checkPhoneFormat = function (e, a, b) {
        debugger;
        var attribute = e.getEventSource();
        if (!!attribute) {
            e.getEventSource();
        }
    }

    return { OnLoad : onLoad }
}();

