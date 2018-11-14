///<reference path="Intellisense/MSXRMTOOLS.Xrm.Page.2016.js"/>

function Greet( e ) {
   var firstName = Xrm.Page.getAttribute("firstname").getValue();  
   console.log(firstName + " display in the debug environmnet console");
   alert("Welcome " + firstName );
}