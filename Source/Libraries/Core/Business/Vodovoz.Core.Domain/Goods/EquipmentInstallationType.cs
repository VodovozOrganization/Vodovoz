using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Тип установки оборудования
	/// </summary>
	public enum EquipmentInstallationType
	{
		/// <summary>
		/// Напольный
		/// </summary>
		[Display(Name = "Напольный")]
		Floor,
		/// <summary>
		/// Настольный
		/// </summary>
		[Display(Name = "Настольный")]
		Desktop,
		/// <summary>
		/// Встраиваемый
		/// </summary>
		[Display(Name = "Встраиваемый")]
		Embedded
	}
}
