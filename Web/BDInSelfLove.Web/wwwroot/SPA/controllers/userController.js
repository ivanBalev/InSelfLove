const userController = function () {

    const showProfile = function (context) {

        let user = sessionStorage.getItem('username');
        let offersAdded = [];

        requester.get('offers', 'appdata', 'Kinvey')
            .then(helper.handler)
            .then(data => {
                offersAdded = data.filter(d => d.creator === user);
            })
            .then(() => {
                helper.setHeaderView(context);
                context.username = user;
                context.eventsCount = offersAdded.length;
                context.events = offersAdded;

                helper.loadPartials(context)
                    .then(function () {
                        this.partial('./SPA/views/user/profilePage.hbs')
                    })
            });
    }

    const getRegister = function (context) {
        helper.loadPartials(context)
            .then(function () {
                this.partial('./SPA/views/user/registerPage.hbs')
            });
    };

    const getLogin = function (context) {
        helper.loadPartials(context)
            .then(function () {
                this.partial('./SPA/views/user/loginPage.hbs')
            })
    };

    const postRegister = function (context) {
        let username = context.params.username;
        let password = context.params.password;
        let rePassword = context.params.rePassword;

        if (password === rePassword &&
            username !== '' &&
            password !== '') {

            const payload = {
                username,
                password,
                boughtItems: 0,
            }

            helper.notify('loading', 'Loading...');

            requester.post('', 'user', 'Basic', payload)
                .then(helper.handler)
                .then(data => {

                    helper.stopNotification();
                    helper.notify('success', 'You have registered successfully.');

                    sessionStorage.setItem('username', data.username);
                    sessionStorage.setItem('authtoken', data._kmd.authtoken);

                    context.redirect('#/home');
                });
        } else {
            helper.notify('error', 'Invalid data. Please try again.');
        }
    };

    const postLogin = function (context) {
        const payload = {
            username: context.params.username,
            password: context.params.password
        }

        helper.notify('loading', 'Loading...');

        requester.post('login', 'user', 'Basic', payload)
            .then(helper.handler)
            .then(data => {

                helper.stopNotification();
                helper.notify('success', 'You have logged in successfully.');

                sessionStorage.setItem('username', data.username);
                sessionStorage.setItem('authtoken', data._kmd.authtoken);

                context.redirect('#/home');
            });
    };

    const logout = function (context) {

        requester.post('_logout', 'user', 'Kinvey')
            .then(helper.handler)
            .then(() => {
                helper.notify('success', 'You have successfully logged out,');
                sessionStorage.clear();

                context.redirect('#/home');
            });
    };

    const buyProduct = function () {
        
        requester.get('', 'user', 'Kinvey')
            .then(helper.handler)
            .then((data) => {
                let user = data
                .find(u => u.username === sessionStorage.getItem('username'));
                
                let payload = {
                    username: user.username,
                    boughtItems: user.boughtItems++,
                }

                requester.put(user._id, 'user', 'Kinvey', payload)
                .then(helper.handler)
                .then(() => {
                    helper.notify('success', 'You have successfully bought this item.');
                })
            });
    }

    return {
        getRegister,
        postRegister,
        getLogin,
        postLogin,
        logout,
        showProfile,
        buyProduct
    }
}();