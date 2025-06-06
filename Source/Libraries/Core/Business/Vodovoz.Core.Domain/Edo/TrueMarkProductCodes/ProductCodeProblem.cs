using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	/// <summary>
	/// Тип проблемы кода ЧЗ
	/// </summary>
	public enum ProductCodeProblem
	{
		/// <summary>
		/// Нет
		/// </summary>
		[Display(Name = "Нет")]
		None,

		/// <summary>
		/// Дефект бутыля
		/// </summary>
		[Display(Name = "Дефект бутыля")]
		Defect,

		/// <summary>
		/// Код не отсканирован
		/// </summary>
		[Display(Name = "Код не отсканирован")]
		Unscanned,

		/// <summary>
		/// Дубликат кода
		/// </summary>
		[Display(Name = "Дубликат кода")]
		Duplicate
	}
}
