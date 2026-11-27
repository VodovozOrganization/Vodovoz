using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Models.Cash
{
	/// <summary>
	/// Уровни группировки отчёта ДДС и ДДР
	/// </summary>
	public enum CashFlowReportGrouping
	{
		/// <summary>
		/// Иерархия групп и статей
		/// </summary>
		[Display(Name = "Статья")]
		Article,

		/// <summary>
		/// Центр финансовой ответственности
		/// </summary>
		[Display(Name = "Центр финансовой ответственности")]
		CfoSubdivision,

		/// <summary>
		/// Центр финансовых затрат
		/// </summary>
		[Display(Name = "Центр финансовых затрат")]
		CfzSubdivision
	}
}
