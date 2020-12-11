using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Azure.Platform.HealthCheck.Controllers
{
    public class HealthCheckEndpointController : Controller
    {

        private readonly IConfiguration _configuration;

        private const string ConfigKey = "status";

        private const string ConfigExpectedValue = "azure";

        private const string SecretConfigKey = "password-password";

        private const string SecretConfigExpectedValue = "londonbridgeisfallingdown";

        private string MachineName = Environment.MachineName;       
        

        public HealthCheckEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/ping")]
        public IActionResult GetStandardHealthCheck()
        {
            var hcAppSetting = _configuration.GetValue<string>(ConfigKey);
            string failInstanceName = _configuration["FailInstanceName"];
            if (!string.IsNullOrWhiteSpace(hcAppSetting)
                && hcAppSetting.ToLowerInvariant().Equals(ConfigExpectedValue.ToLowerInvariant()))
            {

                //Simulate fault on a single instance and one app out of multiple apps hosted on instance
                if (MachineName == failInstanceName && Request.Host.Value.ToLower().Contains("app1"))
                {                    
                    return StatusCode(500);
                }

                var result = new { MachineName, HttpStatusCode.OK };
                
                return Ok(result);
            }
            
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/ping-secrets-vault")]
        public IActionResult GetSecrets()
        {
            var secretPassword = _configuration.GetValue<string>(SecretConfigKey);
            if (!string.IsNullOrWhiteSpace(secretPassword)
                && secretPassword.ToLowerInvariant().Equals(SecretConfigExpectedValue.ToLowerInvariant()))
            {
                return Ok(secretPassword);
            }

            var notFoundResult = new { MachineName, HttpStatusCode.ServiceUnavailable };            
            return NotFound(notFoundResult);
        }
    }
}