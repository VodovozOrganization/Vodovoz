using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Тип налога
	/// </summary>
	public enum TaxType
	{
		/// <summary>
		/// Не указано
		/// </summary>
		[Display(Name = "Не указано")]
		None,
		/// <summary>
		/// С НДС
		/// </summary>
		[Display(Name = "С НДС")]
		WithVat,
		/// <summary>
		/// Без НДС
		/// </summary>
		[Display(Name = "Без НДС")]
		WithoutVat
	}
}
