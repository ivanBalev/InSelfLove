﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Appointment
{
    public class AppointmentManipulateModel
    {
        public int Id { get; set; }

        public bool CanBeOnSite { get; set; }

        public bool IsOnSite { get; set; }
    }
}
