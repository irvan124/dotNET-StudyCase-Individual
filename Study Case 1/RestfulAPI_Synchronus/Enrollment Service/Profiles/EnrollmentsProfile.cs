using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Enrollment_Service.Profiles
{
    public class EnrollmentsProfile : Profile
    {
        public EnrollmentsProfile()
        {
            CreateMap<Models.Enrollment, Data.Enrollments.EnrollmentDto>();
            CreateMap<Data.Enrollments.EnrollmentCreateDto, Models.Enrollment>();
        }
    }
}