using System;
using System.Linq;
using System.Timers;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Reports.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OrdersRatingsJournalViewModel : JournalViewModelBase
	{
		private readonly OrdersRatingsJournalFilterViewModel _filterViewModel;
		private readonly ICommonServices _commonServices;
		private readonly IFileDialogService _fileDialogService;
		private readonly int _autoRefreshInterval = 30;
		private Timer _autoRefreshTimer;
		private bool _autoRefreshEnabled;

		public OrdersRatingsJournalViewModel(
			OrdersRatingsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			Action<OrdersRatingsJournalFilterViewModel> filterParams = null
			) : base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			var dataLoader = new ThreadDataLoader<OrdersRatingsJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(RatingsQuery);
			DataLoader = dataLoader;
			SearchEnabled = false;
			
			Title = "Журнал оценок заказов";
			
			UpdateOnChanges(typeof(OrderRating));
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
			CreateExportAction();
			CreateDefaultEditAction();
			CreateStartAutoRefresh();
			CreateStopAutoRefresh();
		}
		
		private IQueryOver<OrderRating> RatingsQuery(IUnitOfWork uow)
		{
			OrderRating ratingAlias = null;
			OrderRatingReason orderRatingReasonAlias = null;
			Order orderAlias = null;
			OnlineOrder onlineOrderAlias = null;
			Employee processedByEmployeeAlias = null;
			OrdersRatingsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => ratingAlias)
				.Left.JoinAlias(r => r.OnlineOrder, () => onlineOrderAlias)
				.Left.JoinAlias(r => r.Order, () => orderAlias)
				.Left.JoinAlias(r => r.OrderRatingReasons, () => orderRatingReasonAlias)
				.Left.JoinAlias(r => r.ProcessedByEmployee, () => processedByEmployeeAlias);
			
			var processedByEmployeeProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => processedByEmployeeAlias.LastName),
				Projections.Property(() => processedByEmployeeAlias.Name),
				Projections.Property(() => processedByEmployeeAlias.Patronymic)
			);

			#region Фильтрация

			if(_filterViewModel.OrderRatingId.HasValue)
			{
				query.Where(r => r.Id == _filterViewModel.OrderRatingId);
			}
			
			if(_filterViewModel.OrderRatingStatus.HasValue)
			{
				query.Where(o => o.OrderRatingStatus == _filterViewModel.OrderRatingStatus);
			}
			
			if(_filterViewModel.OrderRatingSource.HasValue)
			{
				query.Where(o => o.Source == _filterViewModel.OrderRatingSource.Value);
			}
			
			if(_filterViewModel.OnlineOrderId.HasValue)
			{
				query.Where(() => onlineOrderAlias.Id == _filterViewModel.OnlineOrderId);
			}
			
			if(_filterViewModel.OrderId.HasValue)
			{
				query.Where(() => orderAlias.Id == _filterViewModel.OrderId);
			}

			var startDate = _filterViewModel.StartDate;
			var endDate = _filterViewModel.EndDate;

			if(startDate.HasValue)
			{
				query.Where(r => r.Created >= startDate.Value);
			}
			
			if(endDate.HasValue)
			{
				query.Where(r => r.Created <= endDate.Value.LatestDayTime());
			}

			if(!string.IsNullOrWhiteSpace(_filterViewModel.OrderRatingReason))
			{
				query.Where(
					Restrictions.Like(
						Projections.Property(() => orderRatingReasonAlias.Name),
						_filterViewModel.OrderRatingReason,
						MatchMode.Anywhere));
			}
			
			if(_filterViewModel.OrderRatingValue.HasValue)
			{
				switch(_filterViewModel.RatingCriterion)
				{
					case ComparisonSings.Equally:
						query.Where(r => r.Rating == _filterViewModel.OrderRatingValue);
						break;
					case ComparisonSings.Less:
						query.Where(r => r.Rating < _filterViewModel.OrderRatingValue);
						break;
					case ComparisonSings.LessOrEqual:
						query.Where(r => r.Rating <= _filterViewModel.OrderRatingValue);
						break;
					case ComparisonSings.More:
						query.Where(r => r.Rating > _filterViewModel.OrderRatingValue);
						break;
					case ComparisonSings.MoreOrEqual:
						query.Where(r => r.Rating >= _filterViewModel.OrderRatingValue);
						break;
				}
			}

			#endregion
			
			query.SelectList(list => list
					.SelectGroup(r => r.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => onlineOrderAlias.Id).WithAlias(() => resultAlias.OnlineOrderId)
					.Select(processedByEmployeeProjection).WithAlias(() => resultAlias.ProcessedByEmployee)
					.Select(OrderRatingProjections.GetOrderRatingReasons()).WithAlias(() => resultAlias.OrderRatingReasons)
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

		private void CreateExportAction()
		{
			var journalAction = new JournalAction(
				"Экспорт",
				objects => true,
				objects => true,
				objects =>
				{
					var rows = RatingsQuery(UoW).List<OrdersRatingsJournalNode>();

					if(!rows.Any())
					{
						return;
					}
					
					var report = new OrdersRatingsJournalReport(_fileDialogService);
					report.Export(rows);
				}
			);
			
			NodeActionsList.Add(journalAction);
		}

		private void CreateDefaultEditAction()
		{
			var permissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(OrderRating));
			var editAction = new JournalAction("Изменить",
				selected =>
				{
					var selectedNodes = selected.OfType<OrdersRatingsJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					
					return permissionResult.CanRead;
				},
				selected => true,
				selected =>
				{
					var selectedNodes = selected.OfType<OrdersRatingsJournalNode>();
					
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					var selectedNode = selectedNodes.First();

					NavigationManager.OpenViewModel<OrderRatingViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(selectedNode.Id));
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
			_filterViewModel.IsShow = true;
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
