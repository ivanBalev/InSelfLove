const homeController = function () {

    const getHome = function (context) {

        helper.setHeaderView(context);
        helper.loadPartials(context)
            .then(function () {
                this.partial('https://res.cloudinary.com/dzcajpx0y/raw/upload/v1585493652/hbs/views/home/homePage_dvcnm1.hbs')
            });
    }

    return {
        getHome
    }
}();