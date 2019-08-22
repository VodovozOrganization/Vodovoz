using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Config;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalNodes;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class NomenclaturesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Nomenclature, NomenclatureDlg, NomenclatureJournalNode, NomenclatureFilterViewModel>
	{
		readonly ICommonServices commonServices;
		readonly int currentUserId;

		public NomenclaturesJournalViewModel(
			NomenclatureFilterViewModel filterViewModel,
			IEntityConfigurationProvider entityConfigurationProvider,
			ICommonServices commonServices
		) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал ТМЦ";
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.currentUserId = commonServices.UserService.CurrentUserId;
			SetOrder<SelfDeliveryJournalNode>(x => x.Name);
			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(WarehouseMovementOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		public int[] ExcludingNomenclatureIds { get; set; }

		protected override Func<IQueryOver<Nomenclature>> ItemsSourceQueryFunction => () => {
			var canAddSpares = commonServices.PermissionService.ValidateUserPresetPermission("can_add_spares_to_order", currentUserId);
			var canAddBottles = commonServices.PermissionService.ValidateUserPresetPermission("can_add_bottles_to_order", currentUserId);
			var canAddMaterials = commonServices.PermissionService.ValidateUserPresetPermission("can_add_materials_to_order", currentUserId);
			var canAddEquipmentNotForSale = commonServices.PermissionService.ValidateUserPresetPermission("can_add_equipment_not_for_sale_to_order", currentUserId);

			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			NomenclatureJournalNode resultAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;

			var subqueryAdded = QueryOver.Of(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemoved = QueryOver.Of(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryReserved = QueryOver.Of(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where(() => orderItemsAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(() => nomenclatureAlias.DoNotReserve == false)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted
					   || orderAlias.OrderStatus == OrderStatus.InTravelList
					   || orderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => orderItemsAlias.Count));

			var itemsQuery = UoW.Session.QueryOver(() => nomenclatureAlias)
								.Where(() => !nomenclatureAlias.IsArchive)
								;

			if(ExcludingNomenclatureIds != null && ExcludingNomenclatureIds.Any())
				itemsQuery.WhereNot(() => nomenclatureAlias.Id.IsIn(ExcludingNomenclatureIds));

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id
				)
			);

			if(!FilterViewModel.RestrictDilers)
				itemsQuery.Where(() => !nomenclatureAlias.IsDiler);
			if(FilterViewModel.SelectedCategories.Contains(NomenclatureCategory.water))
				itemsQuery.Where(() => nomenclatureAlias.IsDisposableTare == FilterViewModel.RestrictDisposbleTare);

			itemsQuery.Where(n => n.Category.IsIn(FilterViewModel.SelectedCategories));

			if(FilterViewModel.SelectedCategories.Count() == 1 && Nomenclature.GetCategoriesWithSaleCategory().Contains(FilterViewModel.SelectedCategories.FirstOrDefault()))
				itemsQuery.Where(n => n.SaleCategory.IsIn(FilterViewModel.SelectedSubCategories));
			if(!canAddSpares)
				itemsQuery.Where(n => !(n.Category == NomenclatureCategory.spare_parts && n.SaleCategory == SaleCategory.notForSale));
			if(!canAddBottles)
				itemsQuery.Where(n => !(n.Category == NomenclatureCategory.bottle && n.SaleCategory == SaleCategory.notForSale));
			if(!canAddMaterials)
				itemsQuery.Where(n => !(n.Category == NomenclatureCategory.material && n.SaleCategory == SaleCategory.notForSale));
			if(!canAddEquipmentNotForSale)
				itemsQuery.Where(n => !(n.Category == NomenclatureCategory.equipment && n.SaleCategory == SaleCategory.notForSale));

			itemsQuery.Left.JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias)
				.Where(() => !nomenclatureAlias.IsSerial)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.SelectSubQuery(subqueryAdded).WithAlias(() => resultAlias.Added)
					.SelectSubQuery(subqueryRemoved).WithAlias(() => resultAlias.Removed)
					.SelectSubQuery(subqueryReserved).WithAlias(() => resultAlias.Reserved)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>())
				;

			return itemsQuery;
		};

		protected override Func<NomenclatureDlg> CreateDialogFunction => () => new NomenclatureDlg();

		protected override Func<NomenclatureJournalNode, NomenclatureDlg> OpenDialogFunction => node => new NomenclatureDlg(node.Id);
	}
}
