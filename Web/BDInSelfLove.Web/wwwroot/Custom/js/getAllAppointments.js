// .AspNetCore.Identity.Application cookie exists?
var userLoggedIn = getCookie('.AspNetCore.Identity.Application');
if (userLoggedIn === null) {
    return;
}
// fetch api/GetAll

// store in sessionStorage




function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) {
        return parts.pop().split(';').shift();
    }
}