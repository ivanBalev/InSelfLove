const userIsAdmin = document.getElementById('btnWorkingHours') !== null;
const culture = document.cookie.match('Culture')?.input.substr(-2) || 'bg';
const userIsLoggedIn = document.querySelector('a[href*="Logout"]') !== null;
const cultureIsEn = culture === 'en';
const csfrToken = document.querySelector("#csfrToken input[name=__RequestVerificationToken]").value;
const shortWeekdayNames = { 'Sun': 0, 'Mon': 1, 'Tue': 2, 'Wed': 3, 'Thu': 4, 'Fri': 5, 'Sat': 6 };
const todayShortName = new Date().toString().split(' ')[0];
const workDayStartStr = "workDayStart";
const workDayEndStr = "workDayEnd";                                                 // Remove nulls, 0, ""
const allAppointments = document.querySelector('.dbEvents').textContent.split(';').filter(n => n).map(x => x).map(e => JSON.parse(e));
const availableAppointments = allAppointments?.filter(a => !a.isUnavailable && new Date(a.start) > new Date());

// Style
const themeColor = "#92ab95";
const yellowColor = "#EEB440";
const clockwiseArrowSymbol = '\u27F3';
const plusSymbol = '+';
const checkmarkSymbol = '\u2713';

// How many days are displayed on screen
let dayCount = 5;
// Reduce for smaller screens
if (
    navigator.userAgent.match(/Android/i) ||
    navigator.userAgent.match(/iPhone/i)
) {
    dayCount = 3;
}

// Alert messages
const approved = cultureIsEn ? 'Approved' : 'Одобрен';
const awaitingApproval = cultureIsEn ? 'Awaiting approval' : 'Очаква потвърждение';
const genericError = cultureIsEn ? 'Error' : 'Грешка';
const appointmentDescriptionError = cultureIsEn ? 'Please enter more than 30 characters' :
    'Моля, въведи повече от 30 символа';

// Buttons
const submitDailyAvailabilityBtn = document.getElementById('submitDailyAvailability');
const sendAppointmentBtn = document.getElementById('sendAppointment');
const approveBtn = document.getElementById('approveAppointment');
const cancelBtn = document.getElementById('btnDelete');
const confirmCancelBtn = document.getElementsByClassName('confirmCancelAppointment')[0];
const workingHoursSubmitBtn = document.getElementById('workingHoursSubmitBtn');
const occupyBtn = document.querySelector('#occupyAppointment');

// Onsite-related btn/msg
const onSiteDetailsMsg = document.querySelector('#onSiteDetailsMsg');
const onSiteDetailsToggle = document.querySelector('#onSiteDetailsToggle');
const onsiteDetailsCheckbox = onSiteDetailsToggle?.querySelector('.toggle-checkbox');
const onSiteBookMsg = document.querySelector('#onSiteBookMsg');
const onSiteBookToggle = document.querySelector('#onSiteBookToggle');
const onsiteBookCheckbox = onSiteBookToggle?.querySelector('.toggle-checkbox');

// Modals
const bookModal = document.getElementById('bookAppointmentModal');
const detailsModal = document.getElementById('appointmentDetailsModal');
const cancelConfirmModal = document.getElementById('cancelAppointmentConfirm');
const dailyAvailabilityModal = document.getElementById('dailyAvailabilityModal');
const workingHoursModal = document.getElementById('workingHours');
const loginModal = document.getElementById('loginModal');

// Fields 
const patientIssueDescription = document.getElementById('patientIssueDescription');

// Global variables
let currentSelectedDate = '';
let availableDailySlots = [];
let currentAppointment = null;

// MAIN FUNCTION
document.addEventListener('DOMContentLoaded', function () {

    var calendar = new FullCalendar.Calendar(document.getElementById('calendar'), {
        selectable: true,
        initialView: 'custom',
        firstDay: 1, //Monday
        headerToolbar: {
            start: 'prev', end: 'next'
        },
        titleFormat: {
            year: '2-digit', month: 'short', day: 'numeric'
        },
        views: {
            custom: {
                dayHeaderFormat: { weekday: 'long', month: 'numeric', day: 'numeric', omitCommas: true },
                type: 'dayGridWeek',
                duration: { days: dayCount },
                dayCount: dayCount
            }
        },
        allDaySlot: false,
        // Fix for select event firing only after 1-second touch hold (value default is 1000 ms)
        selectLongPressDelay: 1,
        slotLabelInterval: { days: 1 },
        height: 'auto',
        locale: culture,
        select: select,
        events: allAppointments,
        eventDisplay: 'block',
        eventBackgroundColor: 'white',
        eventBorderColor: '#92ab95',
        displayEventTime: false,
        eventClick: eventClick,
        eventClassNames: eventClassNames,
    })

    calendar.render();
    scrollToCorrectDay(calendar);
});
// END OF MAIN FUNCTION

