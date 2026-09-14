using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.FastPayments
{
	public class FastPaymentEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderEntity _order;
		private FastPaymentStatus _fastPaymentStatus;

		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Статус оплаты")]
		public virtual FastPaymentStatus FastPaymentStatus
		{
			get => _fastPaymentStatus;
			protected set => SetField(ref _fastPaymentStatus, value);
		}

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
	}
}
