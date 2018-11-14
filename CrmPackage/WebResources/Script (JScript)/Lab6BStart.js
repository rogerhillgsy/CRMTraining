///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

var QALab = {};

QALab.OnFormLoad = function() {
    QALab.LockAccountNumber();
    QALab.addOnChange("new_sortcode", QALab.CheckLocationAgainstSortCode);
    QALab.addOnChange("new_banklocation", QALab.CheckLocationAgainstSortCode);
    QALab.addOnChange("new_balance", QALab.CheckFundsForAccountType);
    QALab.addOnChange("new_accounttype", QALab.CheckFundsForAccountType);
};

QALab.addOnChange = function(attribute, callback) {
    var attr = Xrm.Page.getAttribute(attribute);
    if (attr != null) attr.addOnChange(callback);
}

QALab.LockAccountNumber = function() {
    var type = Xrm.Page.ui.getFormType();
    control = Xrm.Page.getControl("new_accountnumber");
    if (control != null && type == 1) {
        control.setDisabled(false);
    } else {
        !!control && control.setDisabled(true);
    }
}

QALab.CheckLocationAgainstSortCode = function() {
    var location = Xrm.Page.getAttribute("new_banklocation");
    location = !!location  && location.getValue() ;
    var sortcode = Xrm.Page.getAttribute("new_sortcode");
    sortcode = !!sortcode && sortcode.getValue() ;
    var sortcodecontrol = Xrm.Page.getControl("new_sortcode");
    var uLoc = location.toUpperCase();

    if (uLoc == "UK" && sortcode.substr(0, 3) == "044") {
        sortcodecontrol.clearNotification();
    } else if (uLoc == "UK") {
        sortcodecontrol.setNotification("This sortcode does not match a UK pattern");
    } else if( uLoc == "USA" && sortcode.substr(0, 2) == "01") {
        sortcodecontrol.clearNotification();
    } else if (uLoc == "USA") {
        sortcodecontrol.setNotification("THis sortcode does not match a US pattern");
    }
}

QALab.CheckFundsForAccountType = function() {
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