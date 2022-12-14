// TODO: this is stupid. You can do better
const userIsAdmin = document.getElementById('btnWorkingHours') !== null;
const culture = document.cookie.match('Culture')?.input.substr(-2) || 'bg';
// TODO: this is also stupid. Take from cookie
const userIsLoggedIn = document.querySelector('a[href*="Logout"]') !== null;
const cultureIsEn = culture === 'en';
const csfrToken = document.querySelector("#csfrToken input[name=__RequestVerificationToken]").value;
const shortWeekdayNames = { 'Sun': 0, 'Mon': 1, 'Tue': 2, 'Wed': 3, 'Thu': 4, 'Fri': 5, 'Sat': 6 };
const todayShortName = new Date().toString().split(' ')[0];
const themeColor = "#92ab95";
const yellowColor = "#EEB440";
const clockwiseArrowSymbol = '\u27F3';
const plusSymbol = '+';
const checkmarkSymbol = '\u2713';
const workingHoursArray = document.querySelector('#calendar').getAttribute('workingHours').split('-');
const standardWorkingHours = { start: workingHoursArray[0], end: workingHoursArray[1] };
let dayCount = 5;

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

let currentSelectedDate = '';
let availableDailySlots = [];
let currentAppointment = null;
// TODO: what if we have no appointments from server?
const allAppointments = document.querySelector('.dbEvents').textContent
    .split(';').filter(n => n)
    .map(x => {
        return x;
    })
    .map(e => JSON.parse(e));
const availableAppointments = allAppointments?.filter(a => !a.isUnavailable && new Date(a.start) > new Date());


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
                dayHeaderFormat: { weekday: 'long', month: 'numeric', day: 'numeric' },
                type: 'dayGridWeek',
                duration: { days: dayCount },
                dayCount: dayCount
            }
        },
        allDaySlot: false,
        slotLabelInterval: { days: 1 },
        slotMinTime: standardWorkingHours.start + ':00:00',
        slotMaxTime: standardWorkingHours.end + ':00:00',
        height: 'auto',
        locale: culture,
        select: select,
        events: allAppointments,
        eventDisplay: 'block',
        eventBackgroundColor: 'white',
        eventBorderColor: '#92ab95',
        displayEventTime: false,
        eventClick: function (arg) {
            const appt = getCurrentAppt(arg);
            const isOldAvailableAppt = new Date(appt.start) < new Date() && appt.userId == null;

            if (userIsAdmin) {
                showAppointmentDetails();
                return;
            }

            if (isOldAvailableAppt) {
                return;
            }

            if (userIsAdmin) {
                showAppointmentDetails();
                return;
            } else {
                if (appt.isUnavailable) {
                    return;
                }
                if (appt.userId != 0 && appt.userId != null) {
                    // own appt
                    showAppointmentDetails();
                    return;
                }

                if (appt.userId == null && userIsLoggedIn) {
                    // COLLOSAL SHITE
                    // TODO: Next time anything date-related needs changing
                    // date library will need introducing. Under no cinrcumstances
                    // should one continute working this way.

                    const sameDayAppts = allAppointments.filter(x => {
                        const xDate = new Date(x.start.toString().split(' ').slice(0, 4).join(' '))
                            .toString().split(' ').slice(0, 4).join(' ');
                        const currentApptDate = appt.start.toString().split(' ').slice(0, 4).join(' ');

                        return xDate == currentApptDate;
                    });

                    if (sameDayAppts.find(x => x.userId != null)) {
                        alert('Вече имате запазен час за този ден');
                        return;
                    }

                    // Set up onsite
                    if (appt.canBeOnSite) {
                        onSiteBookMsg.style.display = 'none';
                        onSiteBookToggle.style.display = 'block';
                        onsiteBookCheckbox.checked = false;
                    } else {
                        onSiteBookToggle.style.display = 'none';
                        onSiteBookMsg.style.display = 'block';
                        onSiteBookMsg.textContent = 'Онлайн сесия';
                    }

                    bootstrap.Modal.getOrCreateInstance(bookModal).show();
                } else {
                    bootstrap.Modal.getOrCreateInstance(loginModal).show();
                }
            }
        },
        eventClassNames: function (arg) {
            const appt = getCurrentAppt(arg);
            const isOldAppt = new Date(appt.start) < new Date() && appt.userId == null;

            if (isOldAppt) {
                // Greyed out: non-clckable
                return ['gray'];
            }
            if (appt.isApproved && !appt.isUnavailable) {
                return ['green'];
            }
            if (appt.userId !== null && !appt.isApproved) {
                return ['yellow'];
            } else {
                if (appt.isUnavailable) {
                    return ['gray']
                }
            }
        },
    })

    calendar.render();

    if (userIsLoggedIn) {
        const pendingUpcoming = allAppointments.filter(x => x.userId != null &&
            !x.isApproved && new Date(x.start) > new Date());
        if (pendingUpcoming.length > 0) {
            calendar.gotoDate(new Date(pendingUpcoming.sort(x => new Date(x.start))[0].start));
            return;
        }

        if (!userIsAdmin) {
            const approvedUpcoming = allAppointments.filter(x => x.isApproved);
            if (approvedUpcoming.length > 0) {
                calendar.gotoDate(new Date(approvedUpcoming.sort(x => new Date(x.start))[0].start));
                return;
            }
        }
    }

    // Scroll horizontally to first available appointment
    if (availableAppointments.length > 0) {
        calendar.gotoDate(new Date(availableAppointments[0].start));
    }
});
// END OF MAIN FUNCTION

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

