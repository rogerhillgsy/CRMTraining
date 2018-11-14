///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

qa = qa || {};
qa.training = qa.training ||  {};


qa.training.ShowBankLocation = function() {

    var location = Xrm.Page.getAttribute("new_banklocation").getValue();
    if (location != null) {
        alert("Location is " + location);
        console.log("Location is " + location);
    } else {
        alert("Location was not defined");
    }
}

qa.training.CheckSortCodeNumeric= function() {
    var regex = new RegExp("^\\d{1,}$");
    var sortcode = Xrm.Page.getAttribute("new_sortcode").getValue();
    if (sortcode != null) {
        var result = regex.test(sortcode);
        if (!result) {
            alert("Sortcode must be numeric");
        } else {
            alert("Sort code is OK");
        }
    } else {
        alert("Sortcode was not defined");
    }   
}

qa.training.onload = function () {
    Xrm.Page.getAttribute("new_banklocation").addOnChange(qa.training.ShowBankLocation);
    Xrm.Page.getAttribute("	new_sortcode").addOnChange(qa.training.CheckSortCode);
}
