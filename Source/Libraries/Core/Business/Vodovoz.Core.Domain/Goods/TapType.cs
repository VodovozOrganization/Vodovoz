using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Тип кранов
	/// </summary>
	public enum TapType
	{
		/// <summary>
		/// Нажим кнопкой
		/// </summary>
		[Display(Name = "Нажим кнопкой")]
		ButtonPush,
		/// <summary>
		/// Нажим кружкой
		/// </summary>
		[Display(Name = "Нажим кружкой")]
		CupPush,
		/// <summary>
		/// Нажим рукой
		/// </summary>
		[Display(Name = "Нажим рукой")]
		HandPush
	}
}
