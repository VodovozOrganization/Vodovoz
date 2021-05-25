using DriverAPI.Models;
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
		private readonly IDriverApiParametersProvider webApiParametersProvider;

		public ValuesController(IDriverApiParametersProvider webApiParametersProvider)
		{
			this.webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
		}

		// GET: GetRouteList 
		[HttpGet]
		[Route("/api/GetCompanyPhoneNumber")]
		public CompanyNumberResponseModel GetCompanyPhoneNumber()
		{
			return new CompanyNumberResponseModel()
			{
				Number = webApiParametersProvider.CompanyPhoneNumber
			};
		}
	}
}
