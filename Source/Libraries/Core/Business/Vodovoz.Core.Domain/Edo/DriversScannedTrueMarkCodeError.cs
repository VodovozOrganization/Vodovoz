using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тип ошибки, возникшей при обработке отсканированных водителем кодов ЧЗ
	/// </summary>
	public enum DriversScannedTrueMarkCodeError
	{
		/// <summary>
		/// Нет ошибки
		/// </summary>
		[Display(Name = "Нет ошибки")]
		None,
		/// <summary>
		/// Отсканированный код уже содержится в отсканированном коде верхнего уровня
		/// </summary>
		[Display(Name = "Отсканированный код уже содержится в отсканированном коде верхнего уровня")]
		HighLevelCodesScanned,
		/// <summary>
		/// Код уже содержится в базе
		/// </summary>
		[Display(Name = "Код уже содержится в базе")]
		Duplicate,
		/// <summary>
		/// В ЧЗ нет информации о коде
		/// </summary>
		[Display(Name = "В ЧЗ нет информации о коде")]
		NotTrueMarkCode,
		/// <summary>
		/// Ошибка запроса к API ЧЗ
		/// </summary>
		[Display(Name = "Ошибка запроса к API ЧЗ")]
		TrueMarkApiRequestError,
		/// <summary>
		/// При обработке кода возникло исключение
		/// </summary>
		[Display(Name = "При обработке кода возникло исключение")]
		Exception
	}
}