function scrollToCorrectDay(calendar) {
    if (userIsLoggedIn) {
        // Scroll to first appointment that's awaiting approval
        const pendingUpcoming = allAppointments.filter(x => x.userId != null &&
            !x.isApproved && new Date(x.start) >= new Date());
        if (pendingUpcoming.length > 0) {
            calendar.gotoDate(new Date(pendingUpcoming.sort(x => new Date(x.start))[0].start));
            return;
        }

        if (!userIsAdmin) {
            // Scroll to user's first approved appointment
            const approvedUpcoming = allAppointments.filter(x => !x.isUnavailable && x.isApproved && new Date(x.start) >= new Date());
            if (approvedUpcoming.length > 0) {
                calendar.gotoDate(new Date(approvedUpcoming.sort(x => new Date(x.start))[0].start));
                return;
            }
        }
    }

    // Scroll to first available appointment
    if (availableAppointments.length > 0) {
        calendar.gotoDate(new Date(availableAppointments[0].start));
    }

}

function eventClassNames(arg) {
    // Set up appointments' style
    const appt = getCurrentAppt(arg);

    if (appt.isUnavailable) {
        return ['gray']
    }

    if (appt.isApproved) {
        return ['green'];
    }

    // Appointment is awaiting approval (is not approved but has userId attributed)
    if (appt.userId !== null) {
        return ['yellow'];
    }
}

// When user clicks on an appointment slot
function eventClick(arg) {
    // Get appointment
    const appt = getCurrentAppt(arg);

    if (appt.isUnavailable) {
        return;
    }

    if (!userIsLoggedIn) {
        // Pull up login modal
        bootstrap.Modal.getOrCreateInstance(loginModal).show();
        return;
    }

    // User is admin or this is user's own appointment
    if (userIsAdmin || appt.userId != null) {
        // Show appointment details
        showAppointmentDetails();
        return;
    }

    // Appointment is available
    if (appt.userId == null && userIsLoggedIn) {
        showBookingModal(appt);
    }
}

function getCurrentAppt(arg) {
    // Not sure why fullcalendar breaks viewModels up
    const appt = {
        ...arg.event.extendedProps,
        id: arg.event.id,
        start: arg.event.start,
    };
    currentAppointment = appt;
    return appt;
}

// Booking modal is only for users, not admin
function showBookingModal(appt) {
    // Set up onsite functionality
    if (appt.canBeOnSite) {
        // Hide message
        onSiteBookMsg.style.display = 'none';
        // Show slider
        onSiteBookToggle.style.display = 'block';
        // Set slider value
        onsiteBookCheckbox.checked = false;
    } else {
        // Hide slider
        onSiteBookToggle.style.display = 'none';
        // Show message
        onSiteBookMsg.style.display = 'block';
        // Set message content
        onSiteBookMsg.textContent = 'Онлайн сесия';
    }

    bootstrap.Modal.getOrCreateInstance(bookModal).show();
}

// Appointment details modal setup
function showAppointmentDetails() {
    setUpDateAndTime();
    setUpOnsiteSlider();

    if (userIsAdmin) {
        setUpDetailsForAdmin();
    } else {
        setUpDetailsForUser();
    }

    bootstrap.Modal.getOrCreateInstance(detailsModal).show();
}

function setUpDetailsForAdmin() {
    // Appointment is occupied
    if (currentAppointment.userId !== null) {
        occupyBtn.style.display = 'none';
        // Show user info
        showDetailsInAppointmentDetailsModal();
        setUpDetailsForUser();
    }

    // Appointment is available
    if (currentAppointment.userId === null) {
        occupyBtn.style.display = 'inline-block';
        // Hide user info
        hideDetailsInAppointmentDetailsModal();
    }
}

