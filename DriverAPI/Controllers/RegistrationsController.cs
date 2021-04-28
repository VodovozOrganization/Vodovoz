using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RegistrationsController : ControllerBase
    {
        // POST: RegisterDriverActions
        [HttpPost]
        [Route("/api/RegisterDriverActions")]
        public IActionResult RegisterDriverActions([FromBody] IEnumerable<DriverActionModel> driverActionModels)
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST: RegisterRouteListAddressCoordinates
        [HttpPost]
        [Route("/api/RegisterRouteListAddressCoordinates")]
        public IActionResult RegisterRouteListAddressCoordinate([FromBody] RouteListAddressCoordinate routeListAddressCoordinate)
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST: RegisterTrackCoordinates
        [HttpPost]
        [Route("/api/RegisterTrackCoordinates")]
        public IActionResult RegisterTrackCoordinates([FromBody] RegisterTrackCoordinateRequestModel registerTrackCoordinateRequestModel)
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
