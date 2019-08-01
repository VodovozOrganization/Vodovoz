using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "операция списания топлива",
		NominativePlural = "операции списания топлива")]
	[HistoryTrace]
	public class FuelExpenseOperation : PropertyChangedBase, IDomainObject, ISubdivisionEntity
	{
		public virtual int Id { get; set; }

		private DateTime сreationTime;
		[Display(Name = "Время создания")]
		public virtual DateTime СreationTime {
			get => сreationTime;
			set => SetField(ref сreationTime, value, () => СreationTime);
		}

		private FuelDocument fuelDocument;
		/// <summary>
		/// Документ выдачи топлива. Заполняется только если операция создавалась в результате выдачи топлива
		/// </summary>
		[Display(Name = "Документ выдачи топлива")]
		public virtual FuelDocument FuelDocument {
			get => fuelDocument;
			set => SetField(ref fuelDocument, value, () => FuelDocument);
		}

		/// <summary>
		/// Строка акта списания топлива. Заполняется только если операция создавалась из акта списания
		/// </summary>
		private FuelWriteoffDocumentItem fuelWriteoffDocumentItem;
		[Display(Name = "Строка акта списания топлива")]
		public virtual FuelWriteoffDocumentItem FuelWriteoffDocumentItem {
			get => fuelWriteoffDocumentItem;
			set => SetField(ref fuelWriteoffDocumentItem, value, () => FuelWriteoffDocumentItem);
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

		public virtual string Title => string.Format("{0} №{1}", this.GetType().GetSubjectName(), Id);
	}
}
