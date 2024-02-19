using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Logistics.Drivers
{
	public enum DriverWarehouseEventType
	{
		[Display(Name = "На местности")]
		OnLocation,
		[Display(Name = "На документах")]
		OnDocuments
	}
}
