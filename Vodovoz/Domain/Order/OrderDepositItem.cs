using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
		NominativePlural = "залоги в заказе",
		Nominative = "залог в заказе"
	)]
	public class OrderDepositItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Order order;

		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		int count;

		public virtual int Count {
			get { return count; }
			set { SetField (ref count, value, () => Count); }
		}

		PaymentDirection paymentDirection;

		public virtual PaymentDirection PaymentDirection {
			get { return paymentDirection; }
			set { SetField (ref paymentDirection, value, () => PaymentDirection); }
		}

		DepositOperation depositOperation;

		public virtual DepositOperation DepositOperation {
			get { return depositOperation; }
			set { SetField (ref depositOperation, value, () => DepositOperation); }
		}

		PaidRentEquipment paidRentItem;

		public virtual PaidRentEquipment PaidRentItem {
			get { return paidRentItem; }
			set { SetField (ref paidRentItem, value, () => PaidRentItem); }
		}

		FreeRentEquipment freeRentItem;

		public virtual FreeRentEquipment FreeRentItem {
			get { return freeRentItem; }
			set { SetField (ref freeRentItem, value, () => FreeRentItem); }
		}

		DepositType depositType;

		public virtual DepositType DepositType {
			get { return depositType; }
			set { SetField (ref depositType, value, () => DepositType); }
		}

		public virtual string DepositTypeString {
			get { 
				switch (DepositType) {
				case DepositType.Bottles:
					if (PaymentDirection == PaymentDirection.FromClient)
						return "Залог за бутыли";
					return "Возврат залога за бутыли";
				case DepositType.Equipment:
					if (PaymentDirection == PaymentDirection.FromClient)
						return "Залог за оборудование";
					return "Возврат залога за оборудования";
				default:
					return "Не определено";
				}
			} 
		}

		Decimal deposit;

		public virtual Decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		public virtual Decimal Total { get { return Deposit * Count; } }

		public string Title {
			get{
				return String.Format("{0} на сумму {1}", DepositTypeString, CurrencyWorks.GetShortCurrencyString(Total));
			}
		}
	}

	public enum PaymentDirection
	{
		[Display (Name = "Клиенту")]ToClient,
		[Display (Name = "От клиента")]FromClient
	}

	public class PaymentDirectionStringType : NHibernate.Type.EnumStringType
	{
		public PaymentDirectionStringType () : base (typeof(PaymentDirection))
		{
		}
	}
}

