using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalNodes;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class OrderForMovDocJournalViewModel : FilterableSingleEntityJournalViewModelBase<VodovozOrder, OrderDlg, OrderForMovDocJournalNode, OrderForMovDocJournalFilterViewModel>
	{
		public OrderForMovDocJournalViewModel(
			OrderForMovDocJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал заказов";
			filterViewModel.ConfigureWithoutFiltering(x => x.IsOnlineStoreOrders = true);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<VodovozOrder>> ItemsSourceQueryFunction => (uow) => {
			OrderForMovDocJournalNode resultAlias = null;
			VodovozOrder orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias);

			if(FilterViewModel?.IsOnlineStoreOrders != null && FilterViewModel.IsOnlineStoreOrders)
				query = query.Where(x => x.EShopOrder != null);

			if(FilterViewModel?.OrderStatuses != null && FilterViewModel.OrderStatuses.Any())
				query = query.WhereRestrictionOn(x => x.OrderStatus).IsIn(FilterViewModel.OrderStatuses.ToArray());

			if(FilterViewModel?.StartDate != null)
				query.Where(o => o.CreateDate >= FilterViewModel.StartDate);

			if(FilterViewModel?.EndDate != null)
				query.Where(o => o.CreateDate <= FilterViewModel.EndDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59));

			var bottleCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var orderSumSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
											.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
											.Select(
												Projections.Sum(
													Projections.SqlFunction(
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 * ?2 - IF(?3 IS NULL OR ?3 = 0, IFNULL(?4, 0), ?3)"),
														NHibernateUtil.Decimal,
														Projections.Property<OrderItem>(x => x.Count),
														Projections.Property<OrderItem>(x => x.Price),
														Projections.Property<OrderItem>(x => x.DiscountMoney),
														Projections.Property<OrderItem>(x => x.OriginalDiscountMoney)
													   )
												   )
											   );

			query.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.Client, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			query.Where(GetSearchCriterion(
				() => orderAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress,
				() => orderAlias.OnlinePaymentNumber,
				() => orderAlias.EShopOrder
			));

			var resultQuery = query
				.SelectList(list => list
				   .Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => orderAlias.SelfDelivery).WithAlias(() => resultAlias.IsSelfDelivery)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				   .Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
				   .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => orderAlias.OnlinePaymentNumber).WithAlias(() => resultAlias.OnlineOrder)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompilledAddress)
				   .Select(() => orderAlias.EShopOrder).WithAlias(() => resultAlias.EShopOrder)
				   .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
				   .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.TransformUsing(Transformers.AliasToBean<OrderForMovDocJournalNode>());

			return resultQuery;
		};

		protected override Func<OrderDlg> CreateDialogFunction => () => new OrderDlg();

		protected override Func<OrderForMovDocJournalNode, OrderDlg> OpenDialogFunction => node => new OrderDlg(node.Id);
	}
}
