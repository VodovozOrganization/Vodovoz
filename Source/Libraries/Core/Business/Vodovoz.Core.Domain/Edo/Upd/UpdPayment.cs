using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo.Upd
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "Платежи",
		Nominative = "Платеж")]
	public class UpdPayment : PropertyChangedBase, IDomainObject
	{
		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id { get; set; }

		/// <summary>
		/// Номер платежа
		/// </summary>
		[Display(Name = "Номер платежа")]
		public virtual string PaymentNum { get; set; }

		/// <summary>
		/// Дата платежа, в формате dd.MM.yyyy
		/// </summary>
		[Display(Name = "Дата платежа")]
		public virtual string PaymentDate { get; set; }
	}
}
