///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

var QALabc = {};

QALabc.OnFormLoad = function () {
    QALabc.addOnChange("new_sortcode", QALabc.ValidateForm);
    QALabc.addOnChange("new_banklocation", QALabc.ValidateForm);
    //QALab.addOnChange("new_balance", QALab.CheckFundsForAccountType);
    //QALab.addOnChange("new_accounttype", QALab.CheckFundsForAccountType);
};

QALabc.addOnChange = function (attribute, callback) {
    var attr = Xrm.Page.getAttribute(attribute);
    if (attr != null) attr.addOnChange(callback);
}

QALabc.ValidateForm = function() {
    var location = Xrm.Page.getAttribute("new_banklocation");
    location = !!location && location.getValue();
    var sortcode = Xrm.Page.getAttribute("new_sortcode");
    sortcode = !!sortcode && sortcode.getValue();
    var sortcodecontrol = Xrm.Page.getControl("new_sortcode");
    var result = false;

    if (!QALabc.ValidSortCode(location, sortcode)) {
        sortcodecontrol.setNotification("The account code does not match the location: " + location);
    } else {
        sortcodecontrol.clearNotification();
    }
}

QALabc.ValidSortCode = function (location, sortcode) {
    var rv = false;
    var pattern = "\\d{8}";
    switch (location.toLowerCase()) {
    case "uk":
    case "gb":
        pattern = "^044[1-9]{5}$";
        break;
    case "us":
    case "usa":
            pattern = "^01[1-9]{6}$";
        break;
        default:
            break;
    }
    return (new RegExp(pattern)).test(sortcode);
}

