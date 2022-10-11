using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.Journals.JournalViewModels
{
	public class NomenclatureBalanceByStockJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<Warehouse, WarehouseViewModel, NomenclatureBalanceByStockJournalNode, NomenclatureBalanceByStockFilterViewModel>
	{
		public NomenclatureBalanceByStockJournalViewModel(
			NomenclatureBalanceByStockFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			UpdateOnChanges(
				typeof(Warehouse),
				typeof(Nomenclature),
				typeof(WarehouseMovementOperation)
			);
		}

		protected override Func<IUnitOfWork, IQueryOver<Warehouse>> ItemsSourceQueryFunction => (uow) =>
		{
			Warehouse warehouseAlias = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation incomeAlias = null;
			WarehouseMovementOperation writeoffAlias = null;
			NomenclatureBalanceByStockJournalNode warehouseNodeAlias = null;

			var query = uow.Session.QueryOver(() => warehouseAlias).WhereNot(w => w.IsArchive);

			var incomeSubQuery = QueryOver.Of(() => incomeAlias)
				.Where(x => x.IncomingWarehouse.Id == warehouseAlias.Id);
			var writeoffSubQuery = QueryOver.Of(() => writeoffAlias)
				.Where(x => x.WriteoffWarehouse.Id == warehouseAlias.Id);

			if(FilterViewModel?.Nomenclature != null)
			{
				incomeSubQuery.Where(() => incomeAlias.Nomenclature.Id == FilterViewModel.Nomenclature.Id);
				writeoffSubQuery.Where(() => writeoffAlias.Nomenclature.Id == FilterViewModel.Nomenclature.Id);
			}

			if(FilterViewModel?.Warehouse != null)
			{
				query.Where(() => warehouseAlias.Id == FilterViewModel.Warehouse.Id);
			}

			incomeSubQuery.Select(Projections.Sum(Projections.Property(() => incomeAlias.Amount)));
			writeoffSubQuery.Select(Projections.Sum(Projections.Property(() => writeoffAlias.Amount)));

			query.Where(GetSearchCriterion(
				() => warehouseAlias.Id,
				() => warehouseAlias.Name
			));

			var result = query.SelectList(list => list
					.Select(w => w.Id).WithAlias(() => warehouseNodeAlias.Id)
					.Select(w => w.Name).WithAlias(() => warehouseNodeAlias.WarehouseName)
					.SelectSubQuery(writeoffSubQuery).WithAlias(() => warehouseNodeAlias.Removed)
					.SelectSubQuery(incomeSubQuery).WithAlias(() => warehouseNodeAlias.Added))
				.OrderBy(w => w.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureBalanceByStockJournalNode>());
			return result;
		};

		protected override Func<WarehouseViewModel> CreateDialogFunction => () =>
			throw new NotSupportedException("Не поддерживается создание склада из журнала");

		protected override Func<NomenclatureBalanceByStockJournalNode, WarehouseViewModel> OpenDialogFunction => (node) =>
			throw new NotSupportedException("Не поддерживается открытие склада из журнала");

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any() && selected.All(x => ((NomenclatureBalanceByStockJournalNode)x).NomenclatureAmount > 0),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) =>
				{
					if(selected.All(x => ((NomenclatureBalanceByStockJournalNode)x).NomenclatureAmount > 0))
					{
						OnItemsSelected(selected);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple)
			{
				RowActivatedAction = selectAction;
			}

			NodeActionsList.Add(selectAction);
		}
	}
}
