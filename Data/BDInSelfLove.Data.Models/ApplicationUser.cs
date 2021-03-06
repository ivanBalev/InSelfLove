﻿// ReSharper disable VirtualMemberCallInConstructor
namespace BDInSelfLove.Data.Models
{
    using System;
    using System.Collections.Generic;

    using BDInSelfLove.Data.Common.Models;

    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser, IAuditInfo, IDeletableEntity
    {
        public ApplicationUser()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Roles = new HashSet<IdentityUserRole<string>>();
            this.Claims = new HashSet<IdentityUserClaim<string>>();
            this.Logins = new HashSet<IdentityUserLogin<string>>();
            this.Appointments = new HashSet<Appointment>();
            this.Comments = new HashSet<Comment>();
        }

        // Audit info
        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        // Deletable entity
        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }

        public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; }

        public string ProfilePhoto { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public bool IsBanned { get; set; }

        public string WindowsTimezoneId { get; set; }
    }
}
