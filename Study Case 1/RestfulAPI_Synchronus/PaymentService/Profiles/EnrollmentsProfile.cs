using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace PaymentService.Profiles
{
    public class EnrollmentsProfile : Profile
    {
        public EnrollmentsProfile()
        {
            CreateMap<Models.Enrollment, Dto.EnrollmentDto>();
            CreateMap<Dto.EnrollmentCreateDto, Models.Enrollment>();
        }
    }
}