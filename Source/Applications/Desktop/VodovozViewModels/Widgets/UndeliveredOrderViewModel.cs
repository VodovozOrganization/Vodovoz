using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.Utilities;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.Widgets
{
	public class UndeliveredOrderViewModel : EntityWidgetViewModelBase<UndeliveredOrder>, IDisposable
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IOrderContractUpdater _contractUpdater;
		private IList<UndeliveryObject> _entityObjectSource;
		private IList<UndeliveryKind> _entityKindSource;
		private IList<UndeliveryKind> _entityKinds;
		private UndeliveryKind _entityKind;
		private readonly UndeliveryDetalizationJournalFilterViewModel _entityDetalizationJournalFilterViewModel;
		private List<UndeliveryTransferAbsenceReason> _entityTransferAbsenceReasonItems;
		private string _initialProcDepartmentName = string.Empty;
		private IList<GuiltyInUndelivery> _initialGuiltyList = new List<GuiltyInUndelivery>();
		private readonly bool _canReadDetalization;
		private string _newResultText;
		private DelegateCommand _addFineCommand;
		private DelegateCommand _oldOrderSelectCommand;
		private DelegateCommand _newOrderSelectCommand;
		private DelegateCommand _beforeSaveCommand;
		private DelegateCommand _addResultCommand;
		private DelegateCommand _addCommentToTheFieldCommand;
		private DelegateCommand _clearDetalizationCommand;
		private ITdiTab _newOrderDlg;
		private UndeliveryObject _undeliveryObject;
		private bool _isUndeliveryStatusChanged;
		private bool _isDepartmentChanged;

		public UndeliveredOrderViewModel(
			UndeliveredOrder entity,
			ICommonServices commonServices,
			IUnitOfWorkFactory uowFactory,
			IUndeliveryDetalizationJournalFactory undeliveryDetalizationJournalFactory,
			IUnitOfWork uow,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			ITdiTab tab,
			IOrderRepository orderRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IDeliveryScheduleJournalFactory deliveryScheduleJournalFactory,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			IEmployeeRepository employeeRepository,
			IGtkTabsOpener gtkTabsOpener,
			IRouteListItemRepository routeListItemRepository,
			IOrderContractUpdater contractUpdater)
			: base(entity, commonServices)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			NavigationManager = navigationManager ?? throw new ArgumentException(nameof(navigationManager));
			Scope = scope ?? throw new ArgumentException(nameof(scope));
			Tab = tab ?? throw new ArgumentException(nameof(tab));
			_orderRepository = orderRepository ?? throw new ArgumentException(nameof(orderRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentException(nameof(orderSelectorFactory));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentException(nameof(subdivisionRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentException(nameof(employeeRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentException(nameof(gtkTabsOpener));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));

			_canReadDetalization = CommonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(UndeliveryDetalization)).CanRead;

			DeliveryScheduleJournalFactory = deliveryScheduleJournalFactory;
			WorkingEmployeeAutocompleteSelectorFactory = employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory();

			_entityDetalizationJournalFilterViewModel = new UndeliveryDetalizationJournalFilterViewModel
			{
				CanChangeFilter = false
			};

			UndeliveryDetalizationSelectorFactory = undeliveryDetalizationJournalFactory
				.CreateUndeliveryDetalizationAutocompleteSelectorFactory(_entityDetalizationJournalFilterViewModel);

			ConfigureView();

			ChooseOrderCommand = CreateChooseOrderCommand();
		}

		private void ConfigureView()
		{
			Entity.PropertyChanged += OnEntityPropertyChanged;

			if(Entity.Id > 0 && Entity.InProcessAtDepartment != null)
			{
				_initialProcDepartmentName = Entity.InProcessAtDepartment.Name;
			}

			if(Entity.Id > 0)
			{
				foreach(GuiltyInUndelivery g in Entity.ObservableGuilty)
				{
					_initialGuiltyList.Add(
						new GuiltyInUndelivery
						{
							Id = g.Id,
							UndeliveredOrder = g.UndeliveredOrder,
							GuiltySide = g.GuiltySide,
							GuiltyDepartment = g.GuiltyDepartment
						}
					);
				}
			}

			_entityKinds = _entityKindSource = UoW.GetAll<UndeliveryKind>().Where(k => !k.IsArchive).ToList();

			OrderSelector = _orderSelectorFactory.CreateOrderAutocompleteSelectorFactory(CreateDefaultFilter());

			if(Entity.Id <= 0)
			{
				Entity.DriverCallTime = DateTime.Now;
				Entity.DispatcherCallTime = DateTime.Now;
			}

			if(Entity.Id <= 0 && Entity.InProcessAtDepartment == null)
			{
				Entity.InProcessAtDepartment = _subdivisionRepository.GetQCDepartment(UoW);
			}

			RefreshParentUndeliveryDetalizationObjects();
		}

		private OrderJournalFilterViewModel CreateDefaultFilter()
		{
			List<OrderStatus> hiddenStatusesList = new List<OrderStatus>();
			var grantedStatusesArray = _orderRepository.GetStatusesForOrderCancelation();
			foreach(OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
			{
				if(!grantedStatusesArray.Contains(status))
				{
					hiddenStatusesList.Add(status);
				}
			}

			var filterOrders = Scope.Resolve<OrderJournalFilterViewModel>();
			filterOrders.SetAndRefilterAtOnce(x => x.HideStatuses = hiddenStatusesList.Cast<Enum>().ToArray());

			return filterOrders;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.UndeliveryStatus))
			{
				_isUndeliveryStatusChanged = true;
			}

			if(e.PropertyName == nameof(Entity.DriverCallType))
			{
				OnDriverCallPlaceChanged();
			}

			if(e.PropertyName == nameof(Entity.UndeliveryDetalization))
			{
				RefreshParentUndeliveryDetalizationObjects();
			}

			if(e.PropertyName == nameof(Entity.NewOrder))
			{
				OnPropertyChanged(nameof(NewOrderText));
				OnPropertyChanged(nameof(TransferText));
			}

			if(e.PropertyName == nameof(Entity.OldOrder))
			{
				if(Entity.Id == 0)
				{
					Entity.OldOrderStatus = Entity.OldOrder.OrderStatus;
				}

				RemoveItemsFromStatusEnumAction?.Invoke();
				OnPropertyChanged(nameof(Info));
				OnPropertyChanged(nameof(HasPermissionOrNew));
				OnPropertyChanged(nameof(CanCloseUndeliveries));
				OnPropertyChanged(nameof(CanEditUndeliveries));
				OnPropertyChanged(nameof(RouteListDoesNotExist));
			}
		}

		private void OnDriverCallPlaceChanged()
		{
			var listDriverCallType = UoW.Session.QueryOver<UndeliveredOrder>()
				.Where(x => x.Id == Entity.Id)
				.Select(x => x.DriverCallType).List<DriverCallType>().FirstOrDefault();

			if(listDriverCallType != Entity.DriverCallType)
			{
				var max = UoW.Session.QueryOver<UndeliveredOrder>().Select(NHibernate.Criterion.Projections.Max<UndeliveredOrder>(x => x.DriverCallNr)).SingleOrDefault<int>();
				if(max != 0)
				{
					Entity.DriverCallNr = max + 1;
				}
				else
				{
					Entity.DriverCallNr = 1;
				}
			}
		}

		private void AddAutocomment()
		{
			#region удаление дублей из спсика ответственных

			IList<GuiltyInUndelivery> guiltyTempList = new List<GuiltyInUndelivery>();
			foreach(GuiltyInUndelivery g in Entity.ObservableGuilty)
			{
				guiltyTempList.Add(g);
			}

			Entity.ObservableGuilty.Clear();
			foreach(GuiltyInUndelivery g in guiltyTempList.Distinct())
			{
				Entity.ObservableGuilty.Add(g);
			}

			#endregion

			#region формирование и добавление автокомментарния об изменении списка ответственных

			if(Entity.Id > 0)
			{
				IList<GuiltyInUndelivery> removedGuiltyList = new List<GuiltyInUndelivery>();
				IList<GuiltyInUndelivery> addedGuiltyList = new List<GuiltyInUndelivery>();
				IList<GuiltyInUndelivery> toRemoveFromBoth = new List<GuiltyInUndelivery>();
				foreach(GuiltyInUndelivery r in _initialGuiltyList)
				{
					removedGuiltyList.Add(r);
				}

				foreach(GuiltyInUndelivery a in Entity.ObservableGuilty)
				{
					addedGuiltyList.Add(a);
				}

				foreach(GuiltyInUndelivery gu in addedGuiltyList)
				{
					foreach(var g in removedGuiltyList)
					{
						if(gu == g)
						{
							toRemoveFromBoth.Add(g);
						}
					}
				}

				foreach(var r in toRemoveFromBoth)
				{
					addedGuiltyList.Remove(r);
					removedGuiltyList.Remove(r);
				}

				StringBuilder sb = new StringBuilder();
				if(addedGuiltyList.Any())
				{
					sb.AppendLine("добавил(а) ответственных:");
					foreach(var a in addedGuiltyList)
					{
						sb.AppendLine($"\t- {a}");
					}
				}

				if(removedGuiltyList.Any())
				{
					sb.AppendLine("удалил(а) ответственных:");
					foreach(var r in removedGuiltyList)
					{
						sb.AppendLine($"\t- {r}");
					}
				}

				string text = sb.ToString().Trim();

				if(sb.Length > 0)
				{
					Entity.AddAutoCommentToOkkDiscussion(UoW, text);
				}
			}

			#endregion
		}

		/// <summary>
		/// Создаёт новый заказ, копируя поля существующего.
		/// </summary>
		/// <param name="order">Заказ, из которого копируются свойства.</param>
		private void CreateNewOrder(Order order)
		{
			_newOrderDlg = _gtkTabsOpener.OpenCopyOrderDlg(Tab, order.Id);
			_newOrderDlg.TabClosed -= OnNewOrderDlgClosed;
			_newOrderDlg.TabClosed += OnNewOrderDlgClosed;
		}

		private void OnNewOrderDlgClosed(object sender, EventArgs e)
		{
			var newOrder = (sender as IEntityDialog)?.EntityObject as Order;
			if(newOrder?.Id > 0)
			{
				Entity.NewOrder = newOrder;
				Entity.NewDeliverySchedule = Entity.NewOrder.DeliverySchedule;
			}
		}

		/// <summary>
		/// Открытие существующего заказа
		/// </summary>
		/// <param name="order">Заказ, который требуется открыть</param>
		private void OpenOrder(Order order)
		{
			//просто открываем заказ, если не выявятся ошибки в процессе работы, снести комментарии
			_gtkTabsOpener.OpenOrderDlg(Tab, order.Id);
			/*if(CommonServices.InteractiveService.Question("Требуется сохранить изменения. Продолжить?"))
			{
				UoW.Save(Entity);
				UoW.Commit();
				_gtkTabsOpener.OpenOrderDlg(Tab, order.Id);
			}*/
		}

		private void RefreshParentUndeliveryDetalizationObjects()
		{
			if(Entity.UndeliveryDetalization != null)
			{
				UndeliveryObject = Entity.UndeliveryDetalization?.UndeliveryKind?.UndeliveryObject;
				UndeliveryKind = Entity.UndeliveryDetalization?.UndeliveryKind;
			}
		}

		[PropertyChangedAlso(nameof(CanChangeDetalization))]
		private bool CanReadDetalization => _canReadDetalization;

		public List<FineItem> FineItems => Entity.Fines.SelectMany(x => x.Items).ToList();

		public IList<UndeliveryObject> UndeliveryObjectSource =>
			_entityObjectSource ?? (_entityObjectSource = UoW.GetAll<UndeliveryObject>().Where(x => !x.IsArchive).ToList());

		public UndeliveryObject UndeliveryObject
		{
			get => _undeliveryObject;
			set
			{
				if(SetField(ref _undeliveryObject, value))
				{
					UndeliveryKindSource = value == null ? _entityKinds : _entityKinds.Where(x => x.UndeliveryObject == value).ToList();
					_entityDetalizationJournalFilterViewModel.UndeliveryObject = value;
					OnPropertyChanged(nameof(CanChangeUndeliveryKind));
				}
			}
		}

		public IList<UndeliveryKind> UndeliveryKindSource
		{
			get
			{
				if(Entity.UndeliveryDetalization?.UndeliveryKind != null && Entity.UndeliveryDetalization.UndeliveryKind.IsArchive)
				{
					_entityKindSource.Add(UoW.GetById<UndeliveryKind>(Entity.UndeliveryDetalization.UndeliveryKind.Id));
				}

				return _entityKindSource;
			}
			set => SetField(ref _entityKindSource, value);
		}

		public UndeliveryKind UndeliveryKind
		{
			get => _entityKind;
			set
			{
				if(SetField(ref _entityKind, value))
				{
					_entityDetalizationJournalFilterViewModel.UndeliveryKind = value;
					OnPropertyChanged(nameof(CanChangeDetalization));
				}
			}
		}

		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanChangeUndeliveryKind => CanEdit && UndeliveryObject != null;
		public bool CanChangeDetalization => CanReadDetalization && UndeliveryKind != null;
		public IEntityAutocompleteSelectorFactory UndeliveryDetalizationSelectorFactory { get; }
		public bool HasPermissionOrNew => CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.UndeliveredOrder.CanEditUndeliveries) || Entity.Id == 0;
		public bool CanCloseUndeliveries => CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.UndeliveredOrder.CanCloseUndeliveries);
		public bool CanEditUndeliveries => (CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.UndeliveredOrder.CanEditUndeliveries)
										   || Entity.Id == 0)
										   && Entity.OldOrder != null
										   && Entity.UndeliveryStatus != UndeliveryStatus.Closed;
		public Action RemoveItemsFromStatusEnumAction { get; set; }
		public bool CanChangeProblemSource => CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.UndeliveredOrder.CanChangeUndeliveryProblemSource, CommonServices.UserService.CurrentUserId);
		public IEntityAutocompleteSelectorFactory OrderSelector { get; set; }
		public string Info => Entity.GetOldOrderInfo(_orderRepository);
		public bool RouteListDoesNotExist => Entity.OldOrder != null
			&& (Entity.OldOrderStatus == OrderStatus.NewOrder
				|| Entity.OldOrderStatus == OrderStatus.Accepted
				|| Entity.OldOrderStatus == OrderStatus.WaitForPayment);
		public string NewResultText
		{
			get => _newResultText;
			set => SetField(ref _newResultText, value);
		}

		public bool CanEditReference => CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanDelete);
		public IDeliveryScheduleJournalFactory DeliveryScheduleJournalFactory { get; }
		public Func<bool, bool> SaveUndelivery;

		public IEntityAutocompleteSelectorFactory WorkingEmployeeAutocompleteSelectorFactory { get; }
		public virtual IEnumerable<UndeliveryTransferAbsenceReason> UndeliveryTransferAbsenceReasonItems =>
			_entityTransferAbsenceReasonItems ?? (_entityTransferAbsenceReasonItems =
				UoW.GetAll<UndeliveryTransferAbsenceReason>().Where(u => !u.IsArchive).ToList());
		public string TransferText => Entity.NewOrder == null
			? "Заказ не\nсоздан"
			: Entity.NewOrder.Title + " на сумму " +
			  string.Format(CurrencyWorks.GetShortCurrencyString(Entity.NewOrder.OrderSum));
		public string NewOrderText => Entity.NewOrder == null ? "Создать новый заказ" : "Открыть заказ";

		#region Commands

		public DelegateCommand AddCommentToTheFieldCommand => _addCommentToTheFieldCommand ?? (_addCommentToTheFieldCommand = new DelegateCommand(
			() =>
			{
				_isDepartmentChanged = true;
			}));

		public DelegateCommand AddResultCommand => _addResultCommand ?? (_addResultCommand = new DelegateCommand(
			() =>
			{
				if(!string.IsNullOrWhiteSpace(NewResultText))
				{
					var newComment = new UndeliveredOrderResultComment();
					var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
					newComment.UndeliveredOrder = Entity;
					newComment.Author = currentEmployee;
					newComment.Comment = NewResultText;
					newComment.CreationTime = DateTime.Now;
					Entity.ObservableResultComments.Add(newComment);
					NewResultText = string.Empty;
				}
			}));

		public DelegateCommand BeforeSaveCommand => _beforeSaveCommand ?? (_beforeSaveCommand = new DelegateCommand(
			() =>
			{
				if(_isUndeliveryStatusChanged)
				{
					Entity.AddAutoCommentByChangeStatus();
				}

				if(_isDepartmentChanged && _initialProcDepartmentName != Entity.InProcessAtDepartment?.Name)
				{
					Entity.AddAutoCommentToOkkDiscussion(UoW, $"сменил(а) \"в работе у отдела\" \nс \"{_initialProcDepartmentName}\" на \"{Entity.InProcessAtDepartment.Name}\"");
				}

				AddAutocomment();
				Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.LastEditedTime = DateTime.Now;

				if(Entity.DriverCallType == DriverCallType.NoCall)
				{
					Entity.DriverCallTime = null;
					Entity.DriverCallNr = null;
				}

				var address = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity.OldOrder);
				if(address != null
					&& RouteListItem.GetUndeliveryStatuses().Contains(address.Status))
				{
					address.BottlesReturned = 0;
					UoW.Save(address);
				}
			}));

		public DelegateCommand NewOrderCommand => _newOrderSelectCommand ?? (_newOrderSelectCommand = new DelegateCommand(
			() =>
			{
				if(Entity.NewOrder == null)
				{
					if(Entity.Id == 0)
					{
						var saved = SaveUndelivery?.Invoke(false);
						if(!saved.HasValue || !saved.Value)
						{
							return;
						}
					}

					CreateNewOrder(Entity.OldOrder);
				}
				else
				{
					OpenOrder(Entity.NewOrder);
				}
			}));

		public DelegateCommand OldOrderSelectCommand => _oldOrderSelectCommand ?? (_oldOrderSelectCommand = new DelegateCommand(
			() =>
			{
				//если новый недовоз без выбранного недовезённого заказа
				if(Entity.Id == 0 && Entity.OldOrder == null)
				{
					//открыть окно выбора недовезённого заказа
					var orderJournal = _orderSelectorFactory.CreateOrderJournalViewModel(CreateDefaultFilter());
					orderJournal.SelectionMode = JournalSelectionMode.Single;

					Tab.TabParent.AddSlaveTab(Tab, orderJournal);

					orderJournal.OnEntitySelectedResult += (s, ea) =>
					{
						var selectedId = ea.SelectedNodes.FirstOrDefault()?.Id ?? 0;
						if(selectedId == 0)
						{
							return;
						}

						Entity.OldOrder = UoW.GetById<Order>(selectedId);
					};
				}
			}));

		public DelegateCommand CreateChooseOrderCommand()
		{
			return new DelegateCommand(
			() =>
			{
				var filter = Scope.Resolve<OrderJournalFilterViewModel>();
				filter.SetAndRefilterAtOnce(
					x => x.RestrictCounterparty = Entity.OldOrder?.Client,
					x => x.HideStatuses = new Enum[] { OrderStatus.WaitForPayment },
					x => x.ViewTypes = ViewTypes.Order
				);

				var orderJournal = _orderSelectorFactory.CreateOrderJournalViewModel(filter);
				orderJournal.SelectionMode = JournalSelectionMode.Single;

				Tab.TabParent.AddSlaveTab(Tab, orderJournal);

				orderJournal.OnEntitySelectedResult += (s, ea) =>
				{
					var selectedId = ea.SelectedNodes.FirstOrDefault()?.Id ?? 0;

					if(selectedId == 0)
					{
						return;
					}

					if(Entity.OldOrder.Id == selectedId)
					{
						CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
							"Перенесённый заказ не может совпадать с недовезённым!");
						ChooseOrderCommand.Execute();
						return;
					}

					Entity.NewOrder = UoW.GetById<Order>(selectedId);
					Entity.NewOrder.Author = this.Entity.OldOrder.Author;
					OnPropertyChanged(nameof(TransferText));
					Entity.NewDeliverySchedule = Entity.NewOrder.DeliverySchedule;

					if((Entity.OldOrder.PaymentType == Domain.Client.PaymentType.PaidOnline) &&
					   (Entity.OldOrder.OrderSum == Entity.NewOrder.OrderSum) &&
					   CommonServices.InteractiveService.Question("Перенести на выбранный заказ Оплату по Карте?"))
					{
						Entity.NewOrder.UpdatePaymentType(Entity.OldOrder.PaymentType, _contractUpdater);
						Entity.NewOrder.OnlinePaymentNumber = Entity.OldOrder.OnlinePaymentNumber;
						Entity.NewOrder.UpdatePaymentByCardFrom(Entity.OldOrder.PaymentByCardFrom, _contractUpdater);
					}
				};
			});
		}

		public DelegateCommand ChooseOrderCommand { get; }

		public DelegateCommand AddFineCommand => _addFineCommand ?? (_addFineCommand = new DelegateCommand(
			() =>
			{
				if(Entity.Id == 0)
				{
					if(!CommonServices.InteractiveService.Question("Требуется сохранить недовоз. Сохранить?"))
					{
						return;
					}

					var saved = SaveUndelivery?.Invoke(false);
					if(!saved.HasValue || !saved.Value)
					{
						return;
					}
				}

				var entityUoWBuilder = EntityUoWBuilder.ForCreate();
				var fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(null, entityUoWBuilder).ViewModel;

				using(var uow = _uowFactory.CreateWithoutRoot())
				{
					fineViewModel.UndeliveredOrder = uow.GetById<UndeliveredOrder>(Entity.Id);
				}

				var address = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity.OldOrder);

				if(address != null)
				{
					fineViewModel.Entity.AddAddress(address);
				}

				fineViewModel.EntitySaved += (sender2, args) =>
				{
					Entity.Fines.Add(args.Entity as Fine);

					OnPropertyChanged(nameof(FineItems));
				};
			}
		));

		public DelegateCommand ClearDetalizationCommand => _clearDetalizationCommand ?? (_clearDetalizationCommand = new DelegateCommand(
			() =>
			{
				Entity.UndeliveryDetalization = null;
			}
		));

		public INavigationManager NavigationManager { get; }

		public ILifetimeScope Scope { get; }

		public ITdiTab Tab { get; }

		#endregion

		public void Dispose()
		{
			if(_newOrderDlg != null)
			{
				_newOrderDlg.TabClosed -= OnNewOrderDlgClosed;
			}
		}
	}
}
