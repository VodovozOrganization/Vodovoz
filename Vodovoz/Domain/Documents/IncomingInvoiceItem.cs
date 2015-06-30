using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (JournalName = "Строки накладной", ObjectName = "строка накладной")]
	public class IncomingInvoiceItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual IncomingInvoice Document { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature);
				if (IncomeGoodsOperation.Nomenclature != nomenclature)
					IncomeGoodsOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment);
				if (IncomeGoodsOperation.Equipment != equipment)
					IncomeGoodsOperation.Equipment = equipment;
			}
		}

		decimal amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount);
				if (IncomeGoodsOperation.Amount != amount)
					IncomeGoodsOperation.Amount = amount;
			}
		}

		decimal price;

		[Min (0)]
		[Display (Name = "Цена")]
		public virtual decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		public decimal Sum {
			get {return Price * Amount;}
		}

		public virtual string Name {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string EquipmentString { 
			get { return Equipment != null ? Equipment.Serial : "-"; } 
		}

		public virtual bool CanEditAmount { 
			get { return Nomenclature != null && !Nomenclature.Serial; }
		}

		GoodsMovementOperation incomeGoodsOperation = new GoodsMovementOperation ();

		public GoodsMovementOperation IncomeGoodsOperation {
			get { return incomeGoodsOperation; }
			set { SetField (ref incomeGoodsOperation, value, () => IncomeGoodsOperation); }
		}

	}
}

