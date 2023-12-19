using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Drivers
{
	public class DriverWarehouseEventQrData : IValidatableObject
	{
		public int EventId { get; set; }
		public int? DocumentId { get; set; }
		public decimal? Latitude { get; set; }
		public decimal? Longitude { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(EventId == default(int))
			{
				yield return new ValidationResult("Id события не может быть равным нулю");
			}
		}
	}
}
