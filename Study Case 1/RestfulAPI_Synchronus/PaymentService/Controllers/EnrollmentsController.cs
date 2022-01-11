using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Dto;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollment _enrollment;
        private readonly IMapper _mapper;

        public EnrollmentsController(IMapper mapper, IEnrollment enrollment)
        {
            _enrollment = enrollment;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<EnrollmentDto>> AddEnrollment(EnrollmentCreateDto input)
        {
            Console.WriteLine("Incoming Data form Enrollment Service");
            var enrollment = _mapper.Map<Enrollment>(input);
            var result = await _enrollment.AddEnrollment(enrollment);

            var dto = _mapper.Map<EnrollmentDto>(result);

            return Ok(dto);

        }
    }
}