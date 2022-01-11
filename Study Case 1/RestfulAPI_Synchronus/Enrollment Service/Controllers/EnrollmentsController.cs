using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Enrollment_Service.Data.Enrollments;
using Enrollment_Service.Models;
using Enrollment_Service.SyncDataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Student")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollment _enrollment;
        private readonly IMapper _mapper;
        private readonly IPaymentDataClient _paymentDataClient;

        public EnrollmentsController(IEnrollment enrollment, IMapper mapper, IPaymentDataClient paymentDataClient)
        {
            _enrollment = enrollment;
            _mapper = mapper;
            _paymentDataClient = paymentDataClient;
        }
        [HttpGet]
        public async Task<ActionResult<EnrollmentDto>> GetAllEnrollments()
        {
            var result = await _enrollment.GetAll();
            var dto = _mapper.Map<IEnumerable<EnrollmentDto>>(result);

            return Ok(dto);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<EnrollmentDto>> GetEnrollmentById(int id)
        {
            var result = await _enrollment.GetById(id.ToString());
            if (result == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<EnrollmentDto>(result));
        }
        [HttpPost]
        public async Task<ActionResult<EnrollmentDto>> AddEnrollment(EnrollmentCreateDto input)
        {
            var enrollment = _mapper.Map<Enrollment>(input);
            var result = await _enrollment.Insert(enrollment);

            var dto = _mapper.Map<EnrollmentDto>(result);

            try
            {
                await _paymentDataClient.SendDataEnrollmentToPayment(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
            }


            return Ok(dto);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<EnrollmentDto>> UpdateEnrollment(int id, EnrollmentCreateDto input)
        {
            try
            {
                var enrollment = _mapper.Map<Enrollment>(input);
                var result = await _enrollment.Update(id.ToString(), enrollment);
                var dto = _mapper.Map<EnrollmentDto>(result);

                return Ok(dto);

            }
            catch (System.Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }
        [HttpDelete]
        public async Task<ActionResult> DeleteEnrollment(int id)
        {
            try
            {
                await _enrollment.Delete(id.ToString());
                return Ok($"Enrollment with ID {id} is successfully deleted");
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

    }
}