function setUpDetailsForUser() {
    // Populate username, details and status message
    detailsModal.querySelector('.username').textContent = currentAppointment.userName;
    detailsModal.querySelector('.details').textContent = currentAppointment.description;
    setUpStatusInAppointmentDetailsModal(currentAppointment.isApproved);
}

function setUpDateAndTime() {
    // Get appointment date and hour
    let datelocaleStringArr = new Date(currentAppointment.start)
        .toLocaleString(`${culture}-${culture.toUpperCase()}`).split(', ');
    let date = datelocaleStringArr[0];
    let hour = datelocaleStringArr[1];

    // Populate date & time fields
    detailsModal.querySelector('.date').textContent = date;
    detailsModal.querySelector('.start').textContent = hour;
}

function setUpOnsiteSlider() {
    if (userIsAdmin) {
        // Appointment is approved/awaits approval
        if (currentAppointment.userId !== null) {
            // Admin is not allowed to change onsite property value
            onSiteDetailsToggle.style.display = 'none';
            // Admin only sees whether apptmnt is onsite or online
            onSiteDetailsMsg.style.display = 'block';
            // Set the appropriate message depending on isonsite property
            onSiteDetailsMsg.textContent = currentAppointment.isOnSite ? 'Сесия на живо' : 'Онлайн сесия';
        }

        // Appointment is available
        if (currentAppointment.userId === null) {
            // Admin can still change apptmnt onsite property value
            onSiteDetailsToggle.style.display = 'block';
            // Set onsite slider button state
            onsiteDetailsCheckbox.checked = currentAppointment.canBeOnSite ? true : false;
            // Hide onsite value message 
            onSiteDetailsMsg.style.display = 'none';
        }
    } else {
        // User is not admin
        // This is user's own appointment 
        // Its isonsite property value can no longer be changed
        if (currentAppointment.isOnSite) {
            onSiteDetailsMsg.textContent = 'Сесия на живо';
        } else {
            onSiteDetailsMsg.textContent = 'Онлайн сесия';
        }
    }
}

function hideDetailsInAppointmentDetailsModal() {
    detailsModal.querySelector('.usernameGroup').style.display = 'none';
    detailsModal.querySelector('.detailsGroup').style.display = 'none';
    detailsModal.querySelector('.statusGroup').style.display = 'none';
    detailsModal.querySelector('#approveAppointment').style.display = 'none';
}

function showDetailsInAppointmentDetailsModal() {
    detailsModal.querySelector('.usernameGroup').style.display = 'block';
    detailsModal.querySelector('.detailsGroup').style.display = 'block';
    detailsModal.querySelector('.statusGroup').style.display = 'block';
    // Hide approve button if appointment is already approved
    if (currentAppointment.isApproved) {
        detailsModal.querySelector('#approveAppointment').style.display = 'none';
    } else {
        detailsModal.querySelector('#approveAppointment').style.display = 'block';
    }
}

function setUpStatusInAppointmentDetailsModal(isApproved) {
    if (isApproved) {
        detailsModal.querySelector('.status').textContent = approved;
        detailsModal.querySelector('.status').style.color = themeColor;
    } else {
        detailsModal.querySelector('.status').textContent = awaitingApproval;
        detailsModal.querySelector('.status').style.color = yellowColor;
    }
}

function clearDailyAvailability() {
    availableDailySlots = [];
    let slots = document.getElementsByClassName('dailyTimeSlot');
    for (let i = 0; i < slots.length; i++) {
        slots[i].style.backgroundColor = "white";
    }
};

// Sets up daily availability
function select(selectInfo) {
    const selectedDate = new Date(new Date(selectInfo.start).toString().split(' ').slice(0, 4).join(' '));
    const currentDate = new Date(new Date().toString().split(' ').slice(0, 4).join(' '));

    // Past dates cannot be clicked on and dates can only be clicked on by admin
    if (selectedDate < currentDate || !userIsAdmin) {
        return;
    }

    currentSelectedDate = `${selectInfo.start.getMonth() + 1}-${selectInfo.start.getDate()}-${selectInfo.start.getFullYear()}`;
    clearDailyAvailability();
    bootstrap.Modal.getOrCreateInstance(dailyAvailabilityModal).show();
}

