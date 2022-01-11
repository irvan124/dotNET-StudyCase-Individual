using System;
using System.Collections.Generic;

#nullable disable

namespace PaymentService.Models
{
    public partial class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public string Grade { get; set; }
    }
}
