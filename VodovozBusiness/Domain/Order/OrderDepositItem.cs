using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Repositories.Orders;

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
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		int count;

		[Display(Name = "Количество")]
		public virtual int Count {
			get => count;
			set => SetField(ref count, value, () => Count);
		}

		int? actualCount;

		[Display(Name = "Фактическое количество")]
		public virtual int? ActualCount {
			get => actualCount;
			set => SetField(ref actualCount, value, () => ActualCount);
		}

		DepositOperation depositOperation;

		[Display(Name = "Операция залога")]
		public virtual DepositOperation DepositOperation {
			get => depositOperation;
			set => SetField(ref depositOperation, value, () => DepositOperation);
		}

		PaidRentEquipment paidRentItem;

		[Display(Name = "Залог за оплачиваемое оборудование")]
		public virtual PaidRentEquipment PaidRentItem {
			get => paidRentItem;
			set => SetField(ref paidRentItem, value, () => PaidRentItem);
		}

		FreeRentEquipment freeRentItem;

		[Display(Name = "Залог за бесплатное оборудование")]
		public virtual FreeRentEquipment FreeRentItem {
			get => freeRentItem;
			set => SetField(ref freeRentItem, value, () => FreeRentItem);
		}

		Nomenclature equipmentNomenclature;

		[Display(Name = "Номенклатура оборудования")]
		public virtual Nomenclature EquipmentNomenclature {
			get => equipmentNomenclature;
			set => SetField(ref equipmentNomenclature, value, () => EquipmentNomenclature);
		}

		DepositType depositType;

		[Display(Name = "Тип залога")]
		public virtual DepositType DepositType {
			get => depositType;
			set => SetField(ref depositType, value, () => DepositType);
		}

		public virtual string DepositTypeString {
			get { 
				switch (DepositType) {
					case DepositType.Bottles:
						return "Возврат залога за бутыли";
					case DepositType.Equipment:
						return "Возврат залога за оборудование";
					default:
						return "Не определено";
				}
			} 
		}

		Decimal deposit;

		[Display(Name = "Залог")]
		public virtual Decimal Deposit {
			get => deposit;
			set => SetField(ref deposit, value, () => Deposit);
		}

		/// <summary>
		/// Свойство возвращает подходяшее значение Count или ActualCount в зависимости от статуса заказа.
		/// </summary>
		public int CurrentCount => ActualCount ?? Count;

		public virtual decimal Total => Deposit * CurrentCount;

		public string Title => string.Format("{0} на сумму {1}", DepositTypeString, CurrencyWorks.GetShortCurrencyString(Total));
	}
}

