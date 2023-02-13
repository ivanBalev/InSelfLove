// ReSharper disable VirtualMemberCallInConstructor
namespace InSelfLove.Data.Models
{
    using System;
    using System.Collections.Generic;

    using InSelfLove.Data.Common.Models;

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
            this.Courses = new HashSet<Course>();
            this.Payments = new HashSet<Payment>();
        }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public string ProfilePhoto { get; set; }

        public bool IsBanned { get; set; }

        public string Timezone { get; set; }

        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }

        public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Course> Courses { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }
}
