using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollment_Service.Data.Students
{
    public class StudentCreateDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime EnrollDate { get; set; }

    }
}