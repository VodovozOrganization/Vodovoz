using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
		/// <returns>Список популярных причин</returns>
		[Authorize]
		[HttpGet]
		[Route("/api/GetDriverComplaintReasons")]
		public IEnumerable<APIDriverComplaintReason> GetDriverComplaintReasons()
		{
			return iAPIDriverComplaintData.GetPinnedComplaintReasons();
		}
	}
}
