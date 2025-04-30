using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Источник получения груза
	/// </summary>
	public enum CargoReceiverSource
	{
		/// <summary>
		/// Из контрагента
		/// </summary>
		[Display(Name = "Из контрагента")]
		FromCounterparty,
		/// <summary>
		/// Из точки доставки
		/// </summary>
		[Display(Name = "Из точки доставки")]
		FromDeliveryPoint,
		/// <summary>
		/// Особый
		/// </summary>
		[Display(Name = "Особый")]
		Special
	}
}
