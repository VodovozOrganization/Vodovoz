using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
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
