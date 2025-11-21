using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры промонабора для ИПЗ",
		Accusative = "параметры промонабора для ИПЗ",
		Nominative = "параметры промонабора для ИПЗ")]
	[HistoryTrace]
	public abstract class PromotionalSetOnlineParameters : PropertyChangedBase, IDomainObject
	{
		private GoodsOnlineAvailability? _promotionalSetOnlineAvailability;
		private PromotionalSet _promotionalSet;
		
		public virtual int Id { get; set; }
		
		[Display(Name = "Промонабор")]
		public virtual PromotionalSet PromotionalSet
		{
			get => _promotionalSet;
			set => SetField(ref _promotionalSet, value);
		}

		[Display(Name = "Онлайн доступность")]
		public virtual GoodsOnlineAvailability? PromotionalSetOnlineAvailability
		{
			get => _promotionalSetOnlineAvailability;
			set => SetField(ref _promotionalSetOnlineAvailability, value);
		}
		
		public abstract GoodsOnlineParameterType Type { get; }
	}
}
