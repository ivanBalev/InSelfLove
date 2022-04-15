const userIsAdmin = document.getElementById('btnWorkingHours') !== null;
const culture = document.cookie.match('Culture')?.input.substr(-2) || 'bg';
const userIsLoggedIn = document.querySelector('a[href*="Logout"]') !== null;
const cultureIsEn = culture === 'en';
const csfrToken = document.querySelector("#csfrToken input[name=__RequestVerificationToken]").value;
const shortWeekdayNames = { 'Sun': 0, 'Mon': 1, 'Tue': 2, 'Wed': 3, 'Thu': 4, 'Fri': 5, 'Sat': 6 };
const todayShortName = new Date().toString().split(' ')[0];
const themeColor = "#92ab95";
const yellowColor = "#ffc107";
const clockwiseArrowSymbol = '\u27F3';
const plusSymbol = '+';
const checkmarkSymbol = '\u2713';
const workingHoursArray = document.querySelector('#calendar').getAttribute('workingHours').split('-');
const standardWorkingHours = { start: workingHoursArray[0], end: workingHoursArray[1] };

// Alert messages
const approved = cultureIsEn ? 'Approved' : 'Одобрен';
const awaitingApproval = cultureIsEn ? 'Awaiting approval' : 'Очаква одобрение';
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
let selectedAppointment = null;

document.addEventListener('DOMContentLoaded', function () {

    let appointments = GetAppointments();
    var calendarEl = document.getElementById('calendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        selectable: true,
        initialView: 'timeGridWeek',
        firstDay: shortWeekdayNames[todayShortName],
        headerToolbar: {
            start: 'prev', center: 'title', end: 'next'
        },
        titleFormat: {
            year: '2-digit', month: 'short', day: 'numeric'
        },
        views: {
            timeGridWeek: {
                dayHeaderFormat: { weekday: 'short' }
            }
        },
        allDaySlot: false,
        slotMinTime: standardWorkingHours.start + ':00:00',
        slotMaxTime: standardWorkingHours.end + ':00:00',
        height: 'auto',
        locale: culture,
        select: select,
        longPressDelay: 1,
        events: appointments,
        eventClick: function (info) {
            selectedAppointment = info.event.extendedProps;
            // Appointment available for booking
            if (info.event.extendedProps.UserUserName === null && !userIsAdmin) {
                if (!userIsLoggedIn) {
                    bootstrap.Modal.getOrCreateInstance(loginModal).show();
                    return;
                }

                bootstrap.Modal.getOrCreateInstance(bookModal).show();
                return;
            }
            setUpAppointmentDetailsModal();
        },
        eventClassNames: function (arg) {
            if (!arg.event.extendedProps.IsApproved && arg.event.extendedProps.UserUserName) {
                // Awaiting approval
                return ['yellow'];
            }
            // Approved or available
            return ['green'];
        },
        eventContent: function (arg) {
            if (!arg.event.extendedProps.IsApproved) {
                // Available : Awaiting approval
                return arg.event.extendedProps.UserUserName === null ? plusSymbol : clockwiseArrowSymbol;
            } else {
                // Approved
                return checkmarkSymbol;
            }
        },
    })

    calendar.render();
});

// Appointment details modal setup
function setUpAppointmentDetailsModal() {
    let datelocaleStringArr = new Date(selectedAppointment.Start)
        .toLocaleString(`${culture}-${culture.toUpperCase()}`).split(', ');
    let day = datelocaleStringArr[0];
    let startHour = datelocaleStringArr[1];
    let startHourArr = startHour.split(':');
    let endHour = (parseInt(startHourArr[0]) + 1) + ':' + startHourArr.slice(1).join(':');

    detailsModal.querySelector('.date').textContent = day;
    detailsModal.querySelector('.start').textContent = startHour;
    detailsModal.querySelector('.end').textContent = endHour;

    if (userIsAdmin && selectedAppointment.UserUserName === null) {
        // Admin's own unoccupied appointment slot
        hideDetailsInAppointmentDetailsModal();
    } else {
        detailsModal.querySelector('.username').textContent = selectedAppointment.UserUserName;
        detailsModal.querySelector('.details').textContent = selectedAppointment.Description;
        setUpStatusInAppointmentDetailsModal(selectedAppointment.IsApproved);
        if (userIsAdmin) {
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
    if (selectedAppointment.isApproved) {
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
    if (selectInfo.start.getTime() < new Date().getTime() || !userIsAdmin) {
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

function GetAppointments() {
    let dbEvents = document.querySelector('.dbEvents').textContent.split(';').filter(n => n).map(e => JSON.parse(e));
    let appointments = [];
    let availableAppointmentsPresent = false;

    for (const currentEvent of dbEvents) {
        if (!availableAppointmentsPresent && currentEvent.UserUserName === null) {
            availableAppointmentsPresent = true;
        }

        // Set event start and end in the format requested by fullcalendar
        // TODO: below is utter shite but works
        let startDate = new Date(currentEvent.Start);
        currentEvent.start = startDate;
        //currentEvent.end = new Date(new Date(currentEvent.start).setHours(startDate.getHours() + 1));

        appointments.push(currentEvent);
    }

    if (!availableAppointmentsPresent && !userIsAdmin) {
        // Show 'no available appointments' alert
        document.querySelector('.alert').style.display = 'block';
    }

    return appointments;
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

submitDailyAvailabilityBtn.addEventListener('click', function () {
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
    let userIssueDescription = bookModal.querySelector('#patientIssueDescription').value.trim();
    if (userIssueDescription == '' || userIssueDescription.length < 30) {
        alert(appointmentDescriptionError)
        return;
    }

    let data = {
        Id: selectedAppointment.Id,
        Description: userIssueDescription,
    }

    postData(
        '/api/appointments/Book',
        data,
        csfrToken)
        .then(() => {
            bootstrap.Modal.getOrCreateInstance(bookModal).hide();
            patientIssueDescription.value = '';
            window.location.reload();
        });
});

approveBtn?.addEventListener('click', function () {
    // No point in approving past appointment
    if (new Date(selectedAppointment.Start) < new Date()) {
        return;
    }

    postData(
        '/api/appointments/Approve',
        { id: selectedAppointment.Id },
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
    if (new Date(selectedAppointment.Start) < new Date()) {
        return;
    }

    bootstrap.Modal.getOrCreateInstance(detailsModal).hide();
    bootstrap.Modal.getOrCreateInstance(cancelConfirmModal).show();
});

confirmCancelBtn.addEventListener('click', function () {
    postData(
        '/api/appointments/Cancel',
        { id: selectedAppointment.Id },
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
