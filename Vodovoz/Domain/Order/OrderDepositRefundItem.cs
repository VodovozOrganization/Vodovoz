using System;
using Vodovoz.Domain.Operations;
using QSOrmProject;

namespace Vodovoz.Domain.Orders
{
	public class OrderDepositRefundItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Order order;

		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
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
					return "Возврат залога за бутыли";
				case DepositType.Equipment:
					return "Возврат залога за оборудования";
				default:
					return "Не определено";
				}
			} 
		}

		Decimal refundDeposit;

		public virtual Decimal RefundDeposit {
			get { return refundDeposit; }
			set { SetField (ref refundDeposit, value, () => RefundDeposit); }
		}
	}
}

