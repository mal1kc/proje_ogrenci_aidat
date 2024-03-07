let passField = document.getElementById("Password");
let emailField = document.getElementById("EmailAddress");
let messageDiv = document.getElementById("message");
let emailptrn = /^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/;
messageDiv.style.display = "block";

function checkOnSubmit(){
    return checkValidity();
}

function checkValidity(field_name){
    switch (field_name) {
        case "passField":
            // TODO :  idk what to check for passField but know need to check somethin but what ?
            return true;
        case "emailField":
            let emailmatch  = emailField.value.match(emailptrn);
            return emailmatch != null && emailmatch.length == 1 ;
        default:
            let truthArr = [];
            truthArr.push(checkValidity("emailField"));
            truthArr.push(checkValidity("passField"));
            return truthArr.every(isTruth);
    }
}

function onkeyUp(field_name){
    if (!checkValidity(field_name)){
        messageDiv.style.display = "block";
        showAlertP(field_name)
        return;
    }
    hideAlertP(field_name)
    messageDiv.style.display = "none";
    return;
}

function showAlertP(field_name){
    let alertP = createAlertP(field_name)
    alertP.style.display = "block";
}

function hideAlertP(field_name){
    let alertP = createAlertP(field_name);
    alertP.style.display = "none";
}


function createAlertP(field_name){
    let alertP = document.getElementById(field_name+"alert");
    if (alertP == null){
        alertP = document.createElement("p");
        alertP.setAttribute("class", "alert alert-danger");
        alertP.setAttribute("id",field_name+"alert");
        messageDiv.appendChild(alertP);
        switch(field_name){
            case "passField":
                alertP.innerText = "Password has invalid characters !";
                // TODO : prob not need
                break;
            case "emailField":
                alertP.innerText = "Email has invalid characters !";
                break;
        }
    }
    return alertP;
}

passField.onkeyup = function(){ onkeyUp("passField")} ;
emailField.onkeyup = function(){onkeyUp("emailField")};
