$(document).ready(function () {
    var events = [];
    var selectedEvent = null;
    var availableDailySlots = [];
    var token = $("#csfrToken input[name=__RequestVerificationToken]").val();
    var themeColor = {
        RGB: "rgb(170, 85, 132)",
        
    }
    fetchEventAndRenderCalendar();

    //Add listeners for daily working hours
    if (document.getElementById('dailyHoursSetter')) {
        let date = document.getElementById('myModalSave').getAttribute('date');
        let slots = document.getElementsByClassName('dailyTimeSlot');

        for (let i = 0; i < slots.length; i++) {
            let slot = slots[i];

            slot.addEventListener('click', function (e) {
                let slotTime = slot.innerHTML.trim();

                if (availableDailySlots.includes(slotTime)) {
                    availableDailySlots = availableDailySlots.filter(s => s !== slotTime);
                    e.target.style.backgroundColor = "white";
                } else {
                    availableDailySlots.push(slotTime);
                    e.target.style.backgroundColor = "rgb(170, 85, 132)";
                }
            })
        }
    }

    async function fetchEventAndRenderCalendar() {
        events = [];
        await $.ajax({
            type: "GET",
            url: "/api/appointment/GetAll",
            success: function (dbEvents) {
                $.each(dbEvents, function (i, currentEvent) {
                    events.push({
                        id: currentEvent.id,
                        start: moment(currentEvent.start),
                        end: moment(currentEvent.start).add(1, 'hours'),
                        clientName: 'Pesho',
                    });
                })

                GenerateCalendar(events);
            },
            error: function (request, textStatus, error) {
                if (request.getResponseHeader('location') != undefined) {
                    window.location.href = request.getResponseHeader('location');
                } else {
                    alert('Error');
                }

            }
        })
    }

    function GenerateCalendar(events) {
        $('#calendar').fullCalendar('destroy');
        $('#calendar').fullCalendar({
            contentHeight: 700,
            defaultDate: new Date(),
            timeFormat: 'h:mm',
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'agendaWeek month'
            },
            eventLimit: true,
            eventColor: '#aa5584',
            defaultView: 'agendaWeek',
            eventTextColor: 'white',
            displayEventTime: true,
            events: events,
            eventClick: eventClickFunc,
            selectable: true,
            eventRender: (eventObj, $el) => {
                $el[0].innerText =  eventObj.clientName;
            },
            eventAfterRender: (event, $el, view) => {
                $el[0].setAttribute('style', $el[0].getAttribute('style') + ' text-align: center;');
                console.log($el[0].getAttribute('class'));
            },
            select: (start) => {
                if (start.isBefore(moment())) {
                    $('#calendar').fullCalendar('unselect');
                    return false;
                }

                selectedEvent = {
                    id: 0,
                    description: '',
                    start: '',
                };
                if (document.getElementById('dailyHoursSetter')) {
                    openDailyHoursForm(start._d);
                } else {
                    openAddForm(start._d);
                }
                $('#calendar').fullCalendar('unselect');
            },
            selectLongPressDelay: 0,
        })
    }

    const eventClickFunc = (calEvent, jsEvent, view) => {
        selectedEvent = calEvent;
        $('#myModal #eventTitle').text(calEvent.title);
        var $description = $('<div/>');

        $description.append($('<p/>').html('<b>Start: </b>' + calEvent.start.format("DD/MM/YYYY HH:mm A")));
        $description.append($('<p/>').html('<b>End: </b>' + calEvent.end.format("DD/MM/YYYY HH:mm A")));

        $('#myModal #pDetails').empty().html($description);

        $('#myModal').modal();
    }

    const openDailyHoursForm = (date) => {
        //For each element
        //on click
        //make background certain colour if no colour or remove colour 
        //and, more importantly
        //add the text to a collection which will later be sent to server

        //IN DB!! - just create fake appointments for unavailable slots

        document.getElementById('myModalSave').setAttribute('date', date);



        //check if date exists in dailyTimeSlots
        //submitDailyAvailability must trigger clearing of availableDailySlots and colors in table

        $('#myModalSave').modal();
    }

    $('#closeDailyAvailability').click(function () {
        let slots = document.getElementsByClassName('dailyTimeSlot');

        for (let i = 0; i < slots.length; i++) {
            slots[i].style.backgroundColor = "white";
        }

        availableDailySlots = [];
    })

    $('#submitDailyAvailability').click(function () {

        let date = document.getElementById('myModalSave').getAttribute('date');

        let slots = [];

        for (let i = 0; i < availableDailySlots.length; i++) {
            let dateArray = date.split(' ');
            dateArray[4] = availableDailySlots[i];

            slots[i] = dateArray.join(' ');
        }

        console.log(slots);

        $.ajax({
            type: 'POST',
            data: {
                timeSlots: slots,
            },
            url: '/api/appointment/SubmitDailyAvailability',
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                $('#workingHours').modal('hide');

                let slots = document.getElementsByClassName('dailyTimeSlot');
                for (let i = 0; i < slots.length; i++) {
                    slots[i].style.backgroundColor = "white";
                }
                availableDailySlots = [];
                $('#myModalSave').modal('hide');
            },
            error: function (message) {
                console.log(message);
                $('#workingHours').modal('hide');
            }
        })
    })

    const openAddForm = (date) => {
        $.ajax({
            type: "GET",
            data: { date: date },
            url: "/api/appointment/GetAppointmentsByDate",
            success: function (appointments) {

                var select = document.getElementById("txtStart");
                select.innerHTML = '';

                $.each(appointments, function (i, v) {
                    var option = document.createElement("option");
                    option.text = moment(v.start).format("HH:mm");
                    option.value = moment(v.start).format("DD.MM.YYYY HH:mm");
                    select.appendChild(option);
                })

                $('#myModal').modal('hide');
                $('#myModalSave').modal();
            },
            error: function (error) {
                alert('Error');
            }
        })
    }

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
        // VALIDATE

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
        if (selectedEvent != null && confirm('Are you sure?')) {
            $.ajax({
                type: "DELETE",
                url: '/api/appointment/Delete',
                data: { 'id': selectedEvent.id },
                headers: { 'X-CSRF-TOKEN': token },
                success: function (data) {
                    if (data.status) {
                        //Refresh the calender
                        fetchEventAndRenderCalendar();
                        $('#myModal').modal('hide');
                    }
                },
                error: function () {
                    alert('Error');
                }
            })
        }
    })

    $('#btnSave').click(function () {
        //Validation/
        if ($('#txtStart').val().trim() == "") {
            alert('Start date required');
            return;
        }
        if ($('#txtDescription').val().trim() == "" || $('#txtDescription').val().trim().length < 30) {
            alert('Description longer than 30 symbols required.')
            return;
        }

        var time = document.getElementById("txtStart");

        var data = {
            Id: $('#hdEventID').val(),
            Start: time.options[time.selectedIndex].value,
            Description: $('#txtDescription').val(),
        }

        SaveEvent(data);


    })
    const SaveEvent = async function(data) {
        $.ajax({
            type: "POST",
            url: '/api/appointment/Save',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: async function (data) {
                if (data.status) {
                    await fetchEventAndRenderCalendar();
                    $('#myModalSave').modal('hide');
                    // Clear input fields
                    $('#txtDescription').val("");
                }
            },
            error: function () {
                alert('Error');
            }
        })
    }
})


