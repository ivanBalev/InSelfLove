$(document).ready(function () {
    var events = [];
    var selectedEvent = null;
    var token = $("#csfrToken input[name=__RequestVerificationToken]").val();
    fetchEventAndRenderCalendar();

    function fetchEventAndRenderCalendar() {
        events = [];
        $.ajax({
            type: "GET",
            url: "/appointment/GetAll",
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
            error: function (error) {
                alert('Failed');
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
                right: 'month,basicWeek,basicDay,agenda'
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
                console.log(start);
                selectedEvent = {
                    id: 0,
                    description: '',
                    start: '',
                };
                openAddEditForm(start._d);
                $('#calendar').fullCalendar('unselect');
            },
        })
    }

    $('#btnEdit').click(function () {
        //Open modal dialog for edit event
        openAddEditForm();
    })
    $('#btnDelete').click(function () {
        if (selectedEvent != null && confirm('Are you sure?')) {
            $.ajax({
                type: "POST",
                url: '/appointment/Delete',
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
                    alert('Failed');
                }
            })
        }
    })

    //function openAddEditForm() {
    //    if (selectedEvent != null) {
    //        $('#hdEventID').val(selectedEvent.id);

    //        if (selectedEvent.start != '') {
    //            $('#txtStart').val(selectedEvent.start.format("DD.MM.YYYY HH:mm"));
    //        }

    //        $('#txtDescription').val(selectedEvent.description);
    //    }
    //    $('#myModal').modal('hide');
    //    $('#myModalSave').modal();
    //}

    function openAddEditForm(date) {
        $.ajax({
            type: "GET",
            data: { date: date },
            url: "/appointment/GetAppointmentsByDate",
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
                alert('Failed');
            }
        })
    }
    $('#btnSave').click(function (test) {
        //Validation/
        console.log(test);
        if ($('#txtStart').val().trim() == "") {
            alert('Start date required');
            return;
        }

        var e = document.getElementById("txtStart");
        console.log(e);
        console.log(e.options[e.selectedIndex].text);

        var data = {
            Id: $('#hdEventID').val(),
            Start: e.options[e.selectedIndex].value,
            Description: $('#txtDescription').val(),
        }
        SaveEvent(data);
        // call function for submit data to the server 
    })
    function SaveEvent(data) {
        $.ajax({
            type: "POST",
            url: '/appointment/Save',
            data: data,
            headers: { 'X-CSRF-TOKEN': token },
            success: function (data) {
                if (data.status) {
                    //Refresh the calender
                    fetchEventAndRenderCalendar();
                    $('#myModalSave').modal('hide');
                }
            },
            error: function () {
                alert('Failed');
            }
        })
    }
})


