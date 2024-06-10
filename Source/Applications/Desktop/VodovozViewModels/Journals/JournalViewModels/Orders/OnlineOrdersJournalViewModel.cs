using System;
using System.Linq;
using System.Timers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OnlineOrdersJournalViewModel : JournalViewModelBase
	{
		private const int _minLengthLikeSearch = 3;
		
		private readonly OnlineOrdersJournalFilterViewModel _filterViewModel;
		private readonly ICommonServices _commonServices;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly int _autoRefreshInterval = 30;
		private Timer _autoRefreshTimer;
		private bool _autoRefreshEnabled;

		public OnlineOrdersJournalViewModel(
			OnlineOrdersJournalFilterViewModel journalFilterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IGtkTabsOpener gtkTabsOpener,
			Action<OnlineOrdersJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_filterViewModel = journalFilterViewModel ?? throw new ArgumentNullException(nameof(journalFilterViewModel));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			var dataLoader = new ThreadDataLoader<OnlineOrdersJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			Title = "Журнал онлайн заказов";

			SearchEnabled = false;
			UpdateOnChanges(typeof(OnlineOrder));
			ConfigureFilter(filterParams);
			CreateNodeActions();
			CreatePopupActions();
			StartAutoRefresh();
		}
		
		public override string FooterInfo
		{
			get
			{
				var autoRefreshInfo = GetAutoRefreshInfo();
				return $"{autoRefreshInfo} | {base.FooterInfo}";
			}
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultEditAction();
			CreateStartAutoRefresh();
			CreateStopAutoRefresh();
		}

		public IQueryOver<OnlineOrder> ItemsQuery(IUnitOfWork uow)
		{
			OnlineOrder onlineOrderAlias = null;
			Order orderAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			Employee employeeWorkWithAlias = null;
			OnlineOrdersJournalNode resultAlias = null;
			District districtAlias = null;
			GeoGroup geographicalGroupAlias = null;
			GeoGroup selfDeliveryGeographicalGroupAlias = null;

			var query = uow.Session.QueryOver(() => onlineOrderAlias)
				.Left.JoinAlias(o => o.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
				.Left.JoinAlias(o => o.EmployeeWorkWith, () => employeeWorkWithAlias)
				.Left.JoinAlias(o => o.Order, () => orderAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicalGroupAlias)
				.Left.JoinAlias(() => orderAlias.SelfDeliveryGeoGroup, () => selfDeliveryGeographicalGroupAlias);
			
			var employeeWorkWithProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => employeeWorkWithAlias.LastName),
				Projections.Property(() => employeeWorkWithAlias.Name),
				Projections.Property(() => employeeWorkWithAlias.Patronymic)
			);

			#region Фильтрация

			if(_filterViewModel.RestrictStatus.HasValue)
			{
				query.Where(o => o.OnlineOrderStatus == _filterViewModel.RestrictStatus);
			}
			
			if(_filterViewModel.OnlineOrderPaymentStatus.HasValue)
			{
				query.Where(o => o.OnlineOrderPaymentStatus == _filterViewModel.OnlineOrderPaymentStatus.Value);
			}
			
			if(_filterViewModel.RestrictPaymentType.HasValue)
			{
				query.Where(o => o.OnlineOrderPaymentType == _filterViewModel.RestrictPaymentType);
			}

			if(_filterViewModel.EmployeeWorkWith != null)
			{
				query.Where(o => o.EmployeeWorkWith == _filterViewModel.EmployeeWorkWith);
			}
			
			if(_filterViewModel.RestrictCounterparty != null)
			{
				query.Where(o => o.Counterparty == _filterViewModel.RestrictCounterparty);
			}
			
			if(_filterViewModel.DeliveryPoint != null)
			{
				query.Where(o => o.DeliveryPoint == _filterViewModel.DeliveryPoint);
			}
			
			if(_filterViewModel.RestrictSource.HasValue)
			{
				query.Where(o => o.Source == _filterViewModel.RestrictSource.Value);
			}
			
			if(_filterViewModel.RestrictSelfDelivery.HasValue)
			{
				if(_filterViewModel.RestrictSelfDelivery.Value)
				{
					query.Where(o => o.IsSelfDelivery);
				}
				else
				{
					query.Where(o => !o.IsSelfDelivery);
				}
			}
			
			if(_filterViewModel.RestrictNeedConfirmationByCall.HasValue)
			{
				if(_filterViewModel.RestrictNeedConfirmationByCall.Value)
				{
					query.Where(o => o.IsNeedConfirmationByCall);
				}
				else
				{
					query.Where(o => !o.IsNeedConfirmationByCall);
				}
			}
			
			if(_filterViewModel.RestrictFastDelivery.HasValue)
			{
				if(_filterViewModel.RestrictFastDelivery.Value)
				{
					query.Where(o => o.IsFastDelivery);
				}
				else
				{
					query.Where(o => !o.IsFastDelivery);
				}
			}

			var startDate = _filterViewModel.StartDate;
			var endDate = _filterViewModel.EndDate;

			switch(_filterViewModel.FilterDateType)
			{
				case OrdersDateFilterType.DeliveryDate:
					if(startDate.HasValue)
					{
						query.Where(o => o.DeliveryDate >= startDate);
					}
					
					if(endDate.HasValue)
					{ 
						query.Where(o => o.DeliveryDate <= endDate); 
					}
					break;
				case OrdersDateFilterType.CreationDate:
					break;
			}
			
			if(_filterViewModel.OrderId.HasValue)
			{
				query.Where(o => o.Order.Id == _filterViewModel.OrderId.Value);
			}

			if(_filterViewModel.OnlineOrderId.HasValue)
			{
				query.Where(o => o.Id == _filterViewModel.OnlineOrderId);
			}
			
			if(!string.IsNullOrWhiteSpace(_filterViewModel.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == _filterViewModel.CounterpartyPhone)
					.And(() => !counterpartyPhoneAlias.IsArchive)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}
			
			if(!string.IsNullOrWhiteSpace(_filterViewModel.CounterpartyNameLike)
				&& _filterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(
					Projections.Property(() => counterpartyAlias.FullName),
					_filterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(_filterViewModel.CounterpartyInn))
			{
				query.Where(() => counterpartyAlias.INN == _filterViewModel.CounterpartyInn);
			}
			
			if (_filterViewModel.GeographicGroup != null)
			{
				query.Where(() => (!orderAlias.SelfDelivery && geographicalGroupAlias.Id == _filterViewModel.GeographicGroup.Id)
					|| (orderAlias.SelfDelivery && selfDeliveryGeographicalGroupAlias.Id == _filterViewModel.GeographicGroup.Id));
			}

			#endregion

			query.Where(_filterViewModel.SearchByAddressViewModel?.GetSearchCriterion(
				() => deliveryPointAlias.CompiledAddress
			));
			
			query.SelectList(list => list
					.SelectGroup(o => o.Id).WithAlias(() => resultAlias.Id)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
					.Select(o => o.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(o => o.IsSelfDelivery).WithAlias(() => resultAlias.IsSelfDelivery)
					.Select(o => o.IsFastDelivery).WithAlias(() => resultAlias.IsFastDelivery)
					.Select(() => deliveryScheduleAlias.Name).WithAlias(() => resultAlias.DeliveryTime)
					.Select(o => o.OnlineOrderStatus).WithAlias(() => resultAlias.OnlineOrderStatus)
					.Select(employeeWorkWithProjection).WithAlias(() => resultAlias.ManagerWorkWith)
					.Select(o => o.Source).WithAlias(() => resultAlias.Source)
					.Select(o => o.OnlineOrderSum).WithAlias(() => resultAlias.OnlineOrderSum)
					.Select(o => o.OnlineOrderPaymentStatus).WithAlias(() => resultAlias.OnlineOrderPaymentStatus)
					.Select(o => o.OnlinePayment).WithAlias(() => resultAlias.OnlinePayment)
					.Select(o => o.OnlineOrderPaymentType).WithAlias(() => resultAlias.OnlineOrderPaymentType)
					.Select(o => o.IsNeedConfirmationByCall).WithAlias(() => resultAlias.IsNeedConfirmationByCall)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				)
				.OrderBy(o => o.OnlineOrderStatus).Asc
				.TransformUsing(Transformers.AliasToBean<OnlineOrdersJournalNode>());

			return query;
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();
			
			PopupActionsList.Add(
				new JournalAction(
					"Перейти в оформленный заказ",
					selectedItems => selectedItems.All(x => (x as OnlineOrdersJournalNode).OrderId.HasValue),
					selectedItems => true,
					(selectedItems) =>
					{
						var selectedNodes = selectedItems.Cast<OnlineOrdersJournalNode>();

						foreach(var selectedNode in selectedNodes)
						{
							_gtkTabsOpener.OpenOrderDlgFromViewModelByNavigator(this, selectedNode.OrderId.Value);
						}
					}
				)
			);
		}

		private void CreateStartAutoRefresh()
		{
			var journalAction = new JournalAction(
				"Вкл автообновление",
				objects => true,
				objects => !_autoRefreshEnabled,
				objects =>
				{
					_autoRefreshTimer.Start();
					_autoRefreshEnabled = true;
					OnPropertyChanged(nameof(FooterInfo));
					UpdateJournalActions?.Invoke();
				}
			);
			
			NodeActionsList.Add(journalAction);
		}
		
		private void CreateStopAutoRefresh()
		{
			var journalAction = new JournalAction(
				"Выкл автообновление",
				objects => true,
				objects => _autoRefreshEnabled,
				objects =>
				{
					_autoRefreshTimer.Stop();
					_autoRefreshEnabled = false;
					OnPropertyChanged(nameof(FooterInfo));
					UpdateJournalActions?.Invoke();
				}
			);
			
			NodeActionsList.Add(journalAction);
		}

		private void ConfigureFilter(Action<OnlineOrdersJournalFilterViewModel> filterParams)
		{
			if(_filterViewModel is null) return;
			_filterViewModel.Journal = this;
			filterParams?.Invoke(_filterViewModel);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
		
		private void CreateDefaultEditAction()
		{
			var permissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(OnlineOrder));
			var editAction = new JournalAction("Изменить",
				selected =>
				{
					var selectedNodes = selected.OfType<OnlineOrdersJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					
					return permissionResult.CanRead;
				},
				selected => true,
				selected =>
				{
					var selectedNodes = selected.OfType<OnlineOrdersJournalNode>();
					
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					var selectedNode = selectedNodes.First();
					
					NavigationManager.OpenViewModel<OnlineOrderViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(selectedNode.Id));
				}
			);
			
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}
		
		private void StartAutoRefresh()
		{
			if(_autoRefreshEnabled)
			{
				return;
			}
			
			_autoRefreshTimer = new Timer(_autoRefreshInterval * 1000);
			_autoRefreshTimer.Elapsed += OnFilterViewModelFiltered;
			_autoRefreshTimer.Start();
			_autoRefreshEnabled = true;
		}
		
		private string GetAutoRefreshInfo()
		{
			return _autoRefreshEnabled ? $"Автообновление каждые {_autoRefreshInterval} сек." : "Автообновление выключено";
		}
		
		public override void Dispose()
		{
			if(_autoRefreshTimer != null)
			{
				_autoRefreshTimer.Stop();
				_autoRefreshTimer.Elapsed -= OnFilterViewModelFiltered;
				_autoRefreshTimer.Close();
				_autoRefreshTimer = null;
			}
			base.Dispose();
		}
	}
}
