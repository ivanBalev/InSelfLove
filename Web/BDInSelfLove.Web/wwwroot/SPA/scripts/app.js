const app = Sammy("#shop", function () {
    this.use('Handlebars', 'hbs');

    // Home
    this.get('#/home', homeController.getHome);

    // User
    this.get('#/login', userController.getLogin);
    this.post('#/login', userController.postLogin);

    this.get('#/register', userController.getRegister);
    this.post('#/register', userController.postRegister);

    this.get('#/logout', userController.logout);

    //Offers
    this.get('#/createOffer', offerController.getCreateOffer);
    this.post('#/createOffer', offerController.postCreateOffer);

    this.get('#/dashboard', offerController.getDashboard);

    this.get('#/edit/:id', offerController.getEditOffer);
    this.post('#/edit/:id', offerController.postEditOffer);

    this.get('#/delete/:id', offerController.getDeleteOffer);
    this.post('#/delete/:id', offerController.postDeleteOffer);

    this.get('#/details/:id', offerController.showDetails);

    this.get('#/buy', userController.buyProduct);
});

(() => {
    app.run('#/home');
})();