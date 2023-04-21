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
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Dialogs.Goods;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	//TODO проверить работу запроса и убрать из выборки экземпляры
	public class NomenclatureStockBalanceJournalViewModel : FilterableSingleEntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureStockJournalNode, NomenclatureStockFilterViewModel>
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
			WarehouseBulkGoodsAccountingOperation operationAlias = null;

			var balanceInStock = QueryOver.Of(() => operationAlias)
				.Where(() => operationAlias.Nomenclature.Id == nomenclatureAlias.Id);

			if(FilterViewModel?.Warehouse != null)
			{
				balanceInStock.And(() => operationAlias.Warehouse.Id == FilterViewModel.Warehouse.Id);
			}

			balanceInStock.Select(Projections.Sum(Projections.Property(() => operationAlias.Amount)));

			var projectionBalance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				Projections.SubQuery(balanceInStock)
			);

			var queryStock = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.Unit, () => measurementUnitsAlias);
			
			//Хардкодим выборку номенклатур не для инвентарного учета
			queryStock.Where(() => !nomenclatureAlias.HasInventoryAccounting);

			if(FilterViewModel != null) {
				if(!FilterViewModel.ShowArchive) {
					queryStock.Where(() => nomenclatureAlias.IsArchive == false);
				}

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
				if(FilterViewModel.Warehouse != null && SelectionMode != JournalSelectionMode.None) {
					queryStock.Where(Restrictions.Gt(projectionBalance, 0));
				} else {
					queryStock.Where(Restrictions.Not(Restrictions.Eq(projectionBalance,0)));
				}
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
					.Select(projectionBalance).WithAlias(() => resultAlias.StockAmount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockJournalNode>());

			return queryStock;
		};

		protected override Func<NomenclatureViewModel> CreateDialogFunction =>
			() => throw new InvalidOperationException("Нельзя создавать номенклатуры из данного журнала");

		protected override Func<NomenclatureStockJournalNode, NomenclatureViewModel> OpenDialogFunction =>
			(node) => throw new InvalidOperationException("Нельзя изменять номенклатуры из данного журнала");
	}
}
