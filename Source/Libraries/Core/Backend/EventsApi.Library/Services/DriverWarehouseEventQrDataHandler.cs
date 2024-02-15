using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using EventsApi.Library.Dtos;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace EventsApi.Library.Services
{
	public class DriverWarehouseEventQrDataHandler : IDriverWarehouseEventQrDataHandler
	{
		private readonly ILogger<DriverWarehouseEventQrDataHandler> _logger;
		private readonly IList<ValidationResult> _validationResults;

		public DriverWarehouseEventQrDataHandler(
			ILogger<DriverWarehouseEventQrDataHandler> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_validationResults = new List<ValidationResult>();
		}
		
		public DriverWarehouseEventQrData ConvertQrData(string qrData)
		{
			var result = qrData.Split(DriverWarehouseEvent.QrParametersSeparator);

			if(result[0] != DriverWarehouseEvent.QrType)
			{
				_logger.LogError("Неправильный Qr код: {QrData}", qrData);
				return null;
			}

			var data = ConvertQrData(result);

			if(!ValidateQrData(data))
			{
				var sb = new StringBuilder();
					
				foreach(var validationResult in _validationResults)
				{
					sb.AppendLine(validationResult.ErrorMessage);
				}
					
				_logger.LogError("Не прошли валидацию: {ValidationResult}", sb.ToString());
				
				return null;
			}

			return data;
		}

		private DriverWarehouseEventQrData ConvertQrData(string[] qrData)
		{
			int.TryParse(qrData[1], out var eventId);
			int.TryParse(qrData[2], out var documentId);
			decimal.TryParse(qrData[3], out var latitude);
			decimal.TryParse(qrData[4], out var longitude);

			return new DriverWarehouseEventQrData
			{
				EventId = eventId,
				DocumentId = documentId,
				Latitude = latitude,
				Longitude = longitude
			};
		}
		
		private bool ValidateQrData(DriverWarehouseEventQrData data)
		{
			_validationResults.Clear();
			
			return Validator.TryValidateObject(data, new ValidationContext(data), _validationResults, true);
		}
	}
}
