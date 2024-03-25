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
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OrdersRatingsJournalViewModel : JournalViewModelBase
	{
		private readonly OrdersRatingsJournalFilterViewModel _filterViewModel;
		private readonly ICommonServices _commonServices;
		private readonly int _autoRefreshInterval = 30;
		private Timer _autoRefreshTimer;
		private bool _autoRefreshEnabled;

		public OrdersRatingsJournalViewModel(
			OrdersRatingsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			Action<OrdersRatingsJournalFilterViewModel> filterParams
			) : base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			
			var dataLoader = new ThreadDataLoader<OnlineOrdersJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(RatingsQuery);
			DataLoader = dataLoader;

			Title = "Журнал онлайн заказов";
			
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
		
		public IQueryOver<OrderRating> RatingsQuery(IUnitOfWork uow)
		{
			OrderRating ratingAlias = null;
			OrderRatingReason orderRatingReasonAlias = null;
			Order orderAlias = null;
			OnlineOrder onlineOrderAlias = null;
			Employee employeeAlias = null;
			OrdersRatingsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => ratingAlias)
				.Left.JoinAlias(r => r.OnlineOrder, () => onlineOrderAlias)
				.Left.JoinAlias(r => r.Order, () => orderAlias)
				.Left.JoinAlias(r => r.OrderRatingReasons, () => orderRatingReasonAlias)
				.Left.JoinAlias(r => r.Employee, () => employeeAlias);
			
			var employeeProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => employeeAlias.LastName),
				Projections.Property(() => employeeAlias.Name),
				Projections.Property(() => employeeAlias.Patronymic)
			);

			#region Фильтрация

			if(_filterViewModel.OrderRatingStatus.HasValue)
			{
				query.Where(o => o.OrderRatingStatus == _filterViewModel.OrderRatingStatus);
			}
			
			if(_filterViewModel.OrderRatingSource.HasValue)
			{
				query.Where(o => o.Source == _filterViewModel.OrderRatingSource.Value);
			}

			var startDate = _filterViewModel.StartDate;
			var endDate = _filterViewModel.EndDate;

			if(startDate.HasValue)
			{
				query.Where(r => r.Created >= startDate.Value);
			}
			
			if(endDate.HasValue)
			{
				query.Where(r => r.Created <= endDate.Value);
			}
			
			if(_filterViewModel.OrderRatingStatus.HasValue)
			{
				query.Where(o => o.OrderRatingStatus == _filterViewModel.OrderRatingStatus);
			}

			#endregion
			
			query.SelectList(list => list
					.SelectGroup(r => r.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => onlineOrderAlias.Id).WithAlias(() => resultAlias.OnlineOrderId)
					.Select(employeeProjection).WithAlias(() => resultAlias.Employee)
					.Select(r => r.Created).WithAlias(() => resultAlias.OrderRatingCreated)
					.Select(r => r.OrderRatingStatus).WithAlias(() => resultAlias.OrderRatingStatus)
					.Select(r => r.Source).WithAlias(() => resultAlias.OrderRatingSource)
					.Select(r => r.Comment).WithAlias(() => resultAlias.OrderRatingComment)
					.Select(r => r.Rating).WithAlias(() => resultAlias.Rating)
				)
				.OrderBy(r => r.OrderRatingStatus).Asc
				.TransformUsing(Transformers.AliasToBean<OrdersRatingsJournalNode>());

			return query;
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateExportAction();
			CreateDefaultEditAction();
			CreateStartAutoRefresh();
			CreateStopAutoRefresh();
		}

		private void CreateExportAction()
		{
			//Реализовать экспорт журнала в эксель
		}

		private void CreateDefaultEditAction()
		{
			var permissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(OrderRating));
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

					if(selectedNode.EntityType == typeof(OnlineOrder))
					{
						NavigationManager.OpenViewModel<OnlineOrderViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
					else if(selectedNode.EntityType == typeof(RequestForCall))
					{
						NavigationManager.OpenViewModel<RequestForCallViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				}
			);
			
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
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
		
		private void ConfigureFilter(Action<OrdersRatingsJournalFilterViewModel> filterParams)
		{
			if(_filterViewModel is null) return;
			filterParams?.Invoke(_filterViewModel);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
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
