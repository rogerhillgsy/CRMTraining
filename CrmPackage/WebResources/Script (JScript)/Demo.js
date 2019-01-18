function  Greet()
{
    var firstname = formExecutionContext.getAttribute("firstname").getValue();

    console.log(firstname + " display in the debuug environment console");

    alert("Welcome " + firstname);
}