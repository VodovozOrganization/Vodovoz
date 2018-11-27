using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки накладной",
		Nominative = "строка накладной")]
	[HistoryTrace]
	public class IncomingInvoiceItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual IncomingInvoice Document { get; set; }

		Nomenclature nomenclature;

		[Required(ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField(ref nomenclature, value, () => Nomenclature);
				if(IncomeGoodsOperation.Nomenclature != nomenclature)
					IncomeGoodsOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set {
				SetField(ref equipment, value, () => Equipment);
				if(IncomeGoodsOperation.Equipment != equipment)
					IncomeGoodsOperation.Equipment = equipment;
			}
		}

		decimal amount;

		[Min(1)]
		[Display(Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set {
				SetField(ref amount, value, () => Amount);
				if(IncomeGoodsOperation.Amount != amount)
					IncomeGoodsOperation.Amount = amount;
			}
		}

		[Min(0)]
		[Display(Name = "Цена")]
		public virtual decimal PrimeCost {
			get { return IncomeGoodsOperation.PrimeCost; }
			set {
				if(value != IncomeGoodsOperation.PrimeCost)
					IncomeGoodsOperation.PrimeCost = value;
			}
		}

		VAT vat;

		public virtual VAT VAT {
			get { return vat; }
			set { SetField(ref vat, value, () => VAT); }
		}

		public virtual decimal Sum => PrimeCost * Amount;

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : "";

		public virtual string EquipmentString => Equipment != null && Equipment.Nomenclature.IsSerial ? Equipment.Serial : "-";

		public virtual bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		WarehouseMovementOperation incomeGoodsOperation = new WarehouseMovementOperation();

		public virtual WarehouseMovementOperation IncomeGoodsOperation {
			get { return incomeGoodsOperation; }
			set { SetField(ref incomeGoodsOperation, value, () => IncomeGoodsOperation); }
		}

		public virtual string Title {
			get {
				return String.Format("[{2}] {0} - {1}",
					Nomenclature.Name,
				    Nomenclature.Unit.MakeAmountShortStr(Amount),
					Document.Title);
			}
		}
	}
}