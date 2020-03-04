using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Store;
using Vodovoz.FilterViewModels.Warehouses;

namespace Vodovoz.JournalViewModels.Warehouses
{
	public class WarehouseJournalViewModel : FilterableSingleEntityJournalViewModelBase<Warehouse, WarehouseDlg, WarehouseJournalNode, WarehouseJournalFilterViewModel>
	{
		public WarehouseJournalViewModel(WarehouseJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал складов";
			UpdateOnChanges(
				typeof(Warehouse)
			);
		}

		protected override Func<IUnitOfWork, IQueryOver<Warehouse>> ItemsSourceQueryFunction => (uow) => {
			WarehouseJournalNode resultAlias = null;
			Warehouse warehouseAlias = null;

			var query = uow.Session.QueryOver<Warehouse>(() => warehouseAlias);

			if(FilterViewModel != null && FilterViewModel.ResctrictArchive) {
				query.Where(c => !c.IsArchive);
			}

			query.Where(GetSearchCriterion(
				() => warehouseAlias.Id,
						() => warehouseAlias.Name
			));

			var result = query.SelectList(list => list
			.Select(c => c.Id).WithAlias(() => resultAlias.Id)
			.Select(c => c.Name).WithAlias(() => resultAlias.Title)
			.Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.TransformUsing(Transformers.AliasToBean<WarehouseJournalNode>());

			return result;
		};


		protected override Func<WarehouseDlg> CreateDialogFunction => () => new WarehouseDlg();

		protected override Func<WarehouseJournalNode, WarehouseDlg> OpenDialogFunction => (node) => new WarehouseDlg(node.Id);
	}

	public class WarehouseJournalNode : JournalEntityNodeBase<Warehouse>
	{
		public bool IsArchive { get; set; }
		public string IsArchiveString => IsArchive ? "Да" : "";
	}
}
