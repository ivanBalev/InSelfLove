using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Contact
{
    public class ContactFormInputModel
    {
        [MinLength(30, ErrorMessage = "Please enter more than 30 characters.")]
        [Required(ErrorMessage = "Please enter more than 30 characters.")]
        public string Message { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string RecaptchaToken { get; set; }

        public string RecaptchaExpectedAction { get; set; }
    }
}
