// ReSharper disable VirtualMemberCallInConstructor
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
            this.Comments = new HashSet<Comment>();
            this.Appointments = new HashSet<Appointment>();
            this.ReportsSubmitted = new HashSet<Report>();
            this.ReportsReceived = new HashSet<Report>();
            this.Bans = new HashSet<Ban>();
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

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public virtual ICollection<Report> ReportsSubmitted { get; set; }

        public virtual ICollection<Report> ReportsReceived { get; set; }

        public bool IsBanned { get; set; }

        public virtual ICollection<Ban> Bans { get; set; }
    }
}
