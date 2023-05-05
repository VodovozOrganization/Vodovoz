using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Dialogs.Goods;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	//TODO проверить работу запроса
	public class NomenclatureStockBalanceJournalViewModel :
		SingleEntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureStockJournalNode>
	{
		private readonly ILifetimeScope _scope;
		private NomenclatureStockFilterViewModel _filterViewModel;

		public NomenclatureStockBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope scope,
			Action<NomenclatureStockFilterViewModel> filterParams = null) : base(unitOfWorkFactory, commonServices)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			TabName = "Складские остатки";

			CreateFilter(filterParams);
			
			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(GoodsAccountingOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		private void CreateFilter(Action<NomenclatureStockFilterViewModel> filterParams)
		{
			_filterViewModel = _scope.Resolve<NomenclatureStockFilterViewModel>(new TypedParameter(typeof(DialogViewModelBase), this));

			if(filterParams != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterParams);
			}

			Filter = _filterViewModel;
		}

		protected override List<IJournalAction> NodeActionsList { get; set; }

		protected override void CreateNodeActions()
		{
			NodeActionsList = new List<IJournalAction>();
			CreateDefaultSelectAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<Nomenclature>> ItemsSourceQueryFunction => uow =>
		{
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			NomenclatureStockJournalNode resultAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeBulkGoodsAccountingOperation employeeBulkOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarBulkGoodsAccountingOperation carBulkOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			GoodsAccountingOperation operationAlias = null;

			IProjection balanceProjection = null;

			var queryStock = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => measurementUnitsAlias);

			if(_filterViewModel.Warehouse != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.JoinEntityAlias(() => warehouseBulkOperationAlias,
						() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id
							&& warehouseBulkOperationAlias.Warehouse.Id == _filterViewModel.Warehouse.Id,
						JoinType.LeftOuterJoin);
				
					balanceProjection = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => warehouseBulkOperationAlias,
							() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id
								&& warehouseBulkOperationAlias.Warehouse.Id == _filterViewModel.Warehouse.Id,
							JoinType.LeftOuterJoin)
						.JoinEntityAlias(() => warehouseInstanceOperationAlias,
							() => nomenclatureAlias.Id == warehouseInstanceOperationAlias.Nomenclature.Id
								&& warehouseInstanceOperationAlias.Warehouse.Id == _filterViewModel.Warehouse.Id,
							JoinType.LeftOuterJoin);
				
					balanceProjection = Projections.Conditional(
						Restrictions.Where(() => warehouseBulkOperationAlias == null),
						Projections.Sum(() => warehouseInstanceOperationAlias.Amount),
						Projections.Sum(() => warehouseBulkOperationAlias.Amount));
				}
			}
			else if(_filterViewModel.EmployeeStorage != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.JoinEntityAlias(() => employeeInstanceOperationAlias,
						() => nomenclatureAlias.Id == employeeInstanceOperationAlias.Nomenclature.Id
							&& employeeInstanceOperationAlias.Employee.Id == _filterViewModel.EmployeeStorage.Id,
						JoinType.LeftOuterJoin);

					balanceProjection = Projections.Sum(() => employeeInstanceOperationAlias.Amount);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => employeeBulkOperationAlias,
							() => nomenclatureAlias.Id == employeeBulkOperationAlias.Nomenclature.Id
								&& employeeBulkOperationAlias.Employee.Id == _filterViewModel.EmployeeStorage.Id,
							JoinType.LeftOuterJoin)
						.JoinEntityAlias(() => employeeInstanceOperationAlias,
							() => nomenclatureAlias.Id == employeeInstanceOperationAlias.Nomenclature.Id
								&& employeeInstanceOperationAlias.Employee.Id == _filterViewModel.EmployeeStorage.Id,
							JoinType.LeftOuterJoin);
				
					balanceProjection = Projections.Conditional(
						Restrictions.Where(() => employeeBulkOperationAlias == null),
						Projections.Sum(() => employeeInstanceOperationAlias.Amount),
						Projections.Sum(() => employeeBulkOperationAlias.Amount));
				}
			}
			else if(_filterViewModel.CarStorage != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock
						.JoinEntityAlias(() => carBulkOperationAlias,
							() => nomenclatureAlias.Id == carBulkOperationAlias.Nomenclature.Id
								&& carBulkOperationAlias.Car.Id == _filterViewModel.CarStorage.Id,
							JoinType.LeftOuterJoin);
				
					balanceProjection = Projections.Sum(() => carBulkOperationAlias.Amount);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => carBulkOperationAlias,
							() => nomenclatureAlias.Id == carBulkOperationAlias.Nomenclature.Id
								&& carBulkOperationAlias.Car.Id == _filterViewModel.CarStorage.Id,
							JoinType.LeftOuterJoin)
						.JoinEntityAlias(() => carInstanceOperationAlias,
							() => nomenclatureAlias.Id == carInstanceOperationAlias.Nomenclature.Id
								&& carInstanceOperationAlias.Car.Id == _filterViewModel.CarStorage.Id,
							JoinType.LeftOuterJoin);
				
					balanceProjection = Projections.Conditional(
						Restrictions.Where(() => carBulkOperationAlias == null),
						Projections.Sum(() => carInstanceOperationAlias.Amount),
						Projections.Sum(() => carBulkOperationAlias.Amount));
				}
			}
			else
			{
				queryStock.JoinEntityAlias(() => operationAlias,
					() => nomenclatureAlias.Id == operationAlias.Nomenclature.Id,
					JoinType.LeftOuterJoin);
				
				balanceProjection = Projections.Sum(() => operationAlias.Amount);
			}

			if(!_filterViewModel.ShowArchive)
			{
				queryStock.Where(() => nomenclatureAlias.IsArchive == false);
			}

			if(_filterViewModel.ExcludedNomenclatureIds != null && _filterViewModel.ExcludedNomenclatureIds.Any())
			{
				queryStock.Where(
					Restrictions.Not(
						Restrictions.In(
							Projections.Property(() => nomenclatureAlias.Id), 
							_filterViewModel.ExcludedNomenclatureIds.ToArray()
						)
					)
				);
			}
			
			if(_filterViewModel.Warehouse != null  && SelectionMode != JournalSelectionMode.None)
			{
				queryStock.Where(Restrictions.Gt(balanceProjection, 0));
			}
			else if(_filterViewModel.EmployeeStorage != null && SelectionMode != JournalSelectionMode.None)
			{
				queryStock.Where(Restrictions.Gt(balanceProjection, 0));
			}
			else if(_filterViewModel.CarStorage != null && SelectionMode != JournalSelectionMode.None)
			{
				queryStock.Where(Restrictions.Gt(balanceProjection, 0));
			}
			else
			{
				queryStock.Where(Restrictions.Not(Restrictions.Eq(balanceProjection,0)));
			}

			queryStock.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id
				)
			);

			queryStock.OrderByAlias(() => nomenclatureAlias.Name);

			queryStock
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => nomenclatureAlias.MinStockCount).WithAlias(() => resultAlias.MinNomenclatureAmount)
					.Select(() => nomenclatureAlias.HasInventoryAccounting).WithAlias(() => resultAlias.HasInventoryAccounting)
					.Select(() => measurementUnitsAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => measurementUnitsAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.Select(balanceProjection).WithAlias(() => resultAlias.StockAmount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockJournalNode>());

			return queryStock;
		};

		protected override Func<NomenclatureViewModel> CreateDialogFunction =>
			() => throw new InvalidOperationException("Нельзя создавать номенклатуры из данного журнала");

		protected override Func<NomenclatureStockJournalNode, NomenclatureViewModel> OpenDialogFunction =>
			node => throw new InvalidOperationException("Нельзя изменять номенклатуры из данного журнала");
	}
}
