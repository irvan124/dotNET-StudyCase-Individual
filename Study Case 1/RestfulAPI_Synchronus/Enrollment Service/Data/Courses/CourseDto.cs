using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollment_Service.Data.Courses
{
    public class CourseDto
    {
        public int CourseID { get; set; }
        public string Title { get; set; }

        public float TotalHours { get; set; }

    }
}