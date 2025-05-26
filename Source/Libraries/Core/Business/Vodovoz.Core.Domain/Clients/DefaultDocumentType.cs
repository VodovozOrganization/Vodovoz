using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Тип документа по умолчанию
	/// </summary>
	public enum DefaultDocumentType
	{
		/// <summary>
		/// УПД
		/// </summary>
		[ItemTitle("УПД")]
		[Display(Name = "УПД")]
		upd,
		/// <summary>
		/// ТОРГ-12 + Счет-Фактура
		/// </summary>
		[ItemTitle("ТОРГ-12 + Счет-Фактура")]
		[Display(Name = "ТОРГ-12 + Счет-Фактура")]
		torg12
	}
}
