///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

var QALab6 = {};

QALab6.setParentAccountIdFilter = function(executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.getControl("parentaccountid").addPreSearch(QALab.filterCustomerAccounts);
}

QALab.filterCustomerAccounts = function(executionContext) {
    var formContext = executionContext.getFormContext();
    var customerAccountFilter = "<filter type='and'><condition attribute='accountcategorycode' operator='eq' value='1'/></filter>";
    formContext.getControl("parentaccountid").addCustomFilter(customerAccountFilter, "account");
}