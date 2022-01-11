using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enrollment_Service.Data.Enrollments;

namespace Enrollment_Service.SyncDataService
{
    public interface IPaymentDataClient
    {
        Task SendDataEnrollmentToPayment(EnrollmentCreateDto input);
    }
}