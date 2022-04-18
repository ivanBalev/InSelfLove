const csfrToken = document.querySelector("form input[name=__RequestVerificationToken]").value;
const siteKey = "6LdSQIIfAAAAAO787M08KaNncgzfLpOO6VknjOeF";
const expectedAction = "SUBMIT_CONTACT_FORM";

document.getElementById('contacts-form').addEventListener('submit', function (e) {
    e.preventDefault();
    const data = Object.fromEntries(new FormData(e.target).entries());

    // Validation
    if (!isEmail(data.Email)) {
        alert('Invalid email');
        return;
    }
    if (data.Message.length < 30) {
        alert('Message needs to be at least 30 characters long.');
        return;
    }

    grecaptcha.enterprise.ready(function () {
        grecaptcha.enterprise.execute(siteKey, { action: expectedAction })
            .then(function (token) {
                data.RecaptchaExpectedAction = expectedAction;
                data.RecaptchaToken = token;

                postData('/Home/Contacts', data, csfrToken)
                    .then(res => {
                        document.getElementsByTagName('nav')[0].after(htmlToElement(res));
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