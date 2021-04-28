using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Models;
using DriverAPI.Models;
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
    public class DriverComplaintsController : ControllerBase
    {
        private readonly IAPIDriverComplaintData iAPIDriverComplaintData;

        public DriverComplaintsController(IAPIDriverComplaintData iAPIDriverComplaintData)
        {
            this.iAPIDriverComplaintData = iAPIDriverComplaintData ?? throw new ArgumentNullException(nameof(iAPIDriverComplaintData));
        }

        /// <summary>
        /// Эндпоинт получения популярных причин
        /// </summary>
        /// <returns>APIDriverComplaintReason Список популярных причин</returns>
        [Authorize]
        [HttpGet]
        [Route("/api/GetDriverComplaintReasons")]
        public IEnumerable<APIDriverComplaintReason> GetDriverComplaintReasons()
        {
            return iAPIDriverComplaintData.GetPinnedComplaintReasons();
        }
    }
}
