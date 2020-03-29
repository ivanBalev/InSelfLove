const offerController = function () {

    const validateUrl = function (str) {
        var expression = /[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)?/gi;
        var regex = new RegExp(expression);
        return str.match(regex);
    }

    const validateInput = function (context) {
        let isValid = context.params.product !== '' &&
            context.params.description !== '' &&
            context.params.price !== '' &&
            validateUrl(context.params.pictureUrl);

        return isValid;
    }

    const getCreateOffer = function (context) {

        helper.setHeaderView(context);
        helper.loadPartials(context)
            .then(function () {
                this.partial('https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/offers/create_pckmai.hbs')
            })

    }

    const postCreateOffer = function (context) {

        helper.notify('loading', 'Loading...');

        if (validateInput(context)) {

            const data = {
                ...context.params,
                creator: sessionStorage.getItem('username'),
            }

            requester.post('offers', 'appdata', 'Kinvey', data)
                .then(helper.handler)
                .then(() => {

                    helper.stopNotification();
                    helper.notify('success', 'You have successfully created an offer.');

                    context.redirect('#/dashboard');
                })
        } else {
            helper.stopNotification();
            helper.notify('error', 'Invalid data. Please try again.');
        }
    }

    const getDashboard = function (context) {
        requester.get('offers', 'appdata', 'Kinvey')
            .then(helper.handler)
            .then((data) => {
                data.forEach(d => {
                    d.isCreator = sessionStorage.getItem('username') === d.creator;
                });

                context.offers = data;
                helper.setHeaderView(context);

                helper.loadPartials(context, {
                    noOffers: 'https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/offers/noOffers_jwf6ge.hbs',
                    singleOffer: 'https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/offers/singleOffer_bdkio1.hbs'
                })
                    .then(function () {
                        this.partial('https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/offers/dashboard_mwqu1c.hbs')
                    });
            })
    }


    const showDetails = function (context) {

        requester.get(`offers/${context.params.id}`, 'appdata', 'Kinvey')
            .then(helper.handler)
            .then((offer) => {
                helper.setHeaderView(context);

                context.offer = offer;

                helper.loadPartials(context)
                    .then(function () {
                        this.partial('https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/offers/details_abww48.hbs')
                    })
            })
    }

    const getEditOffer = function (context) {

        requester.get(`offers/${context.params.id}`, 'appdata', 'Kinvey')
            .then(helper.handler)
            .then(offer => {
                helper.setHeaderView(context);
                context.offer = offer;

                helper.loadPartials(context)
                    .then(function () {
                        this.partial('https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/offers/edit_qvbvqr.hbs')
                    })
            });
    }

    const postEditOffer = function (context) {

        const id = context.params.id;
        delete context.params.id;

        const data = {
            ...context.params,
            creator: sessionStorage.getItem('username'),
        }

        helper.notify('loading', 'Loading...');

        if (validateInput(context)) {
            requester.put(`offers/${id}`, 'appdata', 'Kinvey', data)
                .then(helper.handler)
                .then(() => {
                    helper.stopNotification();
                    helper.notify('success', 'You have successfully edited this offer.');

                    context.redirect('#/dashboard');
                })
        } else {
            helper.notify('error', 'Invalid data. Please try again.');
        }

    }

    const getDeleteOffer = function (context) {

        requester.get(`offers/${context.params.id}`, 'appdata', 'Kinvey')
            .then(helper.handler)
            .then(offer => {
                helper.setHeaderView(context);
                context.offer = offer;

                helper.loadPartials(context)
                    .then(function () {
                        this.partial('https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/offers/delete_it8g3s.hbs')
                    })
            });
    }

    const postDeleteOffer = function (context) {
        const id = context.params.id;

        helper.notify('loading', 'Loading...');

        requester.del(`offers/${id}`, 'appdata', 'Kinvey')
            .then(helper.handler)
            .then(() => {
                helper.stopNotification();
                helper.notify('success', 'You have successfully deleted this offer.');

                context.redirect('#/dashboard')
            })
    }

    return {
        getCreateOffer,
        postCreateOffer,
        getDashboard,
        showDetails,
        getEditOffer,
        postEditOffer,
        postDeleteOffer,
        getDeleteOffer,
    }
}();