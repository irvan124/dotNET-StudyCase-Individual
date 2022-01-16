using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Enrollment_Service.Data.Students;
using Enrollment_Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Enrollment_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudent _student;
        private IMapper _mapper;

        public StudentsController(IStudent student, IMapper mapper)
        {
            _student = student;
            _mapper = mapper;

        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents()
        {

            var result = await _student.GetAll();

            var dto = _mapper.Map<IEnumerable<StudentDto>>(result);
            return Ok(dto);

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetByStudentId(int id)
        {
            var result = await _student.GetById(id.ToString());
            if (result == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<StudentDto>(result));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<StudentDto>> UpdateStudent(int id, StudentCreateDto input)
        {

            try
            {
                var student = _mapper.Map<Student>(input);
                var result = await _student.Update(id.ToString(), student);
                var dto = _mapper.Map<StudentDto>(result);

                return Ok(dto);

            }
            catch (System.Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }
        [HttpPost]
        public async Task<ActionResult<StudentDto>> AddStudent(StudentCreateDto input)
        {
            {
                var student = _mapper.Map<Student>(input);
                var result = await _student.Insert(student);

                var dto = _mapper.Map<StudentDto>(result);

                return Ok(dto);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStudent(int id)
        {
            try
            {
                await _student.Delete(id.ToString());
                return Ok($"Student with ID {id} is successfully deleted");
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}