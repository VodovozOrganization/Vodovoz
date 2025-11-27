using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Cash
{
	/// <summary>
	/// Ставки НДС
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Accusative = "категорию штрафов",
		AccusativePlural = "категории штрафов",
		Genitive = "категории штрафов",
		GenitivePlural = "категорий штрафов",
		Nominative = "категория штрафов",
		NominativePlural = "категории штрафов",
		Prepositional = "категории штрафов",
		PrepositionalPlural = "категориях штрафов")]
	[EntityPermission]
	[HistoryTrace]
	public class VatRate: PropertyChangedBase, IDomainObject, IArchivable, IValidatableObject
	{
		private int _id;
		private bool _isArchive;
		private decimal _vatRateValue;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}
		
		/// <summary>
		/// Архивная
		/// </summary>
		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Размер ставки НДС
		/// </summary>
		[Display(Name = "Размер ставки")]
		public virtual decimal VatRateValue
		{
			get => _vatRateValue;
			set => SetField(ref _vatRateValue, value);
		}

		public string VatRateStringValue => VatRateValue == 0 
			? "Без НДС" 
			: $"НДС {VatRateValue}%";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			throw new NotImplementedException();
		}
	}
}
