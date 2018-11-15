///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

mod7 = function() {

    var POSTCODE = "new_postcode";

    var onLoad = function() {
        addOnChange("cr766_name", getMatchingNames);
        addOnKeyPress(POSTCODE, checkPostcodeOnKey);
    }

    var addOnChange = function(attribute, callback) {
        var attr = Xrm.Page.getAttribute(attribute);
        if (attr != null) attr.addOnChange(callback);
    }

    var addOnKeyPress = function(control, callback) {
        var con = Xrm.Page.getControl(control);
        if (con != null) con.addOnKeyPress(callback);
    }


    var getAttribute = function(attribute) {
        var rv = Xrm.Page.getAttribute(attribute);
        if (!!rv) {
            rv = rv.getValue();
        }
        return rv;
    }

    var getControl = function(control) {
        var rv = Xrm.Page.getControl(control);
        return rv;
    }

    var clearDescription = function() {
        var desc = Xrm.Page.getAttribute("new_description");
        if (!!desc) {
            desc.setValue("");
        }
    }
    var appendDescription = function(s) {
        var desc = Xrm.Page.getAttribute("new_description");
        if (!!desc) {
            var current = desc.getValue();
            current = current || "";
            desc.setValue(current + s + "\r\n");
        }
    }

    var getMatchingNames = function() {
        var name = getAttribute("cr766_name");
        clearDescription();
        Xrm.WebApi.online.retrieveMultipleRecords("cr766_module7test",
            "?$select=cr766_name&$filter=startswith(cr766_name,'" + name + "')").then(
            function success(results) {
                for (var i = 0; i < results.entities.length; i++) {
                    var cr766_name = results.entities[i]["cr766_name"];
                    appendDescription(cr766_name);
                }
            },
            function(error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );
    }

    var postCodeHasError = function() {
        var postcode = getControl(POSTCODE);
        postcode.setNotification("This does not look like a postcode");
    }

    var checkPostcodeAsync = function (postcode, success, error) {
        try {
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: "https://api.postcodes.io/postcodes/" + postcode + "/autocomplete",
                beforeSend: function (XMLHttpRequest) {

                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                async: true,
                success: function (data, textStatus, xhr) {
                    var result = data;
                    if (data.result) {
                        success(result);
                    } else {
                        error();
                    }
                },
                error: error
            });
        } catch (e) {
            alert("Bad Ajax call" + e);
        }
    };

    var checkPostcode = function() {
        var postcode = getAttribute(POSTCODE);

        if (!!postcode) {
            checkPostcodeAsync(postcode,
                function() {
                    clearDescription();
                    appendDescription("Postcode " + postcode + " was ok");
                    var postcode = getControl(POSTCODE);
                    postcode.clearNotification();
                },
                postCodeHasError());
        }
    }

    var checkPostcodeOnKey = function(ext) {
        //var postcodecon = getControl(POSTCODE);
        //var resultSet = {
        //    results: new Array(),
        //    commands: {
        //        id: "postcode_commands",
        //        label: "learn More",
        //        action: function() {
        //            console.log("Learn more");
        //        }
        //    }
        //}
        //resultSet.results.push({ id: 1, fields: ["TW1", "a"] });
        //resultSet.results.push({ id: 2, fields: ["TW2", "a"] });

        //ext.getEventSource().showAutoComplete(resultSet);
        // ext.getEventSource().hideAutoComplete(resultSet);
        var postcode = getAttribute(POSTCODE);
        if (!!postcode && postcode.length > 2) {
            checkPostcodeAsync(postcode,
                function(result) {
                    var resultSet = { results: new Array() };
                
                    for (var i = 0; i < result.result.length; i++) {
                        resultSet.results.push({ id: i, fields: [result.result[i]] });
                    }
                    // resultSet = { results: result.result };

                    ext.getEventSource().clearNotification();
                    ext.getEventSource().showAutoComplete(resultSet);
                },
                function() {
                    ext.getEventSource().hideAutoComplete();
                    postCodeHasError();
                }
            );
        }
    }

    return { OnLoad: onLoad };
}();

