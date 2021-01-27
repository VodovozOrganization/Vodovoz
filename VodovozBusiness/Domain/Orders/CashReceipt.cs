using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using VodovozInfrastructure.Attributes;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "чеки на отправку",
		Nominative = "чек на отправку")]
	public class CashReceipt : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Order order;
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value);
		}

		private bool sent;
        [RestrictedHistoryProperty]
        [Display(Name = "Чек отправлен?")]
		public virtual bool Sent{
			get => sent;
			set => SetField(ref sent, value);
		}

		private int? httpCode;
		[Display(Name = "HTTP код статуса отправки")]
		public virtual int? HttpCode {
			get => httpCode;
			set => SetField(ref httpCode, value);
		}
	}
}
