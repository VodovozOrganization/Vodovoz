using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Drivers
{
	public enum DriverWarehouseEventType
	{
		[Display(Name = "На местности")]
		OnLocation,
		[Display(Name = "На документах")]
		OnDocuments
	}
}
