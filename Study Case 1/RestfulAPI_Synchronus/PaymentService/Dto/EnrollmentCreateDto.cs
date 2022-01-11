using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Dto
{
    public class EnrollmentCreateDto
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public string Grade { get; set; }
    }
}