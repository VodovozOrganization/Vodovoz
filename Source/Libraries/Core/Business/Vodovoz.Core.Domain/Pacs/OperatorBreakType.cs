using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	/// <summary>
	/// Тип перерыва
	/// </summary>
	public enum OperatorBreakType
	{
		/// <summary>
		/// Большой
		/// </summary>
		[Display(Name = "Большой")]
		Long,
		/// <summary>
		/// Малый
		/// </summary>
		[Display(Name = "Малый")]
		Short
	}
}
