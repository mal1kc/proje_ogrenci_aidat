// TODO write better version of this
let myInput = document.getElementById("Password");
let myInputVerify = document.getElementById("PasswordVerify");
let letter = document.getElementById("letter");
let capital = document.getElementById("capital");
let number = document.getElementById("number");
let length = document.getElementById("length");
let secondaryVerify = document.getElementById('pVerify');
let messageDiv = document.getElementById("message");

let lowerCaseLetters = /[a-z]/g;
let upperCaseLetters = /[A-Z]/g;
let numbers = /[0-9]/g;

// When the user clicks on the password field, show the message box

myInput.onfocus = function () {
  messageDiv.style.display = "block";
  messageDiv.childNodes.forEach(child => {
    if (child.nodeName == "P" || child.nodeName == "H3") {
      child.style.display = "block";
    }
  })
}

myInputVerify.onfocus = function () {
  messageDiv.style.display = "block";
  secondaryVerify.style.display = "block";
}

myInputVerify.onblur = function () {
  secondaryVerify.style.display = "none";
}

myInputVerify.onkeydown = passwordVerify;
myInputVerify.onkeyup = passwordVerify;

// When the user clicks outside of the password field, hide the message box
myInput.onblur = function () {
  messageDiv.childNodes.forEach(child => {
    if (child.nodeName == "P" || child.nodeName == "H3") {
      child.style.display = "none";
    }
  })
  //   messageDiv.style.display = "none";
}

// When the user starts to type something inside the password field

myInput.onkeyup = function () {
  // Validate lowercase letters
  if (myInput.value.match(lowerCaseLetters)) {
    letter.classList.remove("invalid");
    letter.classList.add("valid");
  } else {
    letter.classList.remove("valid");
    letter.classList.add("invalid");
  }

  // Validate capital letters
  if (myInput.value.match(upperCaseLetters)) {
    capital.classList.remove("invalid");
    capital.classList.add("valid");
  } else {
    capital.classList.remove("valid");
    capital.classList.add("invalid");
  }

  // Validate numbers
  if (myInput.value.match(numbers)) {
    number.classList.remove("invalid");
    number.classList.add("valid");
  } else {
    number.classList.remove("valid");
    number.classList.add("invalid");
  }

  // Validate length
  if (myInput.value.length >= 8) {
    length.classList.remove("invalid");
    length.classList.add("valid");
  } else {
    length.classList.remove("valid");
    length.classList.add("invalid");
  }
  passwordVerify();

}

function passwordVerify() {
  if (myInput.value == myInputVerify.value) {
    secondaryVerify.classList.remove("invalid");
    secondaryVerify.classList.add("valid");
  }
  else {
    secondaryVerify.classList.remove("valid");
    secondaryVerify.classList.add("invalid");
  }
}

function checkPasswordValidity() {
  passwordVerify();
  let truthArr = [
  ]
  messageDiv.childNodes.forEach(child => {
    if (child.nodeName == "P") {
      truthArr.push(child.classList.contains("valid"))
    }
  }
  )
  truthArr.push(secondaryVerify.classList.contains("valid"));
  return truthArr.every(isTruth);
}

function showInvalidAlert() {

  let isValid = checkPasswordValidity();
  let passwordInvalid = createPasswordInvalidAlert()
  if (!isValid) {
    messageDiv.append(passwordInvalid);
    messageDiv.style.display = "block";
  }
  else {
    deletePasswordInvalidAlert();
  }
  return isValid;
}

function createPasswordInvalidAlert() {

  let p = document.getElementById("passwordInvalid");
  if (p == null) {
    let pp = document.createElement("p");
    pp.setAttribute("class", "alert alert-danger");
    pp.setAttribute("id", "passwordInvalid");
    pp.innerText = "password is invalid";
    return pp;
  }
  else { return p };

}

function deletePasswordInvalidAlert() {
  let p = document.getElementById("passwordInvalid");
  if (p != null) {
    messageDiv.removeChild(p);
  }
}
