using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "действия с документами эдо и честный знак",
		Nominative = "действия с документом это и честный знак"
	)]
	[HistoryTrace]
	public class OrderEdoTrueMarkDocumentsActions : PropertyChangedBase, IDomainObject
	{
		private Order _order;
		private bool _isNeedToResendEdoUpd;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Требуется переотправка УПД по ЭДО")]
		public bool IsNeedToResendEdoUpd
		{
			get => _isNeedToResendEdoUpd;
			set => SetField(ref _isNeedToResendEdoUpd, value);
		}

		public virtual string Title => $"Действия с документами ЭДО и Честный знак заказа №{Order.Id}";
	}
}
