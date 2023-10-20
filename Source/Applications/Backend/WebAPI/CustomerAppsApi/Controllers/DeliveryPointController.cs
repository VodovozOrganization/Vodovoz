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
		public AddedDeliveryPointDto AddDeliveryPoint(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			return _deliveryPointModel.AddDeliveryPoint(newDeliveryPointInfoDto);
		}
		
		[HttpPost("UpdateOnlineComment")]
		public UpdatedDeliveryPointCommentDto UpdateOnlineComment(UpdatingDeliveryPointCommentDto updatingComment)
		{
			return _deliveryPointModel.UpdateDeliveryPointOnlineComment(updatingComment);
		}
	}
}
