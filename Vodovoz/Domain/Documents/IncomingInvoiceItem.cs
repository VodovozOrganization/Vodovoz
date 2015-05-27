using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QSProjectsLib;
using System;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Строки накладной", ObjectName = "строка накладной")]
	public class IncomingInvoiceItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		int amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		decimal price;

		[Min (0)]
		[Display (Name = "Цена")]
		public virtual decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		public virtual string Name {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string EquipmentString { 
			get { return Equipment != null ? Equipment.Serial : "-"; } 
		}

		public virtual bool CanEditAmount { 
			get { return Nomenclature == null ? false : Nomenclature.Category != NomenclatureCategory.equipment; }
		}
	}
}

