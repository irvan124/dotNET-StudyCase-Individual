using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;


namespace Enrollment_Service.Profiles
{
    public class StudentsProfile : Profile
    {
        public StudentsProfile()
        {
            CreateMap<Models.Student, Data.Students.StudentDto>()
               .ForMember(dest => dest.Name,
               opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<Data.Students.StudentCreateDto, Models.Student>();
        }
    }
}