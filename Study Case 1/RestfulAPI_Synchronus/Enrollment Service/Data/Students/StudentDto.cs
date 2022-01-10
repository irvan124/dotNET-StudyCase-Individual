using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollment_Service.Data.Students
{
    public class StudentDto
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public DateTime EnrollDate { get; set; }
    }
}