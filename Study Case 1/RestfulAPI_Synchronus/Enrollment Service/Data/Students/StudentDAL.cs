using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enrollment_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Enrollment_Service.Data.Students
{
    public class StudentDAL : IStudent
    {
        private readonly EnrollmentDBContext _context;

        public StudentDAL(EnrollmentDBContext context)
        {
            _context = context;
        }

        public async Task Delete(string id)
        {
            var result = await GetById(id);
            if (result == null)
            {

                throw new Exception("Data not found");
            }

            try
            {
                _context.Students.Remove(result);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {

                throw new Exception($"Error : {dbEx.Message}");
            }
        }

        public async Task<IEnumerable<Student>> GetAll()
        {
            var results = await _context.Students.Select(s => s).ToListAsync();

            return results;
        }

        public async Task<Student> GetById(string id)
        {
            var result = await _context.Students.Where(s => s.StudentId == Convert.ToInt32(id)).SingleOrDefaultAsync<Student>();

            return result;
        }

        public async Task<Student> Insert(Student obj)
        {
            try
            {
                _context.Students.Add(obj);
                await _context.SaveChangesAsync();
                return obj;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Error: {dbEx.Message}");
            }
        }

        public async Task<Student> Update(string id, Student obj)
        {
            try
            {
                var result = await GetById(id);
                if (result == null)
                {
                    throw new Exception($"Student has not found");
                }
                result.FirstName = obj.FirstName;
                result.LastName = obj.LastName;
                result.EnrollDate = obj.EnrollDate;
                await _context.SaveChangesAsync();
                obj.StudentId = Convert.ToInt32(id);
                return obj;

            }
            catch (Exception ex)
            {

                throw new Exception($"Error: {ex.Message}");
            }

        }
    }
}