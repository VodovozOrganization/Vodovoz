using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QS.BusinessCommon.Domain;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using QS.Project.Services;

namespace Vodovoz.ViewModel
{
	[Obsolete("Использовать представление MVVM")]
	public class NomenclatureForSaleVM : RepresentationModelWithoutEntityBase<NomenclatureForSaleVMNode>
	{

		public NomenclatureRepFilter Filter {
			get => RepresentationFilter as NomenclatureRepFilter;
			set => RepresentationFilter = value as IRepresentationFilter;
		}

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			NomenclatureForSaleVMNode resultAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;

			var subqueryAdded = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemoved = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryReserved = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where(() => orderItemsAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(() => nomenclatureAlias.DoNotReserve == false)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted
					   || orderAlias.OrderStatus == OrderStatus.InTravelList
					   || orderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => orderItemsAlias.Count));

			var itemsQuery = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
						   .Where(() => !nomenclatureAlias.IsArchive);

			if(!Filter.ShowDilers)
				itemsQuery.Where(() => !nomenclatureAlias.IsDiler);
			if(Filter.SelectedCategories.Contains(NomenclatureCategory.water)) {
				itemsQuery.Where(() => nomenclatureAlias.IsDisposableTare == Filter.OnlyDisposableTare);
			}
			itemsQuery.Where(n => n.Category.IsIn(Filter.SelectedCategories));

			if(Filter.SelectedCategories.Count() == 1 && Nomenclature.GetCategoriesWithSaleCategory().Contains(Filter.SelectedCategories.FirstOrDefault()))
				itemsQuery.Where(n => n.SaleCategory.IsIn(Filter.SelectedSubCategories));

			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_add_spares_to_order"))
				itemsQuery.Where(n => !(n.Category == NomenclatureCategory.spare_parts && n.SaleCategory == SaleCategory.notForSale));
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_add_bottles_to_order"))
				itemsQuery.Where(n => !(n.Category == NomenclatureCategory.bottle && n.SaleCategory == SaleCategory.notForSale));
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_add_materials_to_order"))
				itemsQuery.Where(n => !(n.Category == NomenclatureCategory.material && n.SaleCategory == SaleCategory.notForSale));
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_add_equipment_not_for_sale_to_order"))
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
				.TransformUsing(Transformers.AliasToBean<NomenclatureForSaleVMNode>());
			
			var items = itemsQuery.List<NomenclatureForSaleVMNode>();

			List<NomenclatureForSaleVMNode> forSale = new List<NomenclatureForSaleVMNode>();
			forSale.AddRange(items);
			forSale.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
			SetItemsSource(forSale);
		}

		static Gdk.Color colorBlack = new Gdk.Color(0, 0, 0);
		static Gdk.Color colorRed = new Gdk.Color(0xff, 0, 0);

		IColumnsConfig columnsConfig = FluentColumnsConfig<NomenclatureForSaleVMNode>.Create()
			.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Номенклатура").SetDataProperty(node => node.Name)
			.AddColumn("Категория").SetDataProperty(node => node.Category.GetEnumTitle())
			.AddColumn("Кол-во").AddTextRenderer(node => node.InStockText)
			.AddColumn("Зарезервировано").AddTextRenderer(node => node.ReservedText)
			.AddColumn("Доступно").AddTextRenderer(node => node.AvailableText)
			.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? colorBlack : colorRed)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#endregion

		public NomenclatureForSaleVM()
			: this(UnitOfWorkFactory.CreateWithoutRoot())
		{ }

		public NomenclatureForSaleVM(IUnitOfWork uow) : base(typeof(Nomenclature), typeof(WarehouseMovementOperation))
		{
			this.UoW = uow;
		}

		public NomenclatureForSaleVM(NomenclatureRepFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		#region implemented abstract members of RepresentationModelWithoutEntityBase

		protected override bool NeedUpdateFunc(object updatedSubject) => true;

		#endregion
	}

	public class NomenclatureForSaleVMNode
	{
		[UseForSearch]
		public int Id { get; set; }

		[UseForSearch]
		public string Name { get; set; }
		public NomenclatureCategory Category { get; set; }
		public decimal InStock => Added - Removed;
		public decimal? Reserved { get; set; }
		public decimal Available => InStock - Reserved.GetValueOrDefault();
		public decimal Added { get; set; }
		public decimal Removed { get; set; }
		public string UnitName { get; set; }
		public short UnitDigits { get; set; }
		public bool IsEquipmentWithSerial { get; set; }
		private string Format(decimal value) => string.Format("{0:F" + UnitDigits + "} {1}", value, UnitName);

		private bool UsedStock => Nomenclature.GetCategoriesForGoods().Contains(Category);

		public string InStockText => UsedStock ? Format(InStock) : string.Empty;
		public string ReservedText => UsedStock && Reserved.HasValue ? Format(Reserved.Value) : string.Empty;
		public string AvailableText => UsedStock ? Format(Available) : string.Empty;
	}
}