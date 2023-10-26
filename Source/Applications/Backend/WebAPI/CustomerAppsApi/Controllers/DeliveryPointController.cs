using System;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class DeliveryPointController : ControllerBase
	{
		private readonly ILogger<DeliveryPointController> _logger;
		private readonly IDeliveryPointModel _deliveryPointModel;

		public DeliveryPointController(
			ILogger<DeliveryPointController> logger,
			IDeliveryPointModel deliveryPointModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_deliveryPointModel = deliveryPointModel ?? throw new ArgumentNullException(nameof(deliveryPointModel));
		}

		[HttpGet("GetDeliveryPoints")]
		public DeliveryPointsDto GetDeliveryPoints([FromQuery] Source source, int counterpartyErpId)
		{
			return _deliveryPointModel.GetDeliveryPoints(source, counterpartyErpId);
		}
		
		[HttpPost("AddDeliveryPoint")]
		public IActionResult AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			var deliveryPointDto = _deliveryPointModel.AddDeliveryPoint(newDeliveryPointInfoDto, out var statusCode);

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
			var code = _deliveryPointModel.UpdateDeliveryPointOnlineComment(updatingComment);
			
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
