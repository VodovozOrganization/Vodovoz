using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Тип крепления стаканодержателя
	/// </summary>
	public enum CupHolderBracingType
	{
		/// <summary>
		/// Магнит
		/// </summary>
		[Display(Name = "Магнит")]
		Magnet,
		/// <summary>
		/// Стаканодержатель
		/// </summary>
		[Display(Name = "Стаканодержатель")]
		CupHolder
	}
}
