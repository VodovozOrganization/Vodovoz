using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Logistics.Drivers
{
	public enum EventQrDocumentType
	{
		[Display(Name = "Маршрутный лист")]
		RouteList,
		[Display(Name = "Талон погрузки")]
		CarLoadDocument,
		[Display(Name = "Талон разгрузки")]
		CarUnloadDocument
	}
}
