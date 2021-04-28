using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {
        // GET: GetRouteList 
        [HttpGet]
        [Route("/api/GetCompanyPhoneNumber")]
        public string GetCompanyPhoneNumber()
        {
            return "Here must be phone number";
        }
    }
}
