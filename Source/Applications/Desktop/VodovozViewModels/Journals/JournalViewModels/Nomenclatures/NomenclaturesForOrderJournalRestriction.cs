using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using QS.Services;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Infrastructure;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures
{
	public class NomenclaturesForOrderJournalRestriction : IAdditionalJournalRestriction<Nomenclature>
	{
		readonly int userId;
		readonly ICommonServices commonServices;

		public NomenclaturesForOrderJournalRestriction(ICommonServices commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			userId = this.commonServices.UserService.CurrentUserId;
			CreateRestrictions();
		}

		public IEnumerable<Expression<Func<Nomenclature, bool>>> ExternalRestrictions { get; private set; }

		void CreateRestrictions()
		{
			var canAddSpares = commonServices.PermissionService.ValidateUserPresetPermission("can_add_spares_to_order", userId);
			var canAddBottles = commonServices.PermissionService.ValidateUserPresetPermission("can_add_bottles_to_order", userId);
			var canAddMaterials = commonServices.PermissionService.ValidateUserPresetPermission("can_add_materials_to_order", userId);
			var canAddEquipmentNotForSale = commonServices.PermissionService.ValidateUserPresetPermission("can_add_equipment_not_for_sale_to_order", userId);

			List<Expression<Func<Nomenclature, bool>>> restr = new List<Expression<Func<Nomenclature, bool>>>();

			if(!canAddSpares)
				restr.Add(n => !(n.Category == NomenclatureCategory.spare_parts && n.SaleCategory == SaleCategory.notForSale));
			if(!canAddBottles)
				restr.Add(n => !(n.Category == NomenclatureCategory.bottle && n.SaleCategory == SaleCategory.notForSale));
			if(!canAddMaterials)
				restr.Add(n => !(n.Category == NomenclatureCategory.material && n.SaleCategory == SaleCategory.notForSale));
			if(!canAddEquipmentNotForSale)
				restr.Add(n => !(n.Category == NomenclatureCategory.equipment && n.SaleCategory == SaleCategory.notForSale));

			ExternalRestrictions = restr;
		}
	}
}
