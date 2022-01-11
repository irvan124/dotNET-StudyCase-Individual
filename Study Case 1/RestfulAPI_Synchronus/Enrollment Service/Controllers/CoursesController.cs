using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Enrollment_Service.Data.Courses;
using Enrollment_Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourse _course;
        private readonly IMapper _mapper;

        public CoursesController(ICourse course, IMapper mapper)
        {
            _course = course;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CourseDto>> GetAllCourses()
        {
            var result = await _course.GetAll();
            var dto = _mapper.Map<IEnumerable<CourseDto>>(result);
            return Ok(dto);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourseById(int id)
        {
            var result = await _course.GetById(id.ToString());
            if (result == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(result));
        }

        [HttpPost]
        public async Task<ActionResult<CourseDto>> AddCourse(CourseCreateDto input)
        {
            var course = _mapper.Map<Course>(input);
            var result = await _course.Insert(course);

            var dto = _mapper.Map<CourseDto>(result);

            return Ok(dto);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<CourseDto>> UpdateCourse(int id, CourseCreateDto input)
        {
            try
            {
                var course = _mapper.Map<Course>(input);
                var result = await _course.Update(id.ToString(), course);
                var dto = _mapper.Map<CourseDto>(result);

                // return Ok(dto);
                return Ok(new { Message = "Update Success", data = dto });
            }
            catch (System.Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            try
            {
                await _course.Delete(id.ToString());
                return Ok($"Course with ID {id} is successfully deleted");
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}