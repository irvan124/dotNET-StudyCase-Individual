using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Enrollment_Service.Data.Courses;

namespace Enrollment_Service.Profiles
{
    public class CoursesProfile : Profile
    {
        public CoursesProfile()
        {
            CreateMap<Models.Course, CourseDto>()
                .ForMember(dest => dest.TotalHours,
                opt => opt.MapFrom(src => src.Credits * 1.5));

            CreateMap<CourseCreateDto, Models.Course>();
        }
    }
}