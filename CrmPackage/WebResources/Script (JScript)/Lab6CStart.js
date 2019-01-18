 ///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

Xrm.Utility.

var QALabc = {};

QALabc.OnFormLoad = function () {
    QALabc.addOnChange("new_sortcode", QALabc.ValidateForm);
    QALabc.addOnChange("new_banklocation", QALabc.ValidateForm);
    QALabc.addOnChange("new_balance", QALabc.CheckFundsForAccountType);
    QALabc.addOnChange("new_accounttype", QALabc.CheckFundsForAccountType);
    Xrm.Page.data.entity.addOnSave(QALabc.OnFormSave);
};

QALabc.OnFormSave = function(e) {
    var eventargs = e.getEventArgs();
    if (  eventargs.getSaveMode() == 70 // autosave
        || eventargs.getSaveMode() == 1 // save
        || eventargs.getSaveMode() == 2  // Save and close
            ) { 
        if ( QALabc.ValidateForm()) {
            eventargs.preventDefault();
        }
    }
}

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
        // sortcodecontrol.setNotification("The account code does not match the location: " + location);
        Xrm.Page.ui.setFormNotification("Sortcode: " + sortcode + " is not correct", "ERROR", "InvalidSortCode");
        result = true;
    } else {
        // sortcodecontrol.clearNotification();
        Xrm.Page.ui.clearFormNotification("InvalidSortCode");
    }
    return result;
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

QALabc.CheckFundsForAccountType = function () {
    var accountType = Xrm.Page.getAttribute("new_accounttype");
    accountType = !!accountType ? accountType.getSelectedOption().value : null;
    var balanceControl = Xrm.Page.getControl("new_balance");
    var balance = Xrm.Page.getAttribute("new_balance");
    balance = !!balance ? balance.getValue() : null;

    if (accountType == 100000000 && balance < 50) {
        balanceControl.setNotification("Savings account number have more than 50");
    } else {
        balanceControl.clearNotification();
    }
}

