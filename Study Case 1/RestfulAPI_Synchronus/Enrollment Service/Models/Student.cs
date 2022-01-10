using System;
using System.Collections.Generic;

#nullable disable

namespace Enrollment_Service.Models
{
    public partial class Student
    {
        public Student()
        {
            Enrollments = new HashSet<Enrollment>();
        }

        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime EnrollDate { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
