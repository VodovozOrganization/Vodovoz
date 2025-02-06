﻿using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Goods.ProductGroups;

namespace Vodovoz.ViewModels.Journals.Mappings.ProductGroups
{
	/// <summary>
	/// Маппинг группы товаров с открываемым журналом
	/// </summary>
	public class ProductGroupToJournalMapping : EntityToJournalMapping<ProductGroup>
	{
		public ProductGroupToJournalMapping()
		{
			Journal(typeof(ProductGroupsJournalViewModel));
		}
	}
}
