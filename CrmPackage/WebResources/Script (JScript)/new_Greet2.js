///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

function Greet2() {
    var name = Xrm.Page.getAttribute("name").getValue();
    alert("welcome " + name);
}