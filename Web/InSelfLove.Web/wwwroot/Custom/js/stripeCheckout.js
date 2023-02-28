let elements;
let stripe;

document.addEventListener('DOMContentLoaded', async () => {
    const { publishableKey } = await fetch('/stripe/config').then(r => r.json());

    // TODO: locale per user
    stripe = Stripe(publishableKey, { locale: 'bg' });

    checkStatus();

    document
        .querySelector("#payment-form")
        .addEventListener("submit", handleSubmit);
})

// Fetches a payment intent and captures the client secret
async function initialize(appointmentId) {
    setLoading(true);

    const { clientSecret } = await fetch('/stripe/createpaymentintent', {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: appointmentId,
    }).then(r => r.json());

    elements = stripe.elements({ clientSecret });

    const paymentElementOptions = {
        layout: "tabs",
        defaultCollapsed: true,
        wallets: {
            applePay: 'never',
            googlePay: 'never'
            }
    };

    const paymentElement = elements.create("payment", paymentElementOptions);

    paymentElement.mount("#payment-element");

    paymentElement.on('loaderstart', function () {
        setLoading(false);
        document.querySelector('#payment-form #submit').classList.remove('hidden');
    })
}

async function handleSubmit(e) {
    e.preventDefault();
    setLoading(true);

    const { error } = await stripe.confirmPayment({
        elements,
        confirmParams: {
            return_url: window.location.href.split('?')[0],
        },
    });

    // This point will only be reached if there is an immediate error when
    // confirming the payment.
    bootstrap.Modal.getOrCreateInstance(paymentFormModal).hide();
    if (error.type === "card_error" || error.type === "validation_error") {
        showMessage(error.message, true);
    } else {
        showMessage("Възникна неочаквана грешка.", true);
    }

    setLoading(false);
}

// Fetches the payment intent status after payment submission
async function checkStatus() {
    const clientSecret = new URLSearchParams(window.location.search).get(
        "payment_intent_client_secret"
    );

    if (!clientSecret) {
        return;
    }

    const { paymentIntent } = await stripe.retrievePaymentIntent(clientSecret);

    switch (paymentIntent.status) {
        case "succeeded":
            showMessage("Успешно плащане!");
            break;
        case "processing":
            showMessage("Плащането ви се обработва.");
            break;
        case "requires_payment_method":
            showMessage("Неуспешно плащане. Моля, опитайте отново.", true);
            break;
        default:
            showMessage("Възникна неочаквана грешка.", true);
            break;
    }
}

// ------- UI helpers -------

function showMessage(messageText, error) {

    if (error) {
        let failModal = document.getElementById("fail-modal");
        failModal.querySelector('p').textContent = messageText;
        bootstrap.Modal.getOrCreateInstance(failModal).show();
    } else {
        let successModal = document.getElementById("success-modal");
        successModal.querySelector('p').textContent = messageText;
        bootstrap.Modal.getOrCreateInstance(successModal).show();
    }
}

// Show a spinner on payment submission
function setLoading(isLoading) {
    if (isLoading) {
        // Disable the button and show a spinner
        document.querySelector(".bouncer").classList.remove("d-none");
        document.querySelector("#submit").classList.add("d-none");
    } else {
        document.querySelector("#submit").disabled = false;
        document.querySelector(".bouncer").classList.add("d-none");
        document.querySelector("#submit").classList.remove("d-none");
    }
}