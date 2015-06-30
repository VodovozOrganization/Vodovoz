using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки списания",
		Nominative = "строка списания")]
	public class WriteoffDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual WriteoffDocument Document { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value, () => Nomenclature);
				if (WriteOffGoodsOperation.Nomenclature != nomenclature)
					WriteOffGoodsOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set {
				SetField (ref equipment, value, () => Equipment);
				if (WriteOffGoodsOperation.Equipment != equipment)
					WriteOffGoodsOperation.Equipment = equipment;
			}
		}

		CullingCategory cullingCategory;

		[Display (Name = "Оборудование")]
		public virtual CullingCategory CullingCategory {
			get { return cullingCategory; }
			set { SetField (ref cullingCategory, value, () => CullingCategory); }
		}

		decimal amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set {
				SetField (ref amount, value, () => Amount);
				if (WriteOffGoodsOperation.Amount != amount)
					WriteOffGoodsOperation.Amount = amount;
			}
		}

		public virtual string Name {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string EquipmentString { 
			get { return Equipment != null ? Equipment.Serial : "-"; } 
		}

		public virtual string CullingCategoryString {
			get { return CullingCategory != null ? CullingCategory.Name : "-"; }
		}

		public virtual bool CanEditAmount { 
			get { return Nomenclature != null && !Nomenclature.Serial; }
		}

		GoodsMovementOperation writeoffGoodsOperation = new GoodsMovementOperation ();

		public GoodsMovementOperation WriteOffGoodsOperation {
			get { return writeoffGoodsOperation; }
			set { SetField (ref writeoffGoodsOperation, value, () => WriteOffGoodsOperation); }
		}

	}
}

