using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "сырьё",
		Nominative = "сырьё")]
	public class IncomingWaterMaterial: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual IncomingWater Document { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature);
				if (ConsumptionMaterialOperation.Nomenclature != nomenclature)
					ConsumptionMaterialOperation.Nomenclature = nomenclature;
			}
		}

		int amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount);
				if (ConsumptionMaterialOperation.Amount != amount)
					ConsumptionMaterialOperation.Amount = amount;
			}
		}

		public virtual string Name {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		GoodsMovementOperation consumptionMaterialOperation = new GoodsMovementOperation ();

		public GoodsMovementOperation ConsumptionMaterialOperation {
			get { return consumptionMaterialOperation; }
			set { SetField (ref consumptionMaterialOperation, value, () => ConsumptionMaterialOperation); }
		}

	}
}

