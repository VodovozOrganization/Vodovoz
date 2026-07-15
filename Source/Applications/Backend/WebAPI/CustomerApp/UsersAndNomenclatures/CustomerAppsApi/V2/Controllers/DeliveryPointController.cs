using System;
using CustomerAppsApi.Library.V2.Dto;
using CustomerAppsApi.Library.V2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	public class DeliveryPointController : VersionedController
	{
		private readonly IDeliveryPointService _deliveryPointService;

		public DeliveryPointController(
			ILogger<DeliveryPointController> logger,
			IDeliveryPointService deliveryPointService) : base(logger)
		{
			_deliveryPointService = deliveryPointService ?? throw new ArgumentNullException(nameof(deliveryPointService));
		}

		[HttpGet]
		public DeliveryPointsDto GetDeliveryPoints([FromQuery] Source source, int counterpartyErpId)
		{
			return _deliveryPointService.GetDeliveryPoints(source, counterpartyErpId);
		}
		
		[HttpPost]
		public IActionResult AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
			
			var deliveryPointDto = _deliveryPointService.AddDeliveryPoint(newDeliveryPointInfoDto, out var statusCode, isDryRun);

			if(deliveryPointDto is null)
			{
				switch(statusCode)
				{
					case 202:
						return Accepted();
					default:
						return StatusCode(500);
				}
			}
			
			return Created("", deliveryPointDto);
		}

		[HttpPost]
		public IActionResult UpdateOnlineComment(UpdatingDeliveryPointCommentDto updatingComment)
		{
			var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
			
			var code = _deliveryPointService.UpdateDeliveryPointOnlineComment(updatingComment, isDryRun);
			
			switch(code)
			{
				case 404:
					return NotFound();
				case 500:
					return StatusCode(500);
				default:
					return Ok();
			}
		}
	}
}
