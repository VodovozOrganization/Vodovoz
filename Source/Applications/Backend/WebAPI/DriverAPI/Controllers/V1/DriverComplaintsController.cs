using DriverAPI.Library.DTOs;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DriverAPI.Controllers.V1
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class DriverComplaintsController : ControllerBase
	{
		private readonly IDriverComplaintModel _iAPIDriverComplaintData;

		public DriverComplaintsController(IDriverComplaintModel iAPIDriverComplaintData)
		{
			_iAPIDriverComplaintData = iAPIDriverComplaintData ?? throw new ArgumentNullException(nameof(iAPIDriverComplaintData));
		}

		/// <summary>
		/// Эндпоинт получения популярных причин
		/// </summary>
		/// <returns>Список популярных причин</returns>
		[HttpGet]
		[Route("/api/v1/GetDriverComplaintReasons")]
		[Route("/api/GetDriverComplaintReasons")]
		public IEnumerable<DriverComplaintReasonDto> GetDriverComplaintReasons()
		{
			return _iAPIDriverComplaintData.GetPinnedComplaintReasons();
		}
	}
}
