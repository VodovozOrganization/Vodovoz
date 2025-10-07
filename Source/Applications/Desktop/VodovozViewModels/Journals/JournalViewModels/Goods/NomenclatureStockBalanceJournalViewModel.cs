using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
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
	public class NomenclatureStockBalanceJournalViewModel :
		SingleEntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureStockJournalNode>
	{
		private readonly ILifetimeScope _scope;
		private NomenclatureStockFilterViewModel _filterViewModel;
		private string _footerInfo;

		public NomenclatureStockBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope scope,
			Action<NomenclatureStockFilterViewModel> filterParams = null) : base(unitOfWorkFactory, commonServices)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			TabName = "Складские остатки";

			CreateFilter(filterParams);
			
			DataLoader.LoadingStateChanged += OnDataLoaderLoadingStateChanged;
			(DataLoader as ThreadDataLoader<NomenclatureStockJournalNode>).AddQuery(NomenclatureHasInventoryAccountingQueryFunc);
			
			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(GoodsAccountingOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		public override string FooterInfo
		{
			get => _footerInfo;
			set => SetField(ref _footerInfo, value);
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
			EmployeeBulkGoodsAccountingOperation employeeBulkOperationAlias = null;
			CarBulkGoodsAccountingOperation carBulkOperationAlias = null;
			BulkGoodsAccountingOperation bulkOperationAlias = null;

			IProjection bulkBalanceProjection = null;

			var queryStock = uow.Session.QueryOver(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => measurementUnitsAlias)
				.Where(() => !nomenclatureAlias.HasInventoryAccounting);

			if(_filterViewModel.Warehouse != null)
			{
				queryStock.JoinEntityAlias(() => warehouseBulkOperationAlias,
						() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id
							&& warehouseBulkOperationAlias.Warehouse.Id == _filterViewModel.Warehouse.Id,
						JoinType.LeftOuterJoin);
				
				bulkBalanceProjection = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
			}
			else if(_filterViewModel.EmployeeStorage != null)
			{
				queryStock
					.JoinEntityAlias(() => employeeBulkOperationAlias,
						() => nomenclatureAlias.Id == employeeBulkOperationAlias.Nomenclature.Id
							&& employeeBulkOperationAlias.Employee.Id == _filterViewModel.EmployeeStorage.Id,
						JoinType.LeftOuterJoin);
			
				bulkBalanceProjection = Projections.Sum(() => employeeBulkOperationAlias.Amount);
			}
			else if(_filterViewModel.CarStorage != null)
			{
				queryStock
					.JoinEntityAlias(() => carBulkOperationAlias,
						() => nomenclatureAlias.Id == carBulkOperationAlias.Nomenclature.Id
							&& carBulkOperationAlias.Car.Id == _filterViewModel.CarStorage.Id,
						JoinType.LeftOuterJoin);
			
				bulkBalanceProjection = Projections.Sum(() => carBulkOperationAlias.Amount);
			}
			else
			{
				queryStock
					.JoinEntityAlias(() => bulkOperationAlias,
						() => nomenclatureAlias.Id == bulkOperationAlias.Nomenclature.Id,
						JoinType.LeftOuterJoin);

				bulkBalanceProjection = Projections.Sum(() => bulkOperationAlias.Amount);
			}

			if((_filterViewModel.Warehouse != null || _filterViewModel.EmployeeStorage != null || _filterViewModel.CarStorage != null)
				&& SelectionMode != JournalSelectionMode.None)
			{
				queryStock.Where(Restrictions.Gt(bulkBalanceProjection, 0));
			}
			else
			{
				queryStock.Where(Restrictions.Not(Restrictions.Eq(bulkBalanceProjection, 0)));
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

			NomenclatureMinimumBalanceByWarehouse nomenclatureMinimumBalanceByWarehouseAlias = null;

			var minWarehouseBalanceSubquery = QueryOver.Of(() => nomenclatureMinimumBalanceByWarehouseAlias)
				.Where(() => nomenclatureMinimumBalanceByWarehouseAlias.Nomenclature.Id == nomenclatureAlias.Id);

			if(_filterViewModel.Warehouse != null)
			{
				minWarehouseBalanceSubquery.Where(() => nomenclatureMinimumBalanceByWarehouseAlias.Warehouse.Id == _filterViewModel.Warehouse.Id);
			};

			minWarehouseBalanceSubquery
			.Select(
				Projections.Conditional(
					Restrictions.IsNull(Projections.Property(() => nomenclatureMinimumBalanceByWarehouseAlias.Id)),
					Projections.Cast(NHibernateUtil.Int32, Projections.Property(() => nomenclatureAlias.MinStockCount)),
					Projections.Max(() => nomenclatureMinimumBalanceByWarehouseAlias.MinimumBalance)
				));

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
					.SelectSubQuery(minWarehouseBalanceSubquery).WithAlias(() => resultAlias.MinNomenclatureAmount)
					.Select(() => nomenclatureAlias.HasInventoryAccounting).WithAlias(() => resultAlias.HasInventoryAccounting)
					.Select(() => measurementUnitsAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => measurementUnitsAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.Select(bulkBalanceProjection).WithAlias(() => resultAlias.BulkStockAmount)
					.Select(Projections.Constant(0m)).WithAlias(() => resultAlias.InstanceStockAmount)
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockJournalNode>());

			return queryStock;
		};

		protected override Func<IUnitOfWork, int> ItemsCountFunction => uow =>
		{
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeBulkGoodsAccountingOperation employeeBulkOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarBulkGoodsAccountingOperation carBulkOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			BulkGoodsAccountingOperation bulkOperationAlias = null;
			InstanceGoodsAccountingOperation instanceOperationAlias = null;
			
			IProjection instanceBalanceProjection = null;
			IProjection bulkBalanceProjection = null;

			var queryStock = uow.Session.QueryOver(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => measurementUnitsAlias);

			if(_filterViewModel.Warehouse != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.JoinEntityAlias(() => warehouseBulkOperationAlias,
						() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id
							&& warehouseBulkOperationAlias.Warehouse.Id == _filterViewModel.Warehouse.Id,
						JoinType.LeftOuterJoin);

					bulkBalanceProjection = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Constant(0m);
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

					bulkBalanceProjection = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Sum(() => warehouseInstanceOperationAlias.Amount);
				}
			}
			else if(_filterViewModel.EmployeeStorage != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.JoinEntityAlias(() => employeeBulkOperationAlias,
						() => nomenclatureAlias.Id == employeeBulkOperationAlias.Nomenclature.Id
							&& employeeBulkOperationAlias.Employee.Id == _filterViewModel.EmployeeStorage.Id,
						JoinType.LeftOuterJoin);

					bulkBalanceProjection = Projections.Sum(() => employeeBulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Constant(0m);
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

					bulkBalanceProjection = Projections.Sum(() => employeeBulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Sum(() => employeeInstanceOperationAlias.Amount);
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

					bulkBalanceProjection = Projections.Sum(() => carBulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Constant(0m);
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

					bulkBalanceProjection = Projections.Sum(() => carBulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Sum(() => carInstanceOperationAlias.Amount);
				}
			}
			else
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.JoinEntityAlias(() => bulkOperationAlias,
						() => nomenclatureAlias.Id == bulkOperationAlias.Nomenclature.Id,
						JoinType.LeftOuterJoin);

					bulkBalanceProjection = Projections.Sum(() => bulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Constant(0m);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => bulkOperationAlias,
							() => nomenclatureAlias.Id == bulkOperationAlias.Nomenclature.Id,
							JoinType.LeftOuterJoin)
						.JoinEntityAlias(() => instanceOperationAlias,
							() => nomenclatureAlias.Id == instanceOperationAlias.Nomenclature.Id,
							JoinType.LeftOuterJoin);

					bulkBalanceProjection = Projections.Sum(() => bulkOperationAlias.Amount);
					instanceBalanceProjection = Projections.Sum(() => instanceOperationAlias.Amount);
				}
			}

			if((_filterViewModel.Warehouse != null || _filterViewModel.EmployeeStorage != null || _filterViewModel.CarStorage != null)
				&& SelectionMode != JournalSelectionMode.None)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(Restrictions.Gt(bulkBalanceProjection, 0));
				}
				else
				{
					queryStock.Where(CustomRestrictions.OrHaving(
						Restrictions.Gt(bulkBalanceProjection, 0),
						Restrictions.Gt(instanceBalanceProjection, 0)));
				}
			}
			else
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(Restrictions.Not(Restrictions.Eq(bulkBalanceProjection, 0)));
				}
				else
				{
					queryStock.Where(
						CustomRestrictions.OrHaving(
							Restrictions.Not(Restrictions.Eq(bulkBalanceProjection, 0)),
							Restrictions.Not(Restrictions.Eq(instanceBalanceProjection, 0))));
				}
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

			queryStock.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id
				)
			);

			queryStock.OrderByAlias(() => nomenclatureAlias.Name);

			var nomenclatureIds = queryStock
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id)
				)
				.List<int>();

			return nomenclatureIds.Count;
		};

		protected override Func<NomenclatureViewModel> CreateDialogFunction =>
			() => throw new InvalidOperationException("Нельзя создавать номенклатуры из данного журнала");

		protected override Func<NomenclatureStockJournalNode, NomenclatureViewModel> OpenDialogFunction =>
			node => throw new InvalidOperationException("Нельзя изменять номенклатуры из данного журнала");
		
		private Func<IUnitOfWork, IQueryOver<Nomenclature>> NomenclatureHasInventoryAccountingQueryFunc => uow =>
		{
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			NomenclatureStockJournalNode resultAlias = null;
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			InstanceGoodsAccountingOperation instanceOperationAlias = null;

			IProjection instanceBalanceProjection = null;

			var queryStock = uow.Session.QueryOver(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => measurementUnitsAlias)
				.Where(() => nomenclatureAlias.HasInventoryAccounting);

			if(_filterViewModel.Warehouse != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(n => n.Id == 0);
					instanceBalanceProjection = Projections.Constant(0m);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => warehouseInstanceOperationAlias,
							() => nomenclatureAlias.Id == warehouseInstanceOperationAlias.Nomenclature.Id
								&& warehouseInstanceOperationAlias.Warehouse.Id == _filterViewModel.Warehouse.Id,
							JoinType.LeftOuterJoin);
				
					instanceBalanceProjection = Projections.Sum(() => warehouseInstanceOperationAlias.Amount);
				}
			}
			else if(_filterViewModel.EmployeeStorage != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(n => n.Id == 0);
					instanceBalanceProjection = Projections.Constant(0m);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => employeeInstanceOperationAlias,
							() => nomenclatureAlias.Id == employeeInstanceOperationAlias.Nomenclature.Id
								&& employeeInstanceOperationAlias.Employee.Id == _filterViewModel.EmployeeStorage.Id,
							JoinType.LeftOuterJoin);
				
					instanceBalanceProjection = Projections.Sum(() => employeeInstanceOperationAlias.Amount);
				}
			}
			else if(_filterViewModel.CarStorage != null)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(n => n.Id == 0);
					instanceBalanceProjection = Projections.Constant(0m);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => carInstanceOperationAlias,
							() => nomenclatureAlias.Id == carInstanceOperationAlias.Nomenclature.Id
								&& carInstanceOperationAlias.Car.Id == _filterViewModel.CarStorage.Id,
							JoinType.LeftOuterJoin);
				
					instanceBalanceProjection = Projections.Sum(() => carInstanceOperationAlias.Amount);
				}
			}
			else
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(n => n.Id == 0);
					instanceBalanceProjection = Projections.Constant(0m);
				}
				else
				{
					queryStock
						.JoinEntityAlias(() => instanceOperationAlias,
							() => nomenclatureAlias.Id == instanceOperationAlias.Nomenclature.Id,
							JoinType.LeftOuterJoin);

					instanceBalanceProjection = Projections.Sum(() => instanceOperationAlias.Amount);
				}
			}

			if((_filterViewModel.Warehouse != null || _filterViewModel.EmployeeStorage != null || _filterViewModel.CarStorage != null)
				&& SelectionMode != JournalSelectionMode.None)
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(n => n.Id == 0);
					instanceBalanceProjection = Projections.Constant(0m);
				}
				else
				{
					queryStock.Where(Restrictions.Gt(instanceBalanceProjection, 0));
				}
			}
			else
			{
				if(!_filterViewModel.ShowNomenclatureInstance)
				{
					queryStock.Where(n => n.Id == 0);
					instanceBalanceProjection = Projections.Constant(0m);
				}
				else
				{
					queryStock.Where(Restrictions.Not(Restrictions.Eq(instanceBalanceProjection, 0)));
				}
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

			NomenclatureMinimumBalanceByWarehouse nomenclatureMinimumBalanceByWarehouseAlias = null;

			var minWarehouseBalanceSubquery = QueryOver.Of(() => nomenclatureMinimumBalanceByWarehouseAlias)
				.Where(() => nomenclatureMinimumBalanceByWarehouseAlias.Nomenclature.Id == nomenclatureAlias.Id);

			if(_filterViewModel.Warehouse != null)
			{
				minWarehouseBalanceSubquery.Where(() => nomenclatureMinimumBalanceByWarehouseAlias.Warehouse.Id == _filterViewModel.Warehouse.Id);
			};

			minWarehouseBalanceSubquery
			.Select(
				Projections.Conditional(
					Restrictions.IsNull(Projections.Property(() => nomenclatureMinimumBalanceByWarehouseAlias.Id)),
					Projections.Cast(NHibernateUtil.Int32, Projections.Property(() => nomenclatureAlias.MinStockCount)),
					Projections.Max(() => nomenclatureMinimumBalanceByWarehouseAlias.MinimumBalance)
				));

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
					.SelectSubQuery(minWarehouseBalanceSubquery).WithAlias(() => resultAlias.MinNomenclatureAmount)
					.Select(() => nomenclatureAlias.HasInventoryAccounting).WithAlias(() => resultAlias.HasInventoryAccounting)
					.Select(() => measurementUnitsAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => measurementUnitsAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.Select(Projections.Constant(0m)).WithAlias(() => resultAlias.BulkStockAmount)
					.Select(instanceBalanceProjection).WithAlias(() => resultAlias.InstanceStockAmount)
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockJournalNode>());

			return queryStock;
		};
		
		private void CreateFilter(Action<NomenclatureStockFilterViewModel> filterParams)
		{
			_filterViewModel = _scope.Resolve<NomenclatureStockFilterViewModel>(new TypedParameter(typeof(DialogViewModelBase), this));

			if(filterParams != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterParams);
			}

			Filter = _filterViewModel;
		}

		private void OnDataLoaderLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			if(e.LoadingState == LoadingState.InProgress)
			{
				FooterInfo = "Идет загрузка данных...";
			}

			if(e.LoadingState == LoadingState.Idle)
			{
				var stockBalance = Items.Count > 0
					? Items.OfType<NomenclatureStockJournalNode>()
						.Sum(x => decimal.Round(x.StockAmount, x.UnitDigits))
					: 0;
			
				FooterInfo = $"{base.FooterInfo} | Суммарное кол-во загруженных остатков: {stockBalance}";
			}
		}
	}
}
