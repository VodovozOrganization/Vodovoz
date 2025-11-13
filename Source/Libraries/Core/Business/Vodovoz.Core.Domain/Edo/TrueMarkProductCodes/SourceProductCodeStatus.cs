using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	/// <summary>
	/// Статус исходного кода ЧЗ
	/// </summary>
	public enum SourceProductCodeStatus
	{
		/// <summary>
		/// Новый код <br/>
		/// Проверка кода не выполнялась
		/// </summary>
		[Display(Name = "Новый")]
		New,

		/// <summary>
		/// Есть не решаемая проблема с кодом <br/>
		/// Дальнейшее использование кода невозможно
		/// </summary>
		[Display(Name = "Проблема")]
		Problem,

		/// <summary>
		/// Была проблема с кодом, код был заменен <br/>
		/// Можно продолжить работу с замененным кодом в ResultCode
		/// </summary>
		[Display(Name = "Заменен")]
		Changed,

		/// <summary>
		/// Код принят без изменений
		/// </summary>
		[Display(Name = "Принят")]
		Accepted,
		
		/// <summary>
		/// Код сохранен в пул
		/// </summary>
		[Display(Name = "Сохранен в пул")]
		SavedToPool
	}
}
