using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Enrollment_Service.Data.Students;

namespace Enrollment_Service.ValidationAttributes
{
    public class StudentValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var student = (StudentCreateDto)validationContext.ObjectInstance;
            if (student.FirstName == student.LastName)
            {
                return new ValidationResult("Firstname cannot be same as Lastname", new[] { nameof(StudentCreateDto) });
            }
            return ValidationResult.Success;
        }
    }
}