// Appointment details modal setup
function showAppointmentDetails() {
    // Get appointment date and hour
    let datelocaleStringArr = new Date(currentAppointment.start)
        .toLocaleString(`${culture}-${culture.toUpperCase()}`).split(', ');
    let date = datelocaleStringArr[0];
    let hour = datelocaleStringArr[1];

    // Populate date & time fields
    detailsModal.querySelector('.date').textContent = date;
    detailsModal.querySelector('.start').textContent = hour;

    // Set up onsite slider
    if (userIsAdmin) {
        if (currentAppointment.userId !== null) {
            onSiteDetailsToggle.style.display = 'none';
            onSiteDetailsMsg.style.display = 'block';

            if (currentAppointment.isOnSite) {
                onSiteDetailsMsg.textContent = 'Сесия на живо';
            } else {
                onSiteDetailsMsg.textContent = 'Онлайн сесия';
            }
        }
        else {
            onSiteDetailsToggle.style.display = 'block';
            onSiteDetailsMsg.style.display = 'none';
            if (currentAppointment.canBeOnSite) {
                onsiteDetailsCheckbox.checked = true;
            } else {
                onsiteDetailsCheckbox.checked = false;
            }
        }
    } else {
        if (currentAppointment.isOnSite) {
            onSiteDetailsMsg.textContent = 'Сесия на живо';
        } else {
            onSiteDetailsMsg.textContent = 'Онлайн сесия';
        }
    }

    if (userIsAdmin && currentAppointment.userName === null) {
        document.querySelector('#occupyAppointment').style.display = 'inline-block';
        // Admin's own unoccupied appointment slot
        hideDetailsInAppointmentDetailsModal();
    } else {
        detailsModal.querySelector('.username').textContent = currentAppointment.userName;
        detailsModal.querySelector('.details').textContent = currentAppointment.description;
        setUpStatusInAppointmentDetailsModal(currentAppointment.isApproved);
        if (userIsAdmin) {
            // Appt occupied by admin. Hide 'Occupy' btn
            document.querySelector('#occupyAppointment').style.display = 'none';

            // Details fields can only have been with hidden state for admin.
            showDetailsInAppointmentDetailsModal();
        }
    }

    bootstrap.Modal.getOrCreateInstance(detailsModal).show();
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

function select(selectInfo) {
    const selectedDate = new Date(new Date(selectInfo.start).toString().split(' ').slice(0, 4).join(' '));
    const currentDate = new Date(new Date().toString().split(' ').slice(0, 4).join(' '));

    if (selectedDate < currentDate || !userIsAdmin) {
        return;
    }

    clearDailyAvailability();
    currentSelectedDate = `${selectInfo.start.getMonth() + 1}-${selectInfo.start.getDate()}-${selectInfo.start.getFullYear()}`;
    let modalElement = document.getElementById('dailyAvailabilityModal');
    bootstrap.Modal.getOrCreateInstance(modalElement).show();
}

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

submitDailyAvailabilityBtn.addEventListener('click', function () {
    // TODO: Check for overlapping slots

    postData(
        '/api/appointments/Create',
        {
            dateString: currentSelectedDate,
            timeSlotsString: availableDailySlots,
        },
        csfrToken)
        .then(data => {
            bootstrap.Modal.getOrCreateInstance(dailyAvailabilityModal).hide();
            window.location.reload();
        });
});

sendAppointmentBtn.addEventListener('click', function () {
    // Validate description
    let issueDescriptionField = bookModal.querySelector('#patientIssueDescription');
    let userIssueDescription = "";

    // We don't have this field if user has already had an appointment
    if (issueDescriptionField) {
        userIssueDescription = issueDescriptionField.value.trim();
        if (userIssueDescription.length < 10) {
            alert('Моля, опишете накратко или въведете телефонен номер');
            return;
        }
    }

    let data = {
        id: currentAppointment.id,
        description: userIssueDescription,
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

workingHoursSubmitBtn.addEventListener('click', function () {
    let startHour = document.getElementById('startHour').value;
    let endHour = document.getElementById('endHour').value;

    postData(
        '/api/appointments/SetWorkingHours',
        { startHour, endHour },
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(workingHoursModal).hide();
            window.location.reload();
        })
        .catch(() => {
            alert(genericError);
        });
});

document.querySelector('#occupyAppointment')?.addEventListener('click', function () {
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
    if (currentAppointment.userId !== null) {
        // Appt is either taken or awaiting approval -> cannot change appt location at this point
        alert('Часът вече е зает.')
        return;
    }

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

// For admin
//                          Available apptmnt -> bookApptmntModal
//                          Scenario 1 - Apptmnt is available
//                              btn toggle only changes state in db
//                          Taken apptmnt -> apptmntDetailsModal
//                          Scenario 2 - Apptmnt is pending approval
//                              btn switches on -> email we can do it in person if client wants
//                              btn switches off -> email to tell client we cannot do it in person
//                          Scenario 3 - Apptmnt is approved
//                              same as scenario 2

// For user
//                          Scenario 1 - Available -> bookAppointment Modal
//                              btn is off but can be switched on(or vice versa) if admin has allowed -> send admin email in both cases
//                              if admin hasn't allowed, btn is greyed out + msg this apptmnt is only online
//                          Scenario 2 - Pending -> Apptmnt details modal
//                              user can still switch btn state -> send email to admin
//                          Scenario 3 - Approved -> Apptmnt details modal
//                              user can still switch state -> will put apptmnt back into 'awaiting approval' mode
