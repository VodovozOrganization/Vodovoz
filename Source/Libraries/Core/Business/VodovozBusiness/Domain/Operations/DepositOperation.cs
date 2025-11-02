using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Utilities;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "изменения залогов",
		Nominative = "изменение залогов")]
	public class DepositOperation : OperationBase
	{
		Order order;

		public virtual Order Order {
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		DepositType depositType;

		public virtual DepositType DepositType {
			get => depositType;
			set => SetField(ref depositType, value, () => DepositType);
		}

		Counterparty counterparty;

		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value, () => Counterparty);
		}

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint {
			get => deliveryPoint;
			set => SetField(ref deliveryPoint, value, () => DeliveryPoint);
		}

		decimal receivedDeposit;

		public virtual decimal ReceivedDeposit {
			get => receivedDeposit;
			set => SetField(ref receivedDeposit, value, () => ReceivedDeposit);
		}

		decimal refundDeposit;

		public virtual decimal RefundDeposit {
			get => refundDeposit;
			set => SetField(ref refundDeposit, value, () => RefundDeposit);
		}

		public virtual string Title {
			get {
				if(RefundDeposit > 0)
					return string.Format(
						"Возврат залога за {0} на сумму {1}",
						DepositType.GetEnumTitle(),
						CurrencyWorks.GetShortCurrencyString(RefundDeposit)
					);
				return string.Format(
					"Получение залога за {0} на сумму {1}",
					DepositType.GetEnumTitle(),
					CurrencyWorks.GetShortCurrencyString(ReceivedDeposit)
				);
			}
		}
	}
}
