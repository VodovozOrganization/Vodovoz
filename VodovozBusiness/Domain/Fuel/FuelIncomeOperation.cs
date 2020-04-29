using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "операция поступления топлива",
		NominativePlural = "операции поступления топлива")]
	public class FuelIncomeOperation : PropertyChangedBase, IDomainObject, ISubdivisionEntity
	{
		public virtual int Id { get; set; }

		private DateTime сreationTime;
		[Display(Name = "Время создания")]
		public virtual DateTime СreationTime {
			get => сreationTime;
			set => SetField(ref сreationTime, value, () => СreationTime);
		}

		private FuelIncomeInvoiceItem fuelIncomeInvoiceItem;
		/// <summary>
		/// Строка входящей накладной по топливу. Заполняется только если операция создавалась в результате поступления топлива
		/// </summary>
		[Display(Name = "Строка входящей накладной по топливу")]
		public virtual FuelIncomeInvoiceItem FuelIncomeInvoiceItem {
			get => fuelIncomeInvoiceItem;
			set => SetField(ref fuelIncomeInvoiceItem, value, () => FuelIncomeInvoiceItem);
		}

		private FuelTransferDocument fuelTransferDocument;
		/// <summary>
		/// Документ транспортировки. Заполняется только если операция создавалась в ходе транспортировки
		/// </summary>
		[Display(Name = "Документ транспортировки топлива")]
		public virtual FuelTransferDocument FuelTransferDocument {
			get => fuelTransferDocument;
			set => SetField(ref fuelTransferDocument, value, () => FuelTransferDocument);
		}

		private Subdivision relatedToSubdivision;
		[Display(Name = "Относится к подразделению")]
		public virtual Subdivision RelatedToSubdivision {
			get => relatedToSubdivision;
			set => SetField(ref relatedToSubdivision, value, () => RelatedToSubdivision);
		}

		private FuelType fuelType;
		[Display(Name = "Тип топлива")]
		public virtual FuelType FuelType {
			get => fuelType;
			set => SetField(ref fuelType, value, () => FuelType);
		}

		private decimal fuelLiters;
		[Display(Name = "Объем топлива")]
		public virtual decimal FuelLiters {
			get => fuelLiters;
			set => SetField(ref fuelLiters, value, () => FuelLiters);
		}
	}
}
