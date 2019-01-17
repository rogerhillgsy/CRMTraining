function getDataParam() {

    //Get the any query string parameters and load them into the vals array 

    var vals = new Array();

    if (location.search != "") {

        vals = location.search;
        parseDataValue(vals);








    }

    function parseDataValue(datavalue) {

        if (datavalue != "") {
            var vals = new Array();

            var message = document.createElement("p");

            message.innerText = "These are the data parameters values that were passed to this page:";

            document.body.appendChild(message);

            vals = decodeURIComponent(datavalue).split("&");

            for (var i in vals) {

                vals[i] = vals[i].replace(/\+/g, " ").split("=");

            }

            //Create a table and header using the DOM 

            var oTable = document.createElement("table");
            var oTHead = document.createElement("thead");
            var oTHeadTR = document.createElement("tr");
            var oTHeadTRTH1 = document.createElement("th"); oTHeadTRTH1.innerText = "Parameter";
            var oTHeadTRTH2 = document.createElement("th"); oTHeadTRTH2.innerText = "Value";
            oTHeadTR.appendChild(oTHeadTRTH1);
            oTHeadTR.appendChild(oTHeadTRTH2);
            oTHead.appendChild(oTHeadTR);
            oTable.appendChild(oTHead);
            var oTBody = document.createElement("tbody");

            //Loop through vals and create rows for the table

            for (var i in vals) {
                var oTRow = document.createElement("tr");
                var oTRowTD1 = document.createElement("td");
                oTRowTD1.innerText = vals[i][0];
                var oTRowTD2 = document.createElement("td");
                oTRowTD2.innerText = vals[i][1];
                oTRow.appendChild(oTRowTD1);
                oTRow.appendChild(oTRowTD2);
                oTBody.appendChild(oTRow);
            }

            parent.Xrm.WebApi.online.retrieveRecord("rbh_module7test", "2B639D85-771A-E911-A977-0022480149C2", "?$select=createdon").then(
                function success(result) {
                    var createdon = result["createdon"];
                    var d = document.createElement("div");
                    d.innerText = createdon.toString();
                    document.body.appendChild(d);
                },
                function (error) {
                    Xrm.Utility.alertDialog(error.message);
                }
            );

            oTable.appendChild(oTBody);
            document.body.appendChild(oTable);

        }

        else {

            noParams();

        }

    }

    function noParams() {
        var message = document.createElement("p");

        message.innerText = "No data parameter was passed to this page"; document.body.appendChild(message);

    }
}