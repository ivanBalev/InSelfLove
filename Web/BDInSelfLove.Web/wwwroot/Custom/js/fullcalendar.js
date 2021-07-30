$(document).ready(function () {
    const token = $("#csfrToken input[name=__RequestVerificationToken]").val();
    const userIsAdmin = document.getElementById('btnWorkingHours') !== null;
    const culture = document.cookie.match('Culture')?.input.substr(-2);
    const cultureIsEn = culture === 'en';

    const themeColor = "#92ab95";
    const yellowColor = "#ffc107";
    const clockwiseArrowSymbol = '\u2b6e';
    const plusSymbol = '+';
    const checkmarkSymbol = '\u2713';

    let currentSelectedDate = '';
    let appointments = [];
    let selectedAppointment = null;
    let availableDailySlots = [];
    let workingHoursArray = document.querySelector('#calendar').getAttribute('workingHours').split('-');
    let standardWorkingHours = { start: workingHoursArray[0], end: workingHoursArray[1] };

    const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
        'July', 'August', 'September', 'October', 'November', 'December'];
    const monthNamesShort = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
        'Jul', 'Aug', 'Sept', 'Oct', 'Nov', 'Dec'];
    const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const dayNamesShort = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

    const monthNamesBG = ['Януари', 'Февруари', 'Март', 'Април', 'Май', 'Юни',
        'Юли', 'Август', 'Септември', 'Октомври', 'Ноември', 'Декември'];
    const monthNamesBGShort = ['Яну', 'Фев', 'Мар', 'Апр', 'Май', 'Юни', 'Юли', 'Авг', 'Сеп', 'Окт', 'Ное', 'Дек'];
    const dayNamesBG = ['Неделя', 'Понеделник', 'Вторник', 'Сряда', 'Четвъртък', 'Петък', 'Събота'];
    const dayNamesBGShort = ['Нед', 'Пон', 'Вт', 'Сря', 'Чет', 'Пет', 'Съб'];

    const approved = cultureIsEn ? 'Approved' : 'Одобрен';
    const awaitingApproval = cultureIsEn ? 'Awaiting approval' : 'Очаква одобрение';
    const genericError = cultureIsEn ? 'Error' : 'Грешка';
    const appointmentDescriptionError = cultureIsEn ? 'Please enter more than 30 characters' :
        'Моля, въведи повече от 30 символа';

    fetchEventAndRenderCalendar();
    function fetchEventAndRenderCalendar() {
        $.ajax({
            type: "GET",
            url: "/api/appointment/GetAll",
            success: function (dbEvents) {
                // Avoid duplicate appointments when doing ajax calls
                appointments = [];

                $.each(dbEvents, function (i, currentEvent) {
                    // Set event start and end in the format requested by fullcalendar
                    currentEvent.start = moment(currentEvent.start);
                    currentEvent.end = moment(currentEvent.start).add(1, 'hours');
                    appointments.push(currentEvent);
                });

                GenerateCalendar(appointments);
            },
            error: function (err) {
                alert(genericError);
            }
        })
    };

    function GenerateCalendar(events) {
        $('#calendar').fullCalendar('destroy');
        $('#calendar').fullCalendar({
            height: 'auto',
            defaultDate: new Date(),
            timeFormat: 'H:mm',
            titleFormat: 'MMM D',
            columnFormat: 'ddd',
            slotLabelFormat: ['H:mm'],
            header: { left: 'prev', center: 'title', right: 'next' },
            eventLimit: true,
            eventColor: 'white',
            minTime: standardWorkingHours.start + ':00:00',
            maxTime: standardWorkingHours.end + ':00:00',
            firstDay: firstDay,
            monthNames: cultureIsEn ? monthNames : monthNamesBG,
            monthNamesShort: cultureIsEn ? monthNamesShort : monthNamesBGShort,
            dayNames: cultureIsEn ? dayNames : dayNamesBG,
            dayNamesShort: cultureIsEn ? dayNamesShort : dayNamesBGShort,
            defaultView: 'agendaWeek',
            eventTextColor: 'black',
            eventBorderColor: themeColor,
            displayEventTime: true,
            events: events,
            eventClick: eventClick,
            selectable: true,
            allDaySlot: false,
            eventMouseover: eventMouseover,
            eventMouseout: eventMouseout,
            eventRender: (eventObj, $el) => eventRender(eventObj, $el),
            selectLongPressDelay: 0,
            select: select,
        })
    };

    const eventClick = (clickedAppointment) => {
        selectedAppointment = clickedAppointment;
        // Appointment available for booking
        if (clickedAppointment.userUserName === null && !userIsAdmin) {
            $('#bookAppointmentModal').modal();
            return;
        }
        setUpAppointmentDetailsModal(clickedAppointment);
    }

    const clearDailyAvailability = () => {
        availableDailySlots = [];
        let slots = document.getElementsByClassName('dailyTimeSlot');
        for (let i = 0; i < slots.length; i++) {
            slots[i].style.backgroundColor = "white";
        }
    }

    $('#submitDailyAvailability').click(function () {
        $.ajax({
            type: 'POST',
            data: {
                date: currentSelectedDate,
                timeSlots: availableDailySlots,
            },
            url: '/api/appointment/Create',
            headers: { 'X-CSRF-TOKEN': token },
            success: function () {
                $('#dailyAvailabilityModal').modal('hide');
                fetchEventAndRenderCalendar();
            },
            error: function () {
                alert(genericError);
                $('#workingHours').modal('hide');
            }
        })
    })

    $('#workingHoursSubmitBtn').click(function () {
        // TODO: VALIDATE

        let startHour = document.querySelector('#startHour').value;
        let endHour = document.querySelector('#endHour').value;

        $.ajax({
            type: "POST",
            url: '/api/appointment/SetWorkingHours',
            data: {
                startHour,
                endHour,
            },
            headers: { 'X-CSRF-TOKEN': token },
            success: function () {
                fetchEventAndRenderCalendar();
                $('#workingHours').modal('hide');
            },
            error: function () {
                alert(genericError);
                $('#workingHours').modal('hide');
            }
        })
    })

    $('#btnDelete').click(function () {
        if (selectedAppointment.start.isBefore(moment())) {
            return;
        }
        $('#appointmentDetailsModal').modal('hide');
        $('#cancelAppointmentConfirm').modal();
    })

    $('#cancelAppointmentConfirm .confirmCancelAppointment').click(function () {
        $.ajax({
            type: "POST",
            url: '/api/appointment/Cancel',
            data: {
                id: selectedAppointment.id
            },
            headers: { 'X-CSRF-TOKEN': token },
            success: function () {
                $('#cancelAppointmentConfirm').modal('hide');
                fetchEventAndRenderCalendar();
            },
            error: function () {
                alert(genericError);
            }
        })
    });

    $('#sendAppointment').click(function () {
        // Validate description
        let userIssueDescription = $('#patientIssueDescription').val().trim();
        if (userIssueDescription == '' || userIssueDescription.length < 30) {
            alert(appointmentDescriptionError)
            return;
        }

        let data = {
            Start: selectedAppointment.start._i,
            Description: $('#patientIssueDescription').val(),
        }

        $.ajax({
            type: "POST",
            url: '/api/appointment/Book',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                $('#bookAppointmentModal').modal('hide');
                $('#patientIssueDescription').val('');
                fetchEventAndRenderCalendar();
            },
            error: function () {
                alert(genericError);
            }
        })
    })

    $('#appointmentDetailsModal #approveAppointment').click(function () {
        if (selectedAppointment.start.isBefore(moment())) {
            return;
        }

        $.ajax({
            type: "POST",
            url: '/api/appointment/Approve',
            data: {
                id: selectedAppointment.id,
            },
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                $('#appointmentApproval').modal('hide');
                fetchEventAndRenderCalendar();
            },
            error: function () {
                alert(genericError);
            }
        })
    });

    $('#btnWorkingHours').click(function () {
        $('#workingHours').modal();
    })

    const select = (start) => {
        // Forbid adding appointments to past dates & adding appointments by guest users
        if (start.isBefore(moment()) || !userIsAdmin) {
            $('#calendar').fullCalendar('unselect');
            return;
        }

        clearDailyAvailability();

        currentSelectedDate = start.format('MM-DD-YYYY');
        $('#dailyAvailabilityModal').modal();
        $('#calendar').fullCalendar('unselect');
    };

    const eventRender = (eventObj, $el) => {
        let eventElement = $el[0];
        const style = 'style';
        const initialStyleState = eventElement.getAttribute(style).split(';')[0];

        if (!eventObj.isApproved) {
            if (eventObj.userUserName === null) {
                // Available appointment
                eventElement.setAttribute(style, `${initialStyleState}; border-color:${themeColor}; color:${themeColor};`);
                eventElement.innerText = plusSymbol;
            } else {
                // Appointment awaiting approval
                eventElement.setAttribute(style, `${initialStyleState}; border-color:${yellowColor}; color:${yellowColor};`);
                eventElement.innerText = clockwiseArrowSymbol;
            }
        } else {
            // Approved appointment
            eventElement.setAttribute(style, `${initialStyleState}; border-color:${themeColor}; color:${themeColor};`);
            eventElement.innerText = checkmarkSymbol;
        }

        eventElement.style.paddingTop = '12px';
        eventElement.style.fontSize = '16px';
        eventElement.setAttribute(style, `${eventElement.getAttribute(style)} text-align: center;`);
        $el[0] = eventElement;
    };

    function eventMouseover() {
        this.style.backgroundColor = this.style.color;
        this.style.color = 'black';
    };

    function eventMouseout() {
        this.style.color = this.style.backgroundColor;
        this.style.backgroundColor = 'white';
    };

    const firstDay = () => {
        let today = moment()._d.split(' ')[0];
        return daysOfWeek.find(d => d.startsWith(today));
    };

    if (userIsAdmin) {
        // Dynamically add/remove slots to availableDaylySlots collection when admin clicks on the form slots
        (function setUpDailyWorkingHours() {
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
    }

    // Appointment details modal setup
    const setUpAppointmentDetailsModal = (appointment) => {
        $('#appointmentDetailsModal .date')[0].innerText = appointment.start.format("DD/MM/YYYY");
        $('#appointmentDetailsModal .start')[0].innerText = appointment.start.format("HH:mm");
        $('#appointmentDetailsModal .end')[0].innerText = appointment.end.format("HH:mm");

        if (userIsAdmin && appointment.userUserName === null) {
            // Admin's own unoccupied appointment slot
            hideDetailsInAppointmentDetailsModal();
        } else {
            $('#appointmentDetailsModal .username')[0].innerText = appointment.userUserName;
            $('#appointmentDetailsModal .details')[0].innerText = appointment.description;
            setUpStatusInAppointmentDetailsModal(appointment.isApproved);
            if (userIsAdmin) {
                // Details fields can only have been with hidden state for admin.
                showDetailsInAppointmentDetailsModal();
            }
        }

        $('#appointmentDetailsModal').modal();
    }

    const hideDetailsInAppointmentDetailsModal = () => {
        $('#appointmentDetailsModal .usernameGroup').hide();
        $('#appointmentDetailsModal .detailsGroup').hide();
        $('#appointmentDetailsModal .statusGroup').hide();
        $('#appointmentDetailsModal #approveAppointment').hide();
    }

    const showDetailsInAppointmentDetailsModal = () => {
        $('#appointmentDetailsModal .usernameGroup').show();
        $('#appointmentDetailsModal .detailsGroup').show();
        $('#appointmentDetailsModal .statusGroup').show();
        // Hide approve button if appointment is already approved
        if (selectedAppointment.isApproved) {
            $('#appointmentDetailsModal #approveAppointment').hide();
        } else {
            $('#appointmentDetailsModal #approveAppointment').show();
        }
    }

    const setUpStatusInAppointmentDetailsModal = (isApproved) => {
        if (isApproved) {
            $('#appointmentDetailsModal .status')[0].innerText = approved;
            $('#appointmentDetailsModal .status').css('color', themeColor);
        } else {
            $('#appointmentDetailsModal .status')[0].innerText = awaitingApproval;
            $('#appointmentDetailsModal .status').css('color', yellowColor);
        }
    }
})


