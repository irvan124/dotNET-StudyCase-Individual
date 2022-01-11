using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Enrollment_Service.Data.Enrollments;
using Microsoft.Extensions.Configuration;

namespace Enrollment_Service.SyncDataService.Http
{
    public class HttpPaymentDataClient : IPaymentDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpPaymentDataClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task SendDataEnrollmentToPayment(EnrollmentCreateDto input)
        {
            var httpContent = new StringContent(
              JsonSerializer.Serialize(input),
              Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_configuration["PaymentService"],
                httpContent);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--> POST request to Payment service success");
            }
            else
            {
                Console.WriteLine("--> POST request to Payment service failed");
            }
        }
    }
}