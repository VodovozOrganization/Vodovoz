using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum MobileCatalog
	{
		[Display(Name = "Не публиковать")]
		None,
		[Display(Name = "Вода")]
		Water,
		[Display(Name = "Товары.Оборудование")]
		Goods_Equipment,
		[Display(Name = "Товары.Чай, Кофе, Посуда")]
		Goods_CoffeeTea,
		[Display(Name = "Товары.Аксессуары")]
		Goods_Accessory
	}

	public class MobileCatalogStringType : NHibernate.Type.EnumStringType
	{
		public MobileCatalogStringType() : base(typeof(MobileCatalog))
		{
		}
	}

}
