using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public partial class TrueMarkDocument
	{
		/// <summary>
		/// Тип документа Честного знака
		/// </summary>
		[Appellative(
			Gender = GrammaticalGender.Masculine,
			Nominative = "тип документа Честного знака",
			NominativePlural = "типы документов Честного Знака",
			Genitive = "типа документа Честного знака",
			GenitivePlural = "типов документов Честного знака",
			Accusative = "типа документа Честного знака",
			AccusativePlural = "типов документов Честного знака",
			Prepositional = "типе документа Честного знака",
			PrepositionalPlural = "типах документов Честного знака")]
		public enum TrueMarkDocumentType
		{
			/// <summary>
			/// Вывод из оборота
			/// </summary>
			[Display(Name = "Вывод из оборота")]
			Withdrawal,

			/// <summary>
			/// Отмена вывода из оборота
			/// </summary>
			[Display(Name = "Отмена вывода из оборота")]
			WithdrawalCancellation
		}

	}
}
