using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Стадия создания сделки в Битрикс24
	/// </summary>
	public enum BitrixDealCreationStage
	{
		/// <summary>
		/// Сделка в Битрикс24 не создана
		/// </summary>
		[Display(Name = "Сделка не создана")]
		DealNotCreated,

		/// <summary>
		/// Сделка в Битрикс24 создана
		/// </summary>
		[Display(Name = "Сделка создана")]
		DealCreated
	}
}
