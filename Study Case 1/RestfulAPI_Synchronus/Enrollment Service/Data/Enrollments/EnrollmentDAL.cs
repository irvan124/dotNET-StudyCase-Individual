using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enrollment_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Enrollment_Service.Data.Enrollments
{
    public class EnrollmentDAL : IEnrollment
    {
        private readonly EnrollmentDBContext _context;

        public EnrollmentDAL(EnrollmentDBContext context)
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
                _context.Enrollments.Remove(result);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {

                throw new Exception($"Error : {dbEx.Message}");
            }
        }

        public async Task<IEnumerable<Enrollment>> GetAll()
        {
            var result = await _context.Enrollments.Include(e => e.Student).Include(e => e.Course).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<Enrollment> GetById(string id)
        {
            var result = await _context.Enrollments.Where(e => e.EnrollmentId == Convert.ToInt32(id)).SingleOrDefaultAsync<Enrollment>();

            return result;
        }

        public async Task<Enrollment> Insert(Enrollment obj)
        {
            try
            {
                _context.Enrollments.Add(obj);
                await _context.SaveChangesAsync();
                return obj;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Error: {dbEx.Message}");
            }
        }

        public async Task<Enrollment> Update(string id, Enrollment obj)
        {
            try
            {
                var result = await GetById(id);
                if (result == null)
                {
                    throw new Exception($"Student has not found");
                }
                result.EnrollmentId = obj.EnrollmentId;
                result.CourseId = obj.CourseId;
                result.StudentId = obj.StudentId;
                result.Grade = obj.Grade;

                await _context.SaveChangesAsync();

                obj.EnrollmentId = Convert.ToInt32(id);
                obj.CourseId = Convert.ToInt32(id);
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