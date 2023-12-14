using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Controllers
{
	public class DriverWarehouseEventQrDataHandler : IDriverWarehouseEventQrDataHandler
	{
		private readonly IList<ValidationResult> _validationResults = new List<ValidationResult>();
		
		public (DriverWarehouseEventQrData QrData, IEnumerable<ValidationResult> ValidationResults) ConvertQrData(string qrData)
		{
			var result = qrData.Split(DriverWarehouseEvent.QrParametersSeparator);

			if(result[0] != DriverWarehouseEvent.QrType)
			{
				return (null, _validationResults);
			}

			var data = ConvertQrData(result);

			return !ValidateQrData(data) ? (null, _validationResults) : (data, _validationResults);
		}

		private DriverWarehouseEventQrData ConvertQrData(string[] qrData)
		{
			int.TryParse(qrData[1], out var eventId);
			int.TryParse(qrData[2], out var documentId);
			decimal.TryParse(qrData[3], out var latitude);
			decimal.TryParse(qrData[3], out var longitude);

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
