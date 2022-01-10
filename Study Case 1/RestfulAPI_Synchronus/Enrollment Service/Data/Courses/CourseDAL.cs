using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enrollment_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Enrollment_Service.Data.Courses
{
    public class CourseDAL : ICourse
    {
        private readonly EnrollmentDBContext _context;

        public CourseDAL(EnrollmentDBContext context)
        {
            _context = context;
        }
        public async Task Delete(string id)
        {
            try
            {
                var result = await GetById(id);
                if (result == null) throw new Exception($"Course {id} not found");

                //Deleting the data desired
                _context.Courses.Remove(result);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException DbEx)
            {

                throw new Exception(DbEx.Message);
            }
        }

        public async Task<IEnumerable<Course>> GetAll()
        {
            var results = await _context.Courses.Select(c => c).ToListAsync();

            return results;
        }

        public async Task<Course> GetById(string id)
        {
            var result = await _context.Courses.Where(c => c.CourseId == Convert.ToInt32(id)).SingleOrDefaultAsync<Course>();

            return result;
        }

        public async Task<Course> Insert(Course obj)
        {
            try
            {
                _context.Courses.Add(obj);
                await _context.SaveChangesAsync();
                return obj;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Error: {dbEx.Message}");
            }
        }

        public async Task<Course> Update(string id, Course obj)
        {
            try
            {
                var result = await GetById(id);
                if (result == null)
                {
                    throw new Exception($"Course {id} not found");
                }

                result.Title = obj.Title;
                result.Credits = obj.Credits;
                await _context.SaveChangesAsync();

                return obj;

            }
            catch (Exception ex)
            {

                throw new Exception($"Error: {ex.Message}");
            }
        }
    }
}