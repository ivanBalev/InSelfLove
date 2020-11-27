$(document).ready(function () {
    const token = $("#csfrToken input[name=__RequestVerificationToken]").val();
    const themeColor = "rgb(170, 85, 132)";

    var appointments = [];
    var selectedAppointment = null;
    var availableDailySlots = [];
    var userIsAdmin = document.getElementById('btnWorkingHours') !== null;
    var daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    const setUpDailyWorkingHours = () => {
        let slots = document.getElementsByClassName('dailyTimeSlot');

        for (let i = 0; i < slots.length; i++) {
            let slot = slots[i];

            slot.addEventListener('click', function (e) {
                let slotTime = slot.innerHTML.trim();

                // Add leading zero to single digit hours
                if (slotTime.split(':')[0].length === 1) {
                    slotTime = '0' + slotTime;
                }

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
    };

    const fetchEventAndRenderCalendar = () => {
        appointments = [];
        var endPoint = userIsAdmin ? 'AdminAppointments' : 'AvailableAppointments';

        $.ajax({
            type: "GET",
            url: "/api/appointment/" + endPoint,
            success: function (dbEvents) {
                $.each(dbEvents, function (i, currentEvent) {
                    let currentAppointment = {
                        id: currentEvent.id,
                        start: moment(currentEvent.start),
                        end: moment(currentEvent.start).add(1, 'hours'),
                        userUserName: currentEvent.userUserName,
                        userEmail: currentEvent.userEmail,
                        isOwn: currentEvent.isOwn,
                        isApproved: currentEvent.isApproved,
                        description: currentEvent.description,
                        userPhoneNumber: currentEvent.userPhoneNumber,
                    }

                    // add additional info if appointment isn't approved for admin approval modal.
                    if (!currentEvent.isApproved) {
                    }

                    appointments.push(currentAppointment);
                });
                GenerateCalendar(appointments);
            },
            error: function (request, textStatus, error) {
                if (request.getResponseHeader('location') != undefined) {
                    window.location.href = request.getResponseHeader('location');
                } else {
                    alert('Error');
                }
            }
        })
    };

    const GenerateCalendar = (events) => {
        $('#calendar').fullCalendar('destroy');
        $('#calendar').fullCalendar({
            contentHeight: 700,
            defaultDate: new Date(),
            timeFormat: 'H:mm',
            columnFormat: 'ddd',
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'agendaWeek',
            },
            eventLimit: true,
            eventColor: 'white',
            minTime: '7:00:00',
            maxTime: '20:00:00',
            firstDay: () => {
                let today = moment()._d.split(' ')[0];
                return daysOfWeek.find(d => d.startsWith(today));
            },
            defaultView: 'agendaWeek',
            eventTextColor: 'black',
            eventBorderColor: themeColor,
            displayEventTime: true,
            events: events,
            eventClick: eventClick,
            selectable: true,
            allDaySlot: false,
            eventMouseover: function (event, jsEvent, view) {
                this.style.backgroundColor = this.style.color;
                this.style.color = 'black';
            },
            eventMouseout: function (event, jsEvent, view) {
                this.style.color = this.style.backgroundColor;
                this.style.backgroundColor = 'white';
            },
            eventRender: (eventObj, $el) => {
                let eventElement = $el[0];
                const style = 'style';
                const initialStyleState = eventElement.getAttribute(style).split(';')[0];

                if (!eventObj.isApproved) {
                    eventElement.setAttribute(style, initialStyleState + '; border-color: #ffc107; color: #ffc107;');
                    eventElement.innerText = userIsAdmin ? eventElement.innerText.split(' - ')[0] : 'My Appointment';

                } else if (!eventObj.isOwn) {
                    eventElement.setAttribute(style, initialStyleState + '; border-color:#28a745; color:#28a745;');
                    eventElement.innerText = eventElement.innerText.split(' - ')[0];

                } else if (eventObj.isOwn) {
                    eventElement.setAttribute(style, initialStyleState + '; border-color:' + themeColor + '; color:' + themeColor + ';');
                    eventElement.innerText = userIsAdmin ? 'Unavailable' : eventElement.innerText.split(' - ')[0];
                }
                eventElement.setAttribute('style', eventElement.getAttribute('style') + ' text-align: center;');
                $el[0] = eventElement;
            },
            select: (start) => {
                // Disregard past slots
                if (start.isBefore(moment()) || !userIsAdmin) {
                    $('#calendar').fullCalendar('unselect');
                    return;
                }
                // Clear selected appointment
                selectedAppointment = {
                    id: 0,
                    description: '',
                    start: '',
                };
                clearDailyAvailability();

                openDailyHoursForm(start._d);
                $('#calendar').fullCalendar('unselect');
            },
            selectLongPressDelay: 0,
        })
    }

    fetchEventAndRenderCalendar();

    userIsAdmin ? setUpDailyWorkingHours() : null;

    const eventClick = (calEvent) => {
        selectedAppointment = calEvent;
        if (calEvent.userUserName === null) {
            // available appointment
            $('#bookAppointmentModal').modal();
            return;
        }

        // Remove cancellation functionality for past events
        if (calEvent.start.isBefore(moment())) {
            $('#appointmentApproval .modal-footer .approveAppointment').hide();
            $('#appointmentApproval .modal-footer .declineAppointment').hide();
            $('#ownAppointmentModal #btnDelete').hide();
        } else {
            $('#appointmentApproval .modal-footer .approveAppointment').show();
            $('#appointmentApproval .modal-footer .declineAppointment').show();
            $('#ownAppointmentModal #btnDelete').show();
        }

        if (userIsAdmin && !calEvent.isApproved) {
            // appointment awaiting approval
            $('#appointmentApproval .username')[0].innerText = calEvent.userUserName;
            $('#appointmentApproval .description')[0].innerText = calEvent.description;
            $('#appointmentApproval .phone')[0].innerText = calEvent.userPhoneNumber;
            $('#appointmentApproval .modal-title')[0].innerText = 'Appointment Evaluation';
            $('#appointmentApproval').modal();
            return;
        }

        // own appointment
        $('#ownAppointmentModal .date')[0].innerText = calEvent.start.format("DD/MM/YYYY");
        $('#ownAppointmentModal .start')[0].innerText = calEvent.start.format("HH:mm");
        $('#ownAppointmentModal .end')[0].innerText = calEvent.end.format("HH:mm");
        $('#ownAppointmentModal .details')[0].innerText = calEvent.description ? calEvent.description : '';
        $('#ownAppointmentModal .username')[0].innerText = calEvent.userUserName;

        if (!calEvent.isApproved) {
            $('#ownAppointmentModal .status')[0].innerText = 'Awaiting approval';
            $('#ownAppointmentModal .status').css('color', '#ffc107');
        } else if (calEvent.isApproved) {
            $('#ownAppointmentModal .status')[0].innerText = 'Approved';
            $('#ownAppointmentModal .status').css('color', '#28a745');
        }

        if (calEvent.isOwn && userIsAdmin) {
            $('#ownAppointmentModal #btnDelete').show();
            $('#ownAppointmentModal .statusGroup').hide();
            $('#ownAppointmentModal .detailsGroup').hide();
            $('#ownAppointmentModal .usernameGroup').hide();
        } else if (!calEvent.isOwn && userIsAdmin && calEvent.start.isBefore(moment())) {
            $('#ownAppointmentModal #btnDelete').hide();
            $('#ownAppointmentModal .statusGroup').show();
            $('#ownAppointmentModal .detailsGroup').show();
            $('#ownAppointmentModal .usernameGroup').show();
        } else if (!calEvent.isOwn && userIsAdmin) {
            $('#ownAppointmentModal #btnDelete').show();
            $('#ownAppointmentModal .statusGroup').show();
            $('#ownAppointmentModal .detailsGroup').show();
            $('#ownAppointmentModal .usernameGroup').show();
        }

        $('#ownAppointmentModal').modal();
    }

    const openDailyHoursForm = (date) => {
        document.getElementById('dailyAvailabilityModal').setAttribute('date', date);
        $('#dailyAvailabilityModal').modal();
    }

    const clearDailyAvailability = () => {
        availableDailySlots = [];
        let slots = document.getElementsByClassName('dailyTimeSlot');
        for (let i = 0; i < slots.length; i++) {
            slots[i].style.backgroundColor = "white";
        }
    }

    const SaveEvent = async function (data) {
        $.ajax({
            type: "POST",
            url: '/api/appointment/Save',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                fetchEventAndRenderCalendar();
                $('#dailyAvailabilityModal').modal('hide');
                // Clear input fields
                $('#patientIssueDescription').val('');
            },
            error: function () {
                alert('Error');
            }
        })
    }

    $('#submitDailyAvailability').click(function () {

        let date = document.getElementById('dailyAvailabilityModal').getAttribute('date');

        let slots = [];

        for (let i = 0; i < availableDailySlots.length; i++) {
            let dateArray = date.split(' ');
            dateArray[4] = availableDailySlots[i];

            slots[i] = dateArray.join(' ');
        }

        $.ajax({
            type: 'POST',
            data: {
                date: date,
                timeSlots: slots,
            },
            url: '/api/appointment/SubmitDailyAvailability',
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                $('#dailyAvailabilityModal').modal('hide');
                fetchEventAndRenderCalendar();
            },
            error: function (message) {
                $('#workingHours').modal('hide');
            }
        })
    })

    $('#btnWorkingHours').click(function () {

        $.ajax({
            type: "GET",
            url: '/api/appointment/GetWorkingHours',
            success: function (data) {
                $('#workingHours').modal('hide');
            },
            error: function () {
                alert('Error');
                $('#workingHours').modal('hide');
            }
        })

        $('#workingHours').modal();
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
            success: function (data) {
                fetchEventAndRenderCalendar();
                $('#workingHours').modal('hide');
            },
            error: function () {
                alert('Error');
                $('#workingHours').modal('hide');
            }
        })
    })

    $('#btnDelete').click(function () {
        if (userIsAdmin && selectedAppointment.isOwn) {
            let data = {
                id: selectedAppointment.id,
                evaluation: false,
            }

            $.ajax({
                type: "POST",
                url: '/api/appointment/Approve',
                data: data,
                headers: { 'X-CSRF-TOKEN': token },
                success: function (data) {
                    $('#appointmentApproval').modal('hide');
                    fetchEventAndRenderCalendar();
                },
                error: function (err) {
                    alert('Error');
                }
            })

            $('#ownAppointmentModal').modal('hide');
            return;
        }
        $('#ownAppointmentModal').modal('hide');
        $('#declineAppointmentConfirm #declineReasoning').val('');
        $('#declineAppointmentConfirm').modal();
    })

    $('.sendAppointment').click(function () {
        // Validate description
        let userIssueDescription = $('#patientIssueDescription').val().trim();
        if (userIssueDescription == '' || userIssueDescription.length < 30) {
            alert('Please write more than 30 symbols.')
            return;
        }
        // Validate phone number
        let allowedPhoneNumberSymbols = '0123456789+';
        let userPhoneNumber;
        userPhoneNumber = $('#userPhoneNumber').val() !== undefined ? $('#userPhoneNumber').val().trim() : null;
        if (userPhoneNumber !== null && userPhoneNumber !== '') {
            if (userPhoneNumber.includes(char => !allowedPhoneNumberSymbols.includes(char)) ||
                userPhoneNumber.length < 8) {
                alert('Please provide a valid phone number or no phone number at all.');
                return;
            }
        }

        let data = {
            Start: selectedAppointment.start._i,
            Description: $('#patientIssueDescription').val(),
            PhoneNumber: userPhoneNumber,
        }

        $.ajax({
            type: "POST",
            url: '/api/appointment/Save',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                $('#bookAppointmentModal').modal('hide');
                $('#patientIssueDescription').val('');
                fetchEventAndRenderCalendar();
            },
            error: function (err) {
                alert('Error');
            }
        })
    })

    $('#appointmentApproval .approveAppointment').click(function () {
        let data = {
            id: selectedAppointment.id,
            evaluation: true,
        }

        $.ajax({
            type: "POST",
            url: '/api/appointment/Approve',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                $('#appointmentApproval').modal('hide');
                fetchEventAndRenderCalendar();
            },
            error: function (err) {
                alert('Error');
            }
        })
    });

    $('#appointmentApproval .declineAppointment').click(function () {

        $('#declineReasoning')[0].textContent = '';
        $('#declineAppointmentConfirm').modal();
    });

    $('#declineAppointmentConfirm .confirmDeclineAppointment').click(function () {
        let data = {
            id: selectedAppointment.id,
            evaluation: false,
            declineReasoning: $('#declineReasoning').val(),
        }

        $.ajax({
            type: "POST",
            url: '/api/appointment/Approve',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                $('#declineAppointmentConfirm').modal('hide');
                $('#declineReasoning').val('');
                fetchEventAndRenderCalendar();
            },
            error: function (err) {
                alert('Error');
            }
        })
    });
})


