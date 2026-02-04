using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Controllers
{
	public class DiscountController : IDiscountController
	{
		/// <summary>
		/// Проверка применимости скидки к номенклатуре, т.е. если выбранное основание скидки содержит номенклатуру,
		/// которая указана в основании скидки, либо основание содержит категорию номенклатуры, либо основание содержит товарную группу
		/// с такой номенклатурой, то возвращаем true
		/// Иначе false
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="nomenclature">Номенклатура</param>
		/// <returns>true/false</returns>
		public bool IsApplicableDiscount(DiscountReason reason, Nomenclature nomenclature)
		{
			if(reason == null)
			{
				throw new ArgumentNullException(nameof(reason));
			}

			return ContainsNomenclature(nomenclature.Id, reason.Nomenclatures)
				|| ContainsNomenclatureCategory(nomenclature.Category, reason.NomenclatureCategories)
				|| ContainsProductGroup(nomenclature.ProductGroup, reason.ProductGroups);
		}

		/// <summary>
		/// Содержит ли основание скидки соответствующую категорию номенклатуры 
		/// </summary>
		/// <param name="nomenclatureCategory">Категория номенклатуры</param>
		/// <param name="discountNomenclatureCategories">Список категорий номенклатур у основания скидки</param>
		/// <returns>true/false</returns>
		private bool ContainsNomenclatureCategory(
			NomenclatureCategory nomenclatureCategory, IEnumerable<DiscountReasonNomenclatureCategory> discountNomenclatureCategories)
		{
			return discountNomenclatureCategories.Any(x => x.NomenclatureCategory == nomenclatureCategory);
		}
		
		/// <summary>
		/// Содержит ли основание скидки ссылку на указанную номенклатуру
		/// </summary>
		/// <param name="nomenclatureId">Id номенклатуры</param>
		/// <param name="discountNomenclatures">Список номенклатур основания скидки</param>
		/// <returns>ture/false</returns>
		private bool ContainsNomenclature(int nomenclatureId, IEnumerable<Nomenclature> discountNomenclatures) =>
			discountNomenclatures.Any(n => n.Id == nomenclatureId);

		/// <summary>
		/// Содержит ли основание скидки в списке товарную группу строки заказа 
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroups">Товарные группы основания скидки</param>
		/// <returns>true/false</returns>
		private bool ContainsProductGroup(ProductGroup itemProductGroup, IEnumerable<ProductGroup> discountProductGroups) =>
			itemProductGroup != null
			&& discountProductGroups.Any(discountProductGroup => ContainsProductGroup(itemProductGroup, discountProductGroup));
		
		/// <summary>
		/// Проверяет соответствие товарных групп у основания скидки и строки заказа,
		/// с обходом всех ее родительских групп
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroup">Товарная группа основания скидки</param>
		/// <returns>true/false</returns>
		private bool ContainsProductGroup(ProductGroup itemProductGroup, ProductGroup discountProductGroup)
		{
			while(true)
			{
				if(itemProductGroup == discountProductGroup)
				{
					return true;
				}

				if(itemProductGroup.Parent != null)
				{
					itemProductGroup = itemProductGroup.Parent;
					continue;
				}

				return false;
			}
		}
	}
}
