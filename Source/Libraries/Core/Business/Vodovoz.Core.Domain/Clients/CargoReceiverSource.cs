using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public enum CargoReceiverSource
	{
		[Display(Name = "Из контрагента")]
		FromCounterparty,
		[Display(Name = "Из точки доставки")]
		FromDeliveryPoint,
		[Display(Name = "Особый")]
		Special
	}
}
