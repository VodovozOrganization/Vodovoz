using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using NHibernate.SqlCommand;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using NHibernate.Dialect.Function;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class NomenclaturesJournalViewModel
		: EntityJournalViewModelBase<
			Nomenclature,
			NomenclatureViewModel,
			NomenclatureJournalNode>
	{
		private readonly NomenclatureFilterViewModel _filterViewModel;

		public NomenclaturesJournalViewModel(
			NomenclatureFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			Action<NomenclatureFilterViewModel> filterParams = null
		) : base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			TabName = "Журнал ТМЦ";

			if(filterParams != null)
			{
				_filterViewModel.ConfigureWithoutFiltering(filterParams);
			}

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;

			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(GoodsAccountingOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);

			UseSlider = false;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public void HideButtons()
		{
			NodeActionsList.Clear();
		}

		[Obsolete("Лучше передавать через фильтр")]
		public int[] ExcludingNomenclatureIds { get; set; }

		public bool CalculateQuantityOnStock { get; set; } = false;

		public IAdditionalJournalRestriction<Nomenclature> AdditionalJournalRestriction { get; set; } = null;

		protected override IQueryOver<Nomenclature> ItemsQuery(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureOnlineParameters nomenclatureOnlineParametersAlias = null;
			MeasurementUnits unitAlias = null;
			NomenclatureJournalNode resultAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;

			var subQueryBalance = QueryOver.Of(() => operationAlias)
				.Where(() => operationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Select(Projections.Sum<GoodsAccountingOperation>(o => o.Amount));

			var itemsQuery = uow.Session.QueryOver(() => nomenclatureAlias);

			#region Reserved

			IProjection reservedSumProjection = null;
			
			if(CalculateQuantityOnStock)
			{
				itemsQuery
					.JoinEntityAlias(
						() => orderItemsAlias,
						() => orderItemsAlias.Nomenclature.Id == nomenclatureAlias.Id
						      && nomenclatureAlias.DoNotReserve == false,
						JoinType.LeftOuterJoin)
					.JoinEntityAlias(
						() => orderAlias,
						() => orderAlias.Id == orderItemsAlias.Order.Id
						      && orderAlias.OrderStatus.IsIn(
							      new[] { OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading }),
						JoinType.LeftOuterJoin);

				var reservedSqlFunc = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal, "IF(?1 IS NULL, NULL, ?2)"),
					NHibernateUtil.Decimal,
					Projections.Property(() => orderAlias.Id),
					Projections.Property(() => orderItemsAlias.Count));

				reservedSumProjection = Projections.Sum(reservedSqlFunc);
			}

			#endregion Reserved

			if(!CalculateQuantityOnStock)
			{
				itemsQuery.JoinEntityAlias(
					() => nomenclatureOnlineParametersAlias,
					() => nomenclatureOnlineParametersAlias.Nomenclature.Id == nomenclatureAlias.Id,
					JoinType.LeftOuterJoin);
			}

			//Хардкодим выборку номенклатур не для инвентарного учета
			itemsQuery.Where(() => !nomenclatureAlias.HasInventoryAccounting);

			if(!_filterViewModel.RestrictArchive)
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsArchive);
			}

			if(_filterViewModel.RestrictedExcludedIds != null && _filterViewModel.RestrictedExcludedIds.Any())
			{
				itemsQuery.WhereRestrictionOn(() => nomenclatureAlias.Id).Not.IsInG(_filterViewModel.RestrictedExcludedIds);
			}

			if(ExcludingNomenclatureIds != null && ExcludingNomenclatureIds.Any())
			{
				itemsQuery.WhereNot(() => nomenclatureAlias.Id.IsIn(ExcludingNomenclatureIds));
			}

			if(!CalculateQuantityOnStock && _filterViewModel.OnlyOnlineNomenclatures)
			{
				itemsQuery.Where(() => nomenclatureOnlineParametersAlias.NomenclatureOnlineAvailability != null);
			}

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.OnlineStoreExternalId
				)
			);

			if(!_filterViewModel.RestrictDilers)
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsDiler);
			}

			if(_filterViewModel.RestrictCategory == NomenclatureCategory.water)
			{
				itemsQuery.Where(() => nomenclatureAlias.IsDisposableTare == _filterViewModel.RestrictDisposbleTare);
			}

			if(_filterViewModel.RestrictCategory.HasValue)
			{
				itemsQuery.Where(n => n.Category == _filterViewModel.RestrictCategory.Value);
			}

			if(_filterViewModel.SelectCategory.HasValue
				&& _filterViewModel.SelectSaleCategory.HasValue
				&& Nomenclature.GetCategoriesWithSaleCategory().Contains(_filterViewModel.SelectCategory.Value))
			{
				itemsQuery.Where(n => n.SaleCategory == _filterViewModel.SelectSaleCategory);
			}

			if(AdditionalJournalRestriction != null)
			{
				foreach(var expr in AdditionalJournalRestriction.ExternalRestrictions)
				{
					itemsQuery.Where(expr);
				}
			}

			if(_filterViewModel.IsDefectiveBottle)
			{
				itemsQuery.Where(x => x.IsDefectiveBottle);
			}

			if(_filterViewModel.GlassHolderType.HasValue)
			{
				itemsQuery.Where(x => x.GlassHolderType == _filterViewModel.GlassHolderType.Value);
			}

			if(CalculateQuantityOnStock)
			{
				itemsQuery.Left.JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias)
					.Where(() => !nomenclatureAlias.IsSerial)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
						.Select(() => nomenclatureAlias.GlassHolderType).WithAlias(() => resultAlias.GlassHolderType)
						.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
						.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
						.Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
						.SelectSubQuery(subQueryBalance).WithAlias(() => resultAlias.InStock)
						.Select(reservedSumProjection).WithAlias(() => resultAlias.Reserved))
					.OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());
			}
			else
			{
				itemsQuery.Where(() => !nomenclatureAlias.IsSerial)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
						.Select(() => nomenclatureAlias.GlassHolderType).WithAlias(() => resultAlias.GlassHolderType)
						.Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
						.Select(() => false).WithAlias(() => resultAlias.CalculateQtyOnStock))
					.OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>());
			}

			return itemsQuery;
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;

			base.Dispose();
		}
	}
}
