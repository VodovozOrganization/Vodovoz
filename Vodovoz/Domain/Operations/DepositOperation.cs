using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Gamma.Utilities;
using QSProjectsLib;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "изменения залогов",
		Nominative = "изменение залогов")]
	public class DepositOperation: OperationBase
	{
		Order order;

		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		DepositType depositType;

		public virtual DepositType DepositType {
			get { return depositType; }
			set { SetField (ref depositType, value, () => DepositType); }
		}

		Counterparty counterparty;

		public virtual Counterparty Counterparty {
			get { return counterparty; }
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField (ref deliveryPoint, value, () => DeliveryPoint); }
		}

		Decimal receivedDeposit;

		public virtual Decimal ReceivedDeposit {
			get { return receivedDeposit; }
			set { SetField (ref receivedDeposit, value, () => ReceivedDeposit); }
		}

		Decimal refundDeposit;

		public virtual Decimal RefundDeposit {
			get { return refundDeposit; }
			set { SetField (ref refundDeposit, value, () => RefundDeposit); }
		}

		public virtual string Title {get { 
				if(RefundDeposit > 0)
					return String.Format("Возврат залога за {0} на сумму {1}",
						DepositType.GetEnumTitle(),
						CurrencyWorks.GetShortCurrencyString(RefundDeposit)); 
				else
					return String.Format("Получение залога за {0} на сумму {1}",
						DepositType.GetEnumTitle(),
						CurrencyWorks.GetShortCurrencyString(ReceivedDeposit)); 
			}}
	}

	public enum DepositType
	{
		[Display(Name = "Отсутствует")] None,
		[Display(Name = "Тара")] Bottles,
		[Display(Name = "Оборудование")] Equipment
	}

	public class DepositTypeStringType : NHibernate.Type.EnumStringType
	{
		public DepositTypeStringType () : base (typeof(DepositType))
		{
		}
	}

}

