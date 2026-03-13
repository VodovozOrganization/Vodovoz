using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тип кода ЧЗ для промежуточного хранения
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "тип кода ЧЗ для промежуточного хранения",
		NominativePlural = "типы кодов ЧЗ для промежуточного хранения",
		Prepositional = "типе кода ЧЗ для промежуточного хранения",
		PrepositionalPlural = "типах кодов ЧЗ для промежуточного хранения",
		Accusative = "тип кода ЧЗ для промежуточного хранения",
		AccusativePlural = "типы кодов ЧЗ для промежуточного хранения",
		Genitive = "типа кода ЧЗ для промежуточного хранения",
		GenitivePlural = "типы кодов ЧЗ для промежуточного хранения")]
	public enum StagingTrueMarkCodeType
	{
		/// <summary>
		/// Код экземпляра
		/// </summary>
		[Display(Name = "Код экземпляра")]
		Identification,
		/// <summary>
		/// Групповой код
		/// </summary>
		[Display(Name = "Групповой код")]
		Group,
		/// <summary>
		/// Транспортный код
		/// </summary>
		[Display(Name = "Транспортный код")]
		Transport
	}
}
