QALabd = function() {

    var onLoad = function() {
        addPhoneCheck("telephone1");
        addPhoneCheck("mobilephone");
        addPhoneCheck("fax");    
    }

    var addPhoneCheck = function ( attribute )
    {
        var attr = Xrm.Page.getAttribute(attribute);
        if (!!attr) {
            attr.addOnChange(checkPhoneFormat);
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


function TrainingButton() {
    alert("You pressed a button");
}

function CreateDuplicate() {
    alert("Trying to create duplicate");


}
