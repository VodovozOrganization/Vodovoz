using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Permissions;

namespace Vodovoz.Core.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "версия ставки НДС",
		NominativePlural = "версии ставок НДС")]
	[HistoryTrace]
	public class VatRateVersion : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _startDate;
		private DateTime? _endDate;
		private VatRate _vatRate;
		private OrganizationEntity _organization;
		private NomenclatureEntity _nomenclature;
		private int _id;
		
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
		/// Дата начала версии
		/// </summary>
		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		/// <summary>
		/// Дата окончания версии
		/// </summary>
		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		
		/// <summary>
		/// Ставка НДС
		/// </summary>
		[Display(Name = "Ставка НДС")]
		public virtual VatRate VatRate
		{
			get => _vatRate;
			set => SetField(ref _vatRate, value);
		}
		
		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}
		
		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(VatRate == null)
			{
				yield return new ValidationResult("Ставка НДС не выбрана", new[] { nameof(VatRate) });
			}
		}
	}
}
