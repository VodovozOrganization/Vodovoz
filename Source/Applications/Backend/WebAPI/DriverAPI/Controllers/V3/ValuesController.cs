using DriverAPI.DTOs.V3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Vodovoz.Services;

namespace DriverAPI.Controllers.V3
{
	[ApiVersion("3.0")]
	[Route("api/v{version:apiVersion}")]
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
		[Route("GetCompanyPhoneNumber")]
		public CompanyNumberResponseDto GetCompanyPhoneNumber()
		{
			return new CompanyNumberResponseDto()
			{
				Number = _webApiParametersProvider.CompanyPhoneNumber
			};
		}
	}
}
