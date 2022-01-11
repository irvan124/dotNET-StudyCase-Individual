using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data
{
    public class EnrollmentDAL : IEnrollment
    {
        private readonly PaymentDBContext _context;

        public EnrollmentDAL(PaymentDBContext context)
        {
            _context = context;
        }
        public async Task<Enrollment> AddEnrollment(Enrollment obj)
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


    }
}