// Adds/removes appointment slots from availableDailySlots collection before admin hits Submit
// Applies to dailyAvailabilityModal
(function setUpDailyWorkingHours() {
    if (!userIsAdmin) {
        return;
    }

    let slots = document.getElementsByClassName('dailyTimeSlot');

    for (let i = 0; i < slots.length; i++) {
        let slot = slots[i];

        slot.addEventListener('click', function (e) {
            let slotTime = slot.innerHTML.trim();

            // Add or remove from dailySlots collection
            if (availableDailySlots.includes(slotTime)) {
                availableDailySlots = availableDailySlots.filter(s => s !== slotTime);
                e.target.style.backgroundColor = "white";
            } else {
                availableDailySlots.push(slotTime);
                e.target.style.backgroundColor = themeColor;
            }
        })
    }
})();

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

submitDailyAvailabilityBtn?.addEventListener('click', function () {
    // Overwrite working hours cookie
    const workDayStartCookieValue = decodeURIComponent(getCookie(workDayStartStr));
    const workDayEndCookieValue = decodeURIComponent(getCookie(workDayEndStr));

    // If cookie has expired, set default values
    if (!workDayStartCookieValue || workDayStartCookieValue == 'undefined') {
        // setCookie function from timezone.js
        setCookie(workDayStartStr, '9:00', 400);
        setCookie(workDayEndStr, '18:00', 400);
    } else {
        // Overwrite with existing data
        setCookie(workDayStartStr, workDayStartCookieValue, 400);
        setCookie(workDayEndStr, workDayEndCookieValue, 400);
    }
    
    postData(
        '/api/appointments/Create',
        {
            dateString: currentSelectedDate,
            timeSlotsString: availableDailySlots,
        },
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(dailyAvailabilityModal).hide();
            window.location.reload();
        });
});

sendAppointmentBtn.addEventListener('click', function () {
    // We don't have this field if user has already had an appointment
    let patientIssueDescription = bookModal.querySelector('#patientIssueDescription')?.value.trim();

    // Validate description
    if (patientIssueDescription?.length < 10) {
        alert('Моля, опишете накратко или въведете телефонен номер');
        return;
    }

    // Set up input model
    let data = {
        id: currentAppointment.id,
        description: patientIssueDescription,
        isOnSite: onsiteBookCheckbox.checked,
    }

    postData(
        '/api/appointments/Book',
        data,
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(bookModal).hide();
            window.location.reload();
        });
});

approveBtn?.addEventListener('click', function () {
    // No point in approving past appointment
    if (new Date(currentAppointment.start) < new Date()) {
        return;
    }

    postData(
        '/api/appointments/Approve',
        { id: currentAppointment.id },
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(detailsModal).hide();
            window.location.reload();
        })
        .catch(() => {
            alert(genericError);
        });
});

cancelBtn.addEventListener('click', function () {
    // No point in approving past appointment
    if (new Date(currentAppointment.start) < new Date()) {
        return;
    }

    bootstrap.Modal.getOrCreateInstance(detailsModal).hide();
    bootstrap.Modal.getOrCreateInstance(cancelConfirmModal).show();
});

confirmCancelBtn.addEventListener('click', function () {
    postData(
        '/api/appointments/Cancel',
        { id: currentAppointment.id },
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(cancelConfirmModal).hide();
            window.location.reload();
        })
        .catch(() => {
            alert(genericError);
        });
});

workingHoursSubmitBtn?.addEventListener('click', function () {
    let startHour = document.getElementById('startHour').value;
    let endHour = document.getElementById('endHour').value;

    // setCookie functions from timezone.js file
    // Change values in cookies
    setCookie(workDayStartStr, startHour, 400);
    setCookie(workDayEndStr, endHour, 400);

    bootstrap.Modal.getOrCreateInstance(workingHoursModal).hide();
    window.location.reload();
});

occupyBtn?.addEventListener('click', function () {
    postData(
        '/api/appointments/Occupy',
        { id: currentAppointment.id },
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(detailsModal).hide();
            window.location.reload();
        })
        .catch(() => {
            alert(genericError);
        });
})

// Accessible & visible only to admin (SSR cshtml)
onsiteDetailsCheckbox?.addEventListener('change', function () {
    postData(
        '/api/appointments/SetOnSite',
        { id: currentAppointment.id, canBeOnSite: onsiteDetailsCheckbox.checked },
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(detailsModal).hide();
            window.location.reload();
        })
        .catch(() => {
            alert(genericError);
        });
})