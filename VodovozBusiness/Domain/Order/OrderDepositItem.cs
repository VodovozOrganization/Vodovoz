using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Orders
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "залоги в заказе",
		Nominative = "залог в заказе"
	)]
	public class OrderDepositItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		int count;

		[Display(Name = "Количество")]
		public virtual int Count {
			get { return count; }
			set { SetField (ref count, value, () => Count); }
		}

		int actualCount;

		[Display(Name = "Фактическое количество")]
		public virtual int ActualCount {
			get { return actualCount; }
			set { SetField(ref actualCount, value, () => ActualCount); }
		}

		PaymentDirection paymentDirection;

		[Display(Name = "Кто платит")]
		public virtual PaymentDirection PaymentDirection {
			get { return paymentDirection; }
			set { SetField (ref paymentDirection, value, () => PaymentDirection); }
		}

		DepositOperation depositOperation;

		[Display(Name = "Операция залога")]
		public virtual DepositOperation DepositOperation {
			get { return depositOperation; }
			set { SetField (ref depositOperation, value, () => DepositOperation); }
		}

		PaidRentEquipment paidRentItem;

		[Display(Name = "Залог за оплачиваемое оборудование")]
		public virtual PaidRentEquipment PaidRentItem {
			get { return paidRentItem; }
			set { SetField (ref paidRentItem, value, () => PaidRentItem); }
		}

		FreeRentEquipment freeRentItem;

		[Display(Name = "Залог за бесплатное оборудование")]
		public virtual FreeRentEquipment FreeRentItem {
			get { return freeRentItem; }
			set { SetField (ref freeRentItem, value, () => FreeRentItem); }
		}

		Nomenclature equipmentNomenclature;

		[Display(Name = "Номенклатура оборудования")]
		public virtual Nomenclature EquipmentNomenclature {
			get { return equipmentNomenclature; }
			set { SetField (ref equipmentNomenclature, value, () => EquipmentNomenclature); }
		}

		DepositType depositType;

		[Display(Name = "Тип залога")]
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

		[Display(Name = "Залог")]
		public virtual Decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		/// <summary>
		/// Свойство возвращает подходяшее значение Count или ActualCount в зависимости от статуса заказа.
		/// </summary>
		public int CurrentCount {
			get {
				if(OrderRepository.GetStatusesForActualCount(Order).Contains(Order.OrderStatus)) {
					return ActualCount;
				} else {
					return Count;
				}
			}
		}

		public virtual Decimal Total { get { return Deposit * CurrentCount; } }

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

