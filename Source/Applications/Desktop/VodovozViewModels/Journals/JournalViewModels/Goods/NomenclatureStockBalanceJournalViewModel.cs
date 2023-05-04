using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Dialogs.Goods;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	//TODO проверить работу запроса и убрать из выборки экземпляры
	public class NomenclatureStockBalanceJournalViewModel :
		FilterableSingleEntityJournalViewModelBase<
			Nomenclature, NomenclatureViewModel, NomenclatureStockJournalNode, NomenclatureStockFilterViewModel>
	{
		public NomenclatureStockBalanceJournalViewModel(
			NomenclatureStockFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Складские остатки";

			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(GoodsAccountingOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		protected override List<IJournalAction> NodeActionsList { get; set; }

		protected override void CreateNodeActions()
		{
			NodeActionsList = new List<IJournalAction>();
			CreateDefaultSelectAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<Nomenclature>> ItemsSourceQueryFunction => (uow) => {
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			NomenclatureStockJournalNode resultAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			GoodsAccountingOperation operationAlias = null;

			//var balanceInStock = QueryOver.Of(() => operationAlias)
			//	.Where(() => operationAlias.Nomenclature.Id == nomenclatureAlias.Id);
			IProjection balanceProjection = null;
			//balanceInStock.Select(Projections.Sum(() => operationAlias.Amount));

			balanceProjection = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
			/*var projectionBalance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				Projections.SubQuery(balanceInStock)
			);*/

			var queryStock = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => measurementUnitsAlias);
			
			
			//Хардкодим выборку номенклатур не для инвентарного учета
			//queryStock.Where(() => !nomenclatureAlias.HasInventoryAccounting);

			if(FilterViewModel != null)
			{
				if(FilterViewModel.Warehouse != null)
				{
					if(!FilterViewModel.ShowNomenclatureInstance)
					{
						queryStock.JoinEntityAlias(() => warehouseBulkOperationAlias,
							() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id,
							JoinType.LeftOuterJoin);
					
						balanceProjection = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
					}
					else
					{
						queryStock
							.JoinEntityAlias(() => warehouseBulkOperationAlias,
								() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id,
								JoinType.LeftOuterJoin)
							.JoinEntityAlias(() => warehouseInstanceOperationAlias,
								() => nomenclatureAlias.Id == warehouseInstanceOperationAlias.Nomenclature.Id,
								JoinType.LeftOuterJoin);
					
						balanceProjection = Projections.Conditional(
							Restrictions.Where(() => warehouseBulkOperationAlias == null),
							Projections.Sum(() => warehouseInstanceOperationAlias.Amount),
							Projections.Sum(() => warehouseBulkOperationAlias.Amount));
					}
				}
				else if(FilterViewModel.EmployeeStorage != null)
				{
					queryStock.JoinEntityAlias(() => employeeInstanceOperationAlias,
						() => nomenclatureAlias.Id == employeeInstanceOperationAlias.Nomenclature.Id,
						JoinType.LeftOuterJoin);
					
					balanceProjection = Projections.Sum(() => employeeInstanceOperationAlias.Amount);
				}
				else if(FilterViewModel.CarStorage != null)
				{
					queryStock.JoinEntityAlias(() => carInstanceOperationAlias,
						() => nomenclatureAlias.Id == carInstanceOperationAlias.Nomenclature.Id,
						JoinType.LeftOuterJoin);
					
					balanceProjection = Projections.Sum(() => carInstanceOperationAlias.Amount);
				}
				else
				{
					queryStock.JoinEntityAlias(() => operationAlias,
						() => nomenclatureAlias.Id == operationAlias.Nomenclature.Id,
						JoinType.LeftOuterJoin);
					
					balanceProjection = Projections.Sum(() => operationAlias.Amount);
				}
				//if(FilterViewModel.)

				if(!FilterViewModel.ShowArchive)
				{
					queryStock.Where(() => nomenclatureAlias.IsArchive == false);
				}

				if(FilterViewModel.ExcludedNomenclatureIds != null && FilterViewModel.ExcludedNomenclatureIds.Any())
				{
					queryStock.Where(
						Restrictions.Not(
							Restrictions.In(
								Projections.Property(() => nomenclatureAlias.Id), 
								FilterViewModel.ExcludedNomenclatureIds.ToArray()
							)
						)
					);
				}
				
				if(FilterViewModel.Warehouse != null  && SelectionMode != JournalSelectionMode.None)
				{
					queryStock.Where(Restrictions.Gt(balanceProjection, 0));
				}
				else if(FilterViewModel.EmployeeStorage != null && SelectionMode != JournalSelectionMode.None)
				{
					queryStock.Where(Restrictions.Gt(balanceProjection, 0));
				}
				else if(FilterViewModel.CarStorage != null && SelectionMode != JournalSelectionMode.None)
				{
					queryStock.Where(Restrictions.Gt(balanceProjection, 0));
				}
				else
				{
					queryStock.Where(Restrictions.Not(Restrictions.Eq(balanceProjection,0)));
				}
				
				
				/*if(FilterViewModel.Warehouse != null && SelectionMode != JournalSelectionMode.None)
				{
					queryStock.Where(Restrictions.Gt(balanceProjection, 0));
				}
				else
				{
					queryStock.Where(Restrictions.Not(Restrictions.Eq(balanceProjection,0)));
				}*/
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
