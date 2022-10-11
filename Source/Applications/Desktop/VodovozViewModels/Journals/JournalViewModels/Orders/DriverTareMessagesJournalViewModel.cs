using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Tdi;
using System;
using System.Linq;
using System.Timers;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class DriverTareMessagesJournalViewModel : JournalViewModelBase
	{
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private Timer _autoRefreshTimer;

		public DriverTareMessagesJournalViewModel(
			IGtkTabsOpener gtkTabsOpener,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null) 
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			Title = "Сообщения водителей по таре";

			var threadDataLoader = new ThreadDataLoader<DriverMessageJournalNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.AddQuery(GetQuery);
			DataLoader = threadDataLoader;

			CreateNodeActions();

			StartAutoRefresh();
		}

		private void StartAutoRefresh()
		{
			_autoRefreshTimer = new Timer(60000);
			_autoRefreshTimer.Elapsed += (s, e) => Refresh();
			_autoRefreshTimer.Start();
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateEditAction();
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Открыть заказ",
				(selected) => {
					var selectedNodes = selected.OfType<DriverMessageJournalNode>();
					return selectedNodes.Count() == 1;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<DriverMessageJournalNode>();
					if(selectedNodes.Count() != 1)
					{
						return;
					}
					var selectedNode = selectedNodes.First();
					_gtkTabsOpener.OpenOrderDlg(this, selectedNode.OrderId);
					HideJournal();
				}
			);
			RowActivatedAction = editAction;
			NodeActionsList.Add(editAction);
		}

		private void HideJournal()
		{
			if(TabParent is ITdiSliderTab slider)
			{
				slider.IsHideJournal = true;
			}
		}

		private IQueryOver<RouteListItem> GetQuery(IUnitOfWork uow)
		{
			BottlesMovementOperation debtBottlesOperationAlias = null;
			VodovozOrder orderAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			Phone phoneAlias = null;
			DriverMessageJournalNode resultAlias = null;

			var bottlesDebtSubquery = QueryOver.Of(() => debtBottlesOperationAlias)
				.Where(() => debtBottlesOperationAlias.DeliveryPoint.Id == orderAlias.DeliveryPoint.Id)
				.Select(
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Int32, "?1 - ?2"),
						NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => debtBottlesOperationAlias.Delivered)),
						Projections.Sum(Projections.Property(() => debtBottlesOperationAlias.Returned))
					)
				);

			var query = uow.Session.QueryOver(() => routeListItemAlias)
				.Left.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => driverAlias.Phones, () => phoneAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderAlias.DriverMobileAppCommentTime)))
				.Where(() => routeListItemAlias.Status != RouteListItemStatus.Transfered)
				.Where(Restrictions.Eq(Projections.SqlFunction("DATE", NHibernateUtil.Date, Projections.Property(() => orderAlias.DriverMobileAppCommentTime)), DateTime.Today));

			query.Where(
				GetSearchCriterion(
					() => orderAlias.Id,
					() => routeListAlias.Id,
					() => driverAlias.LastName,
					() => driverAlias.Name,
					() => driverAlias.Patronymic,
					() => orderAlias.DriverMobileAppComment,
					() => phoneAlias.DigitsNumber
				)
			);

			query.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Property(() => orderAlias.DriverMobileAppCommentTime)).WithAlias(() => resultAlias.CommentDate)
					.Select(Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
							NHibernateUtil.String,
							Projections.Property(() => driverAlias.LastName),
							Projections.Property(() => driverAlias.Name),
							Projections.Property(() => driverAlias.Patronymic)
						))
						.WithAlias(() => resultAlias.DriverName)
					.Select(Projections.Property(() => phoneAlias.Number)).WithAlias(() => resultAlias.DriverPhone)
					.Select(Projections.Property(() => routeListAlias.Id)).WithAlias(() => resultAlias.RouteListId)
					.Select(Projections.Property(() => orderAlias.BottlesReturn)).WithAlias(() => resultAlias.BottlesReturn)
					.Select(Projections.Property(() => routeListItemAlias.DriverBottlesReturned)).WithAlias(() => resultAlias.ActualBottlesReturn)
					.Select(Projections.SubQuery(bottlesDebtSubquery)).WithAlias(() => resultAlias.AddressBottlesDebt)
					.Select(Projections.Property(() => orderAlias.DriverMobileAppComment)).WithAlias(() => resultAlias.DriverComment)
				)
				.OrderBy(() => orderAlias.DriverMobileAppCommentTime).Desc()
				.TransformUsing(Transformers.AliasToBean<DriverMessageJournalNode>());

			return query;
		}

		public override void Dispose()
		{
			_autoRefreshTimer?.Dispose();
			base.Dispose();
		}
	}
}
