using System.Net.Mime;
using Contracts.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;

namespace SecureCodeSenderApi.Controllers.V1
{
	public class SecureCodeController : VersionedController
	{
		public SecureCodeController(
			ILogger<ApiControllerBase> logger)
			: base(logger)
		{
			
		}
		
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult SendSecureCode(SendSecureCodeDto sendSecureCodeDto)
		{
			
			return Ok();
		}
		
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult CheckSecureCode([FromBody] CheckSecureCodeDto checkSecureCodeDto)
		{
			
			
			return Ok();
		}
	}
}
