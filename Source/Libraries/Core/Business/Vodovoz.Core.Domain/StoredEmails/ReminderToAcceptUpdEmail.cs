using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Напоминание о необходимости принятия УПД
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Neuter,
		Accusative = "напоминание о необходимости принятия УПД",
		AccusativePlural = "напоминания о необходимости принятия УПД",
		Genitive = "напоминания о необходимости принятия УПД",
		GenitivePlural = "напоминаний о необходимости принятия УПД",
		Nominative = "напоминание о необходимости принятия УПД",
		NominativePlural = "напоминания о необходимости принятия УПД",
		Prepositional = "напоминании о необходимости принятия УПД",
		PrepositionalPlural = "напоминаниях о необходимости принятия УПД")]
	public class ReminderToAcceptUpdEmail : CounterpartyEmail
	{
		/// <inheritdoc />
		public override CounterpartyEmailType Type => CounterpartyEmailType.ReminderToAcceptUpd;

		/// <summary>
		/// Документ заказа
		/// </summary>
		[Display(Name = "Документ заказа")]
		public virtual OrderDocumentEntity OrderDocument { get; set; }

		public override IEmailableDocument EmailableDocument { get; }
	}
}
