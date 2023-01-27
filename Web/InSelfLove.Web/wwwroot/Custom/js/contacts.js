const csfrToken = document.querySelector("form input[name=__RequestVerificationToken]").value;
const siteKey = "6LeUiIAfAAAAAHHzOdr30-6LdSQIIfAAAAAO787M08KaNncgzfLpOO6VknjOeF";
const expectedAction = "SUBMIT_CONTACT_FORM";

document.getElementById('contacts-form').addEventListener('submit', function (e) {
    // Prevent default http request
    e.preventDefault();

    // Gather data from all contact form fields
    const data = Object.fromEntries(new FormData(e.target).entries());

    // Validate required fields
    if (!isEmail(data.Email)) {
        alert('Невалиден имейл адрес');
        return;
    }
    if (data.Message.length < 30) {
        alert('Моля, въведете повече от 30 символа.');
        return;
    }

    // grecaptcha comes from the head script we insert in Contacts.cshtml
    grecaptcha.enterprise.ready(function () {
        grecaptcha.enterprise.execute(siteKey, { action: expectedAction })
            .then(function (token) {
                data.RecaptchaExpectedAction = expectedAction;
                data.RecaptchaToken = token;

                postData('/Home/Contacts', data, csfrToken)
                    .then(res => {
                        // Insert response html (status message)
                        document.getElementsByTagName('nav')[0].after(htmlToElement(res));

                        // Reset contact form
                        document.getElementById('contacts-form').reset();
                    });
            });
    });
});

function htmlToElement(html) {
    var template = document.createElement('template');
    html = html.trim();
    template.innerHTML = html;
    return template.content.firstChild;
}

function isEmail(email) {
    var emailFormat = /^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$/;
    if (email !== '' && email.match(emailFormat)) {
        return true;
    }
    else {
        return false;
    }
}

async function postData(url = '', data = {}, csfrToken) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': csfrToken,
        },
        body: JSON.stringify(data)
    });
    return response.text();
}