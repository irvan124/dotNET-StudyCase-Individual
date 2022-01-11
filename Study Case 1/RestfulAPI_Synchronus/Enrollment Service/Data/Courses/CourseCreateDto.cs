using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enrollment_Service.ValidationAttributes;

namespace Enrollment_Service.Data.Courses
{
    // [CourseValidation]
    public class CourseCreateDto
    {
        public string Title { get; set; }
        public int Credits { get; set; }
    }
}