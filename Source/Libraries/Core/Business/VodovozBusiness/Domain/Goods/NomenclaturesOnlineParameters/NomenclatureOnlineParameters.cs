﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры номенклатуры для ИПЗ",
		Accusative = "параметры номенклатуры для ИПЗ",
		Nominative = "параметры номенклатуры для ИПЗ")]
	[HistoryTrace]
	public abstract class NomenclatureOnlineParameters : PropertyChangedBase, IDomainObject
	{
		private decimal? _nomenclatureOnlineDiscount;
		private NomenclatureOnlineMarker? _nomenclatureOnlineMarker;
		private NomenclatureOnlineAvailability? _nomenclatureOnlineAvailability;
		private Nomenclature _nomenclature;
		private IList<NomenclatureOnlinePrice> _nomenclatureOnlinePrices = new List<NomenclatureOnlinePrice>();

		public virtual int Id { get; set; }
		
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
		
		[Display(Name = "Онлайн скидка")]
		public virtual decimal? NomenclatureOnlineDiscount
		{
			get => _nomenclatureOnlineDiscount;
			set => SetField(ref _nomenclatureOnlineDiscount, value);
		}
		
		[Display(Name = "Онлайн цены")]
		public virtual IList<NomenclatureOnlinePrice> NomenclatureOnlinePrices
		{
			get => _nomenclatureOnlinePrices;
			set => SetField(ref _nomenclatureOnlinePrices, value);
		}
		
		[Display(Name = "Онлайн акция")]
		public virtual NomenclatureOnlineMarker? NomenclatureOnlineMarker
		{
			get => _nomenclatureOnlineMarker;
			set => SetField(ref _nomenclatureOnlineMarker, value);
		}
		
		[Display(Name = "Онлайн доступность")]
		public virtual NomenclatureOnlineAvailability? NomenclatureOnlineAvailability
		{
			get => _nomenclatureOnlineAvailability;
			set => SetField(ref _nomenclatureOnlineAvailability, value);
		}
		
		public abstract NomenclatureOnlineParameterType Type { get; }
	}
}
