using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тип связанного документа кода ЧЗ для промежуточного хранения
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "тип связанного документа кода ЧЗ для промежуточного хранения",
		NominativePlural = "типы связанных документов кодов ЧЗ для промежуточного хранения",
		Prepositional = "типе связанного документа кода ЧЗ для промежуточного хранения",
		PrepositionalPlural = "типах связанных документов кодов ЧЗ для промежуточного хранения",
		Accusative = "тип связанного документа кода ЧЗ для промежуточного хранения",
		AccusativePlural = "типы связанных документов кодов ЧЗ для промежуточного хранения",
		Genitive = "типе связанного документа кода ЧЗ для промежуточного хранения",
		GenitivePlural = "типах связанных документов кодов ЧЗ для промежуточного хранения")]
	public enum StagingTrueMarkCodeRelatedDocumentType
	{
		/// <summary>
		/// Строка талона погрузки
		/// </summary>
		[Display(Name = "Строка талона погрузки")]
		CarLoadDocumentItem,
		/// <summary>
		/// Строка маршрутного листа
		/// </summary>
		[Display(Name = "Строка маршрутного листа")]
		RouteListItem,
		/// <summary>
		/// Строка документа самовывоза
		/// </summary>
		[Display(Name = "Строка документа самовывоза")]
		SelfDeliveryDocumentItem
	}
}
