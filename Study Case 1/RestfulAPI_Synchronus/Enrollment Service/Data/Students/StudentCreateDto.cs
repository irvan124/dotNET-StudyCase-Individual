using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Enrollment_Service.ValidationAttributes;

namespace Enrollment_Service.Data.Students
{
    [StudentValidation]
    public class StudentCreateDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime EnrollDate { get; set; }

    }
}