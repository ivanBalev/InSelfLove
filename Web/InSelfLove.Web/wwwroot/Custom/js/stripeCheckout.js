// This is your test publishable API key.
// TODO: locale per user
const stripe = Stripe("pk_test_51Kv04DJ7U5sVQK1wFN7NWaALcSfBmrEymkcZHxnwxTssePIOB3rieSajOh9wiH6jGWswoWJWPe2yj05RWJaZDMqZ002GPAzcw4", { locale: 'bg' });

let elements;

checkStatus();

document
    .querySelector("#payment-form")
    .addEventListener("submit", handleSubmit);

// TODO: introduce email validation for user on registration - enter email address automatically.
// Only users who've confirmed their email should be able to make an online payment
let emailAddress = '';
// Fetches a payment intent and captures the client secret
async function initialize(appointmentId) {
    setLoading(true);

    const response = await fetch("/api/appointments/CreatePaymentIntent", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: appointmentId,
    });
    const { clientSecret } = await response.json();

    const appearance = {
        theme: 'stripe',
    };
    elements = stripe.elements({ appearance, clientSecret });

    const linkAuthenticationElement = elements.create("linkAuthentication");
    linkAuthenticationElement.mount("#link-authentication-element");

    linkAuthenticationElement.on('change', (event) => {
        emailAddress = event.value.email;
    });

    const paymentElementOptions = {
        layout: "tabs",
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
            // Make sure to change this to your payment completion page
            return_url: "https://localhost:44319/api/appointments",
            receipt_email: emailAddress,
        },
    });

    // This point will only be reached if there is an immediate error when
    // confirming the payment. Otherwise, your customer will be redirected to
    // your `return_url`. For some payment methods like iDEAL, your customer will
    // be redirected to an intermediate site first to authorize the payment, then
    // redirected to the `return_url`.
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