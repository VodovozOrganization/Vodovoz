using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Типы исходящих ЭДО документов
	/// </summary>
	public enum OutgoingEdoDocumentType
	{
		/// <summary>
		/// Трансфер
		/// </summary>
		[Display(Name = "Трансфер")]
		Transfer,

		/// <summary>
		/// Документ заказа
		/// </summary>
		[Display(Name = "Документ заказа")]
		Order,

		/// <summary>
		/// Неформализованный документ заказа
		/// </summary>
		[Display(Name = "Неформализованный документ заказа")]
		InformalOrderDocument
	}
}
