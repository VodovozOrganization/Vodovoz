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
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			InstanceGoodsAccountingOperation instanceOperationAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			InventoryInstancesStockJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => instanceAlias)
				.JoinAlias(ini => ini.Nomenclature, () => nomenclatureAlias);

			IProjection balanceProjection = null;
			
			if(_filterViewModel.Warehouse != null)
			{
				query.JoinEntityAlias(() => warehouseInstanceOperationAlias,
					() => warehouseInstanceOperationAlias.InventoryNomenclatureInstance.Id == instanceAlias.Id);

				balanceProjection = Projections.Sum(() => warehouseInstanceOperationAlias.Amount);
			}
			else if(_filterViewModel.EmployeeStorage != null)
			{
				query.JoinEntityAlias(() => employeeInstanceOperationAlias,
					() => employeeInstanceOperationAlias.InventoryNomenclatureInstance.Id == instanceAlias.Id);
				
				balanceProjection = Projections.Sum(() => employeeInstanceOperationAlias.Amount);
			}
			else if(_filterViewModel.CarStorage != null)
			{
				query.JoinEntityAlias(() => carInstanceOperationAlias,
					() => carInstanceOperationAlias.InventoryNomenclatureInstance.Id == instanceAlias.Id);
				
				balanceProjection = Projections.Sum(() => carInstanceOperationAlias.Amount);
			}
			else
			{
				query.JoinEntityAlias(() => instanceOperationAlias,
					() => instanceOperationAlias.InventoryNomenclatureInstance.Id == instanceAlias.Id);
				
				balanceProjection = Projections.Sum(() => instanceOperationAlias.Amount);
			}
			
			query.Where(GetSearchCriterion(
				() => instanceAlias.Id,
				() => nomenclatureAlias.Name));

			#region filter

			if(_filterViewModel.Warehouse != null)
			{
				query.Where(() => warehouseInstanceOperationAlias.Warehouse.Id == _filterViewModel.Warehouse.Id);
			}
			
			if(_filterViewModel.EmployeeStorage != null)
			{
				query.Where(() => employeeInstanceOperationAlias.Employee.Id == _filterViewModel.EmployeeStorage.Id);
			}
			
			if(_filterViewModel.CarStorage != null)
			{
				query.Where(() => carInstanceOperationAlias.Car.Id == _filterViewModel.CarStorage.Id);
			}

			if(_filterViewModel.Nomenclature != null)
			{
				query.Where(() => nomenclatureAlias.Id == _filterViewModel.Nomenclature.Id);
			}

			if(!string.IsNullOrWhiteSpace(_filterViewModel.InventoryNumber))
			{
				query.Where(Restrictions.Like(
					Projections.Property(() => instanceAlias.InventoryNumber),
					_filterViewModel.InventoryNumber,
					MatchMode.Anywhere));
			}

			#endregion

			query.SelectList(list => list
				.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
				.Select(() => instanceAlias.IsUsed).WithAlias(() => resultAlias.IsUsed)
				.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
				.Select(balanceProjection).WithAlias(() => resultAlias.Balance));

			if(_filterViewModel.Warehouse != null || _filterViewModel.EmployeeStorage != null || _filterViewModel.CarStorage != null)
			{
				query.Where(Restrictions.Gt(balanceProjection, 0));
			}
			else
			{
				query.Where(Restrictions.Not(Restrictions.Eq(balanceProjection, 0)));
			}

			return query.TransformUsing(Transformers.AliasToBean<InventoryInstancesStockJournalNode>());
		}
		
		protected override void CreateEntityDialog()
		{
			throw new InvalidOperationException("Нельзя открыть диалог создания из журнала остатков экземпляров номенклатур");
		}

		protected override void EditEntityDialog(InventoryInstancesStockJournalNode node)
		{
			throw new InvalidOperationException("Нельзя открыть диалог изменения из журнала остатков экземпляров номенклатур");
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
	}
}
