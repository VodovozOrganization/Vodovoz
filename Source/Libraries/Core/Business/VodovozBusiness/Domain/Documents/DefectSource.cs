using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents
{
	/// <summary>
	/// Откуда брак
	/// </summary>
	public enum DefectSource
	{
		/// <summary>
		/// Нет
		/// </summary>
		[Display(Name = "")]
		None,
		/// <summary>
		/// Водитель
		/// </summary>
		[Display(Name = "Водитель")]
		Driver,
		/// <summary>
		/// Клиент
		/// </summary>
		[Display(Name = "Клиент")]
		Client,
		/// <summary>
		/// Производство
		/// </summary>
		[Display(Name = "Производство")]
		Production,
		/// <summary>
		/// Склад
		/// </summary>
		[Display(Name = "Склад")]
		Warehouse
	}
}
