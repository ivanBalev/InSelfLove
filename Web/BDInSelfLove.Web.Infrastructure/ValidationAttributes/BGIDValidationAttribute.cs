using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace BDInSelfLove.Web.Infrastructure.ValidationAttributes
{
    public class BGIDValidationAttribute : ValidationAttribute
    {
        public BGIDValidationAttribute(/*ТУК МОЖЕМ СТАНДАРТНО ДА СИ ПОДАВАМЕ ДАННИ ОТ САМИЯ МОДЕЛ КАКТО ПРИ ДР АТРИБУТИ*/)
        {

        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (Regex.IsMatch(value.ToString(), "[0.9]{10}"))
            {
                return new ValidationResult("Invalid ID");
            }

            return ValidationResult.Success;
        }
    }
}
