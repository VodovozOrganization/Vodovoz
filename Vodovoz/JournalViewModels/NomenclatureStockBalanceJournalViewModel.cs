using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalNodes;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class NomenclatureStockBalanceJournalViewModel : FilterableSingleEntityJournalViewModelBase<Nomenclature, NomenclatureDlg, NomenclatureStockJournalNode, NomenclatureStockFilterViewModel>
	{
		public NomenclatureStockBalanceJournalViewModel(
			NomenclatureStockFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices
		) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Складские остатки";

			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(WarehouseMovementOperation),
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
			WarehouseMovementOperation incomeWarehouseOPerationAlias = null;
			WarehouseMovementOperation writeoffWarehouseOperationAlias = null;

			#region Операции прихода на склад

			var incomeSubQuery = QueryOver.Of<WarehouseMovementOperation>(() => incomeWarehouseOPerationAlias)
				.Where(() => incomeWarehouseOPerationAlias.Nomenclature.Id == nomenclatureAlias.Id);

			if(FilterViewModel != null && FilterViewModel.Warehouse != null) {
				incomeSubQuery.Where(
					Restrictions.Eq(
						Projections.Property(() => incomeWarehouseOPerationAlias.IncomingWarehouse),
						FilterViewModel.Warehouse
					)
				);
			} else {
				//так как не выбран склад считать баланс не имеет смысла
				incomeSubQuery.Where(Restrictions.Eq(Projections.Constant(false), true));
			}

			incomeSubQuery.Select(Projections.Sum(Projections.Property(() => incomeWarehouseOPerationAlias.Amount)));

			#endregion Операции прихода на склад

			#region Операции списания со склада

			var writeoffSubQuery = QueryOver.Of<WarehouseMovementOperation>(() => writeoffWarehouseOperationAlias)
				.Where(() => writeoffWarehouseOperationAlias.Nomenclature.Id == nomenclatureAlias.Id);

			if(FilterViewModel != null && FilterViewModel.Warehouse != null) {
				writeoffSubQuery.Where(
					Restrictions.Eq(
						Projections.Property(() => writeoffWarehouseOperationAlias.WriteoffWarehouse),
						FilterViewModel.Warehouse
					)
				);
			} else {
				//так как не выбран склад считать баланс не имеет смысла
				writeoffSubQuery.Where(Restrictions.Eq(Projections.Constant(false), true));
			}

			writeoffSubQuery.Select(Projections.Sum(Projections.Property(() => writeoffWarehouseOperationAlias.Amount)));

			#endregion Операции списания со склада

			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.SubQuery(incomeSubQuery),
				Projections.SubQuery(writeoffSubQuery)
			);

			var queryStock = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => measurementUnitsAlias);

			if(FilterViewModel != null) {
				queryStock.Where(() => nomenclatureAlias.IsArchive == FilterViewModel.ShowArchive);

				if(FilterViewModel.ExcludedNomenclatureIds != null && FilterViewModel.ExcludedNomenclatureIds.Any()) {
					queryStock.Where(
						Restrictions.Not(
							Restrictions.In(
								Projections.Property(() => nomenclatureAlias.Id), 
								FilterViewModel.ExcludedNomenclatureIds.ToArray()
							)
						)
					);
				}
			}

			queryStock.Where(Restrictions.Gt(projection, 0));

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
							.Select(projection).WithAlias(() => resultAlias.StockAmount)
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockJournalNode>());

			return queryStock;
		};

		protected override Func<NomenclatureDlg> CreateDialogFunction => () => throw new InvalidOperationException("Нельзя создавать номенклатуры из данного журнала");

		protected override Func<NomenclatureStockJournalNode, NomenclatureDlg> OpenDialogFunction => (node) => throw new InvalidOperationException("Нельзя изменять номенклатуры из данного журнала");
	}
}
