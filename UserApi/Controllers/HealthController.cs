using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserApi.Controllers
{
    [Route("HealthCheck")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        public IActionResult Check()
        {
            return Ok();
        }
    }
}