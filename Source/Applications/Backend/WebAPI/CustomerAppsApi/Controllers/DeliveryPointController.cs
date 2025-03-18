using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class DeliveryPointController : ControllerBase
	{
		private readonly ILogger<DeliveryPointController> _logger;
		private readonly IDeliveryPointService _deliveryPointService;

		public DeliveryPointController(
			ILogger<DeliveryPointController> logger,
			IDeliveryPointService deliveryPointService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_deliveryPointService = deliveryPointService ?? throw new ArgumentNullException(nameof(deliveryPointService));
		}

		[HttpGet("GetDeliveryPoints")]
		public DeliveryPointsDto GetDeliveryPoints([FromQuery] Source source, int counterpartyErpId)
		{
			return _deliveryPointService.GetDeliveryPoints(source, counterpartyErpId);
		}
		
		[HttpPost("AddDeliveryPoint")]
		public IActionResult AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			var deliveryPointDto = _deliveryPointService.AddDeliveryPoint(newDeliveryPointInfoDto, out var statusCode);

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

		[HttpPost("UpdateOnlineComment")]
		public IActionResult UpdateOnlineComment(UpdatingDeliveryPointCommentDto updatingComment)
		{
			var code = _deliveryPointService.UpdateDeliveryPointOnlineComment(updatingComment);
			
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
