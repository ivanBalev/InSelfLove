const helper = function () {

    const handler = function (response) {

        if (response.status >= 400) {
            stopNotification();
            notify('error', response.statusText);
            throw new Error(`Something went wrong. Error: ${response.statusText}`);
        }

        if (response.status !== 204) {
            response = response.json();
        }

        return response;
    };

    const passwordCheck = function (params) {
        return params.password === params.rePassword;
    };

    const setHeaderView = function (context) {
        const loggedIn = sessionStorage.getItem('authtoken') !== null;

        if(loggedIn){
            context.loggedIn = loggedIn;
            context.username = sessionStorage.getItem('username');
        }

        return loggedIn ? true : false;
    }

    const loadPartials = function (context, externalPartials) {
        let defaultPartials = {
            header: "https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/common/header_n0mzv5.hbs",
            footer: "https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/common/footer_utpcdg.hbs"
        };

        if (externalPartials) {
            for (const key in externalPartials) {
                const element = externalPartials[key];
                
                defaultPartials[key] = element;
            }
        }
        return context.loadPartials(defaultPartials);
    }

    const notify = function (type, textContent) {
        let element = document.getElementById(`${type}Notification`);
        element.textContent = textContent;
        element.style.display = 'block';
        element.addEventListener('click', event => event.currentTarget.style.display = 'none');

    }

    const stopNotification = function () {
        Array.from(document.getElementById('notifications').children)
            .forEach(c => {
                c.style.display = 'none';
            });
    }

    return {
        handler,
        passwordCheck,
        setHeaderView,
        loadPartials,
        notify,
        stopNotification
    }
}();