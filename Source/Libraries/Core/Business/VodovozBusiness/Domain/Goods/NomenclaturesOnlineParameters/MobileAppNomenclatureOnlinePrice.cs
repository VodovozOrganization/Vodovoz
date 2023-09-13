﻿using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "онлайн цены номенклатуры для мобильного приложения",
		Accusative = "онлайн цену номенклатуры для мобильного приложения",
		Nominative = "онлайн цена номенклатуры для мобильного приложения")]
	public class MobileAppNomenclatureOnlinePrice : NomenclatureOnlinePrice
	{
		public override NomenclatureOnlineParameterType Type => NomenclatureOnlineParameterType.ForMobileApp;
	}
}
