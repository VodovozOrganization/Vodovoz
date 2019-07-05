using System;
using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;
using QS.HistoryLog;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "акт списания топлива",
		NominativePlural = "акты списания топлива")]
	[HistoryTrace]
	public class FuelWriteoffDocumentItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private FuelWriteoffDocument fuelWriteoffDocument;
		[Display(Name = "Акт списания топлива")]
		public virtual FuelWriteoffDocument FuelWriteoffDocument {
			get => fuelWriteoffDocument;
			set => SetField(ref fuelWriteoffDocument, value, () => FuelWriteoffDocument);
		}

		private FuelExpenseOperation fuelExpenseOperation;
		[Display(Name = "Операция списания топлива")]
		public virtual FuelExpenseOperation FuelExpenseOperation {
			get => fuelExpenseOperation;
			set => SetField(ref fuelExpenseOperation, value, () => FuelExpenseOperation);
		}

		private FuelType fuelType;
		[Display(Name = "Тип топлива")]
		public virtual FuelType FuelType {
			get => fuelType;
			set => SetField(ref fuelType, value, () => FuelType);
		}

		private decimal liters;
		[Display(Name = "Количество списываемого топлива")]
		public virtual decimal Liters {
			get => liters;
			set => SetField(ref liters, value, () => Liters);
		}

		public virtual void UpdateOperation()
		{
			if(FuelExpenseOperation == null) {
				FuelExpenseOperation = new FuelExpenseOperation();
				FuelExpenseOperation.СreationTime = DateTime.Now;
				FuelExpenseOperation.FuelWriteoffDocumentItem = this;
			}
			FuelExpenseOperation.RelatedToSubdivision = FuelWriteoffDocument.CashSubdivision;
			FuelExpenseOperation.FuelType = FuelType;
			FuelExpenseOperation.FuelLiters = Liters;
		}
	}
}
