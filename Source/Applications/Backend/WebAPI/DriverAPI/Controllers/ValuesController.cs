using DriverAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Vodovoz.Services;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ValuesController : ControllerBase
	{
		private readonly IDriverApiParametersProvider _webApiParametersProvider;

		public ValuesController(IDriverApiParametersProvider webApiParametersProvider)
		{
			_webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
		}

		// GET: GetRouteList 
		[HttpGet]
		[AllowAnonymous]
		[Route("/api/GetCompanyPhoneNumber")]
		public CompanyNumberResponseDto GetCompanyPhoneNumber()
		{
			return new CompanyNumberResponseDto()
			{
				Number = _webApiParametersProvider.CompanyPhoneNumber
			};
		}
	}
}
