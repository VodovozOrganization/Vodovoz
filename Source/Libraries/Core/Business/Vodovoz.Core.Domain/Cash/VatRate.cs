using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.Cash
{
	/// <summary>
	/// Ставки НДС
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Accusative = "ставку НДС",
		AccusativePlural = "ставки НДС",
		Genitive = "ставки НДС",
		GenitivePlural = "ставок НДС",
		Nominative = "ставка НДС",
		NominativePlural = "ставки НДС",
		Prepositional = "ставке НДС",
		PrepositionalPlural = "ставках НДС")]
	[EntityPermission]
	[HistoryTrace]
	public class VatRate: PropertyChangedBase, IDomainObject, IArchivable, IValidatableObject
	{
		private readonly string _noVatRateFor1C = "БезНДС";
		
		private int _id;
		private bool _isArchive;
		private decimal _vatRateValue;
		private Vat1cType _vat1cTypeValue;

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
		
		/// <summary>
		/// Тип 1с ставки НДС
		/// </summary>
		[Display(Name = "Тип 1с ставки НДС")]
		public virtual Vat1cType Vat1cTypeValue
		{
			get => _vat1cTypeValue;
			set => SetField(ref _vat1cTypeValue, value);
		}

		public virtual string Name  => VatRateValue == 0 ? "Без НДС" : VatRateValue + "%";
		public virtual decimal VatNumericValue => VatRateValue / 100;
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(VatRateValue < 0)
			{
				yield return new ValidationResult(
					"Ставка НДС не может быть отрицательной", new[] { nameof(VatRateValue) });
			}
		}

		public virtual FiscalVat ToFiscalVat()
		{
			switch(VatRateValue)
			{
				case 0:
					return FiscalVat.VatFree;
				case 10:
					return FiscalVat.Vat10;
				case 18:
					throw new InvalidOperationException("В чеках нет возможности устанавливать НДС 18%. Скорее всего ошибка в заполнении карточки товара");
				case 20:
					return FiscalVat.Vat20;
				default:
					throw new InvalidOperationException("Нет соответствия между НДС товара и FiscalVat, проверьте карточку товара");
			}
		}
		
		public virtual string GetValue1c() => VatRateValue == 0 ? _noVatRateFor1C : "НДС" + (int)VatRateValue;

		public virtual string GetValue1cComplexAutomation() => VatRateValue == 0 ? _noVatRateFor1C : (int)VatRateValue + "%";

		public virtual string GetValue1cType() => Vat1cTypeValue.GetAttribute<Value1cType>().Value;
	}
}
