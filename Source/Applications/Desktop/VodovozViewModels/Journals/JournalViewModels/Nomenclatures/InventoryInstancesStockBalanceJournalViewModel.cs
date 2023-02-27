using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures
{
	public class InventoryInstancesStockBalanceJournalViewModel
		: EntityJournalViewModelBase<InventoryNomenclatureInstance, InventoryInstanceViewModel, InventoryInstancesStockJournalNode>
	{
		private readonly ILifetimeScope _scope;
		private InventoryInstancesStockBalanceJournalFilterViewModel _filterViewModel;

		public InventoryInstancesStockBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			Action<InventoryInstancesStockBalanceJournalFilterViewModel> filterParams = null,
			IDeleteEntityService deleteEntityService = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService,
				commonServices.CurrentPermissionService)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			TabName = "Журнал остатков экземпляров номенклатур";

			CreateFilter(filterParams);
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}
		
		protected override IQueryOver<InventoryNomenclatureInstance> ItemsQuery(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			InventoryInstancesStockJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => instanceAlias)
				.JoinAlias(ini => ini.Nomenclature, () => nomenclatureAlias);
			
			query.Where(GetSearchCriterion(() => instanceAlias.InventoryNumber));

			var balanceSubQuery = QueryOver.Of<InstanceGoodsAccountingOperation>()
				.Where(igao => igao.InventoryNomenclatureInstance.Id == instanceAlias.Id)
				.Select(Projections.Sum<InstanceGoodsAccountingOperation>(igao => igao.Amount));
			
			return query.SelectList(list => list
					.Select(() => instanceAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.SelectSubQuery(balanceSubQuery).WithAlias(() => resultAlias.Balance))
				//.Where(Restrictions.Gt(Projections.SubQuery(balanceSubQuery), 0))
				.TransformUsing(Transformers.AliasToBean<InventoryInstancesStockJournalNode>());
		}
		
		private void CreateFilter(Action<InventoryInstancesStockBalanceJournalFilterViewModel> filterParams)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(DialogViewModelBase), this),
				new TypedParameter(typeof(Action<InventoryInstancesStockBalanceJournalFilterViewModel>), filterParams)
			};

			_filterViewModel = _scope.Resolve<InventoryInstancesStockBalanceJournalFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
		
		protected virtual void CreateEntityDialog()
		{
			throw new InvalidOperationException("Нельзя открыть диалог создания из журнала остатков экземпляров номенклатур");
		}

		protected virtual void EditEntityDialog(InventoryInstancesStockJournalNode node)
		{
			throw new InvalidOperationException("Нельзя открыть диалог изменения из журнала остатков экземпляров номенклатур");
		}
	}
}
