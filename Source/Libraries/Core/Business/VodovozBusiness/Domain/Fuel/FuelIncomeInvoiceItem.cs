using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "входящая накладная по топливу",
		NominativePlural = "входящие накладные по топливу")]
	public class FuelIncomeInvoiceItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private FuelIncomeInvoice fuelIncomeInvoice;
		[Display(Name = "Входящая накладная по топливу")]
		public virtual FuelIncomeInvoice FuelIncomeInvoice {
			get => fuelIncomeInvoice;
			set => SetField(ref fuelIncomeInvoice, value, () => FuelIncomeInvoice);
		}

		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value, () => Nomenclature);
		}

		private decimal liters;
		[Display(Name = "Объем топлива")]
		public virtual decimal Liters {
			get => liters;
			set => SetField(ref liters, value, () => Liters);
		}

		private decimal price;
		[Display(Name = "Цена")]
		public virtual decimal Price {
			get => price;
			set => SetField(ref price, value, () => Price);
		}

		private FuelIncomeOperation fuelIncomeOperation;
		[Display(Name = "Операция поступления топлива")]
		public virtual FuelIncomeOperation FuelIncomeOperation {
			get => fuelIncomeOperation;
			set => SetField(ref fuelIncomeOperation, value, () => FuelIncomeOperation);
		}

		public virtual decimal TotalSum => Liters * Price;

		public virtual void UpdateOperation()
		{
			if(FuelIncomeOperation == null) {
				FuelIncomeOperation = new FuelIncomeOperation();
				FuelIncomeOperation.СreationTime = FuelIncomeInvoice.СreationTime;
				FuelIncomeOperation.FuelIncomeInvoiceItem = this;
			}
			FuelIncomeOperation.RelatedToSubdivision = FuelIncomeInvoice.Subdivision;
			FuelIncomeOperation.FuelType = Nomenclature.FuelType;
			FuelIncomeOperation.FuelLiters = Liters;
		}
	}
}
