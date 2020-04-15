$(document).ready(function () {
    var events = [];
    var selectedEvent = null;
    var token = $("#csfrToken input[name=__RequestVerificationToken]").val();
    fetchEventAndRenderCalendar();

    function fetchEventAndRenderCalendar() {
        events = [];
        $.ajax({
            type: "GET",
            url: "/api/appointment/GetAll",
            success: function (data) {
                $.each(data, function (i, v) {
                    events.push({
                        id: v.id,
                        start: moment(v.start),
                        end: moment(v.start).add(1, 'hours'),
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
            timeFormat: 'h(:mm)a',
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month'
            },
            eventLimit: true,
            eventColor: '#378006',
            events: events,
            eventClick: function (calEvent, jsEvent, view) {
                selectedEvent = calEvent;
                $('#myModal #eventTitle').text(calEvent.title);
                var $description = $('<div/>');

                $description.append($('<p/>').html('<b>Start: </b>' + calEvent.start.format("DD/MM/YYYY HH:mm A")));
                $description.append($('<p/>').html('<b>End: </b>' + calEvent.end.format("DD/MM/YYYY HH:mm A")));

                $('#myModal #pDetails').empty().html($description);

                $('#myModal').modal();
            },
            selectable: true,
            select: function (start) {

                if (start.isBefore(moment())) {
                    $('#calendar').fullCalendar('unselect');
                    return false;
                }

                selectedEvent = {
                    id: 0,
                    description: '',
                    start: '',
                };
                openAddForm(start._d);
                $('#calendar').fullCalendar('unselect');
            },
        })
    }


    function openAddForm(date) {
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
        $('#workingHours').modal();
    })

    $('#workingHoursSubmitBtn').click(function () {
        // VALIDATE

        let startHour = document.querySelector('#startTime').value;
        let endHour = document.querySelector('#endTime').value;

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

        var e = document.getElementById("txtStart");

        var data = {
            Id: $('#hdEventID').val(),
            Start: e.options[e.selectedIndex].value,
            Description: $('#txtDescription').val(),
        }

        SaveEvent(data);
    })
    function SaveEvent(data) {
        $.ajax({
            type: "POST",
            url: '/api/appointment/Save',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                if (data.status) {
                    fetchEventAndRenderCalendar();
                    $('#myModalSave').modal('hide');
                }
            },
            error: function () {
                alert('Error');
            }
        })
    }
})


