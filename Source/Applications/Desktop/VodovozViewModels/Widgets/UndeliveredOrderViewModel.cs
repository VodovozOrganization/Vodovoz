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

namespace Vodovoz.ViewModels.Widgets
{
	public class UndeliveredOrderViewModel : EntityWidgetViewModelBase<UndeliveredOrder>, IDisposable
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private UndeliveryObject _entityObject;
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
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private DelegateCommand _addFineCommand;
		private DelegateCommand _chooseOrderCommand;
		private DelegateCommand _oldOrderSelectCommand;
		private DelegateCommand _newOrderSelectCommand;
		private DelegateCommand _beforeSaveCommand;
		private DelegateCommand _addResultCommand;
		private DelegateCommand _addCommentToTheFieldCommand;
		private DelegateCommand _clearDetalizationCommand;
		private readonly ITdiTab _tab;
		private ITdiTab _newOrderDlg;
		

		public UndeliveredOrderViewModel(UndeliveredOrder entity, ICommonServices commonServices,
			IUndeliveryDetalizationJournalFactory undeliveryDetalizationJournalFactory, IUnitOfWork uow, INavigationManager navigationManager, ILifetimeScope scope,
			ITdiTab tab, IOrderRepository orderRepository, IOrderSelectorFactory orderSelectorFactory, IDeliveryScheduleJournalFactory deliveryScheduleJournalFactory,
			ISubdivisionRepository subdivisionRepository, IEmployeeJournalFactory employeeJournalFactory, IEmployeeRepository employeeRepository, IGtkTabsOpener gtkTabsOpener)
			: base(entity, commonServices)
		{
			_navigationManager = navigationManager ?? throw new ArgumentException(nameof(navigationManager));
			_scope = scope ?? throw new ArgumentException(nameof(scope));
			_tab = tab ?? throw new ArgumentException(nameof(tab));
			_orderRepository = orderRepository ?? throw new ArgumentException(nameof(orderRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentException(nameof(orderSelectorFactory));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentException(nameof(subdivisionRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentException(nameof(employeeRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentException(nameof(gtkTabsOpener));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));

			_canReadDetalization = CommonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(UndeliveryDetalization)).CanRead;

			DeliveryScheduleJournalFactory = deliveryScheduleJournalFactory;
			WorkingEmployeeAutocompleteSelectorFactory = employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory();

			_entityDetalizationJournalFilterViewModel = new UndeliveryDetalizationJournalFilterViewModel();
			UndeliveryDetalizationSelectorFactory = (undeliveryDetalizationJournalFactory ?? throw new ArgumentException(nameof(undeliveryDetalizationJournalFactory)))
				.CreateUndeliveryDetalizationAutocompleteSelectorFactory(_entityDetalizationJournalFilterViewModel);

			ConfigureView();
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

			var filterOrders = _scope.Resolve<OrderJournalFilterViewModel>();
			filterOrders.SetAndRefilterAtOnce(x => x.HideStatuses = hiddenStatusesList.Cast<Enum>().ToArray());

			return filterOrders;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.UndeliveryStatus))
			{
				Entity.SetUndeliveryStatus(Entity.UndeliveryStatus);
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
				if(UoW.IsNew)
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
							toRemoveFromBoth.Add(g);
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
						sb.AppendLine(String.Format("\t- {0}", a));
					}
				}

				if(removedGuiltyList.Any())
				{
					sb.AppendLine("удалил(а) ответственных:");
					foreach(var r in removedGuiltyList)
					{
						sb.AppendLine(String.Format("\t- {0}", r));
					}
				}

				string text = sb.ToString().Trim();

				if(sb.Length > 0)
				{
					Entity.AddCommentToTheField(UoW, CommentedFields.Reason, text);
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
			_newOrderDlg = _gtkTabsOpener.OpenCopyOrderDlg(_tab, order.Id);
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
			if(CommonServices.InteractiveService.Question("Требуется сохранить недовоз. Сохранить?"))
			{
				UoW.Save();
				UoW.Commit();
				_gtkTabsOpener.OpenOrderDlg(_tab, order.Id);
			}
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
			get => _entityObject;
			set
			{
				if(SetField(ref _entityObject, value))
				{
					UndeliveryKindSource = value == null ? _entityKinds : _entityKinds.Where(x => x.UndeliveryObject == value).ToList();
					_entityDetalizationJournalFilterViewModel.UndeliveryObject = value;
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
		public bool CanChangeDetalization => CanReadDetalization && _entityDetalizationJournalFilterViewModel.UndeliveryKind != null;
		public IEntityAutocompleteSelectorFactory UndeliveryDetalizationSelectorFactory { get; }
		public bool HasPermissionOrNew => CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_undeliveries") || Entity.Id == 0;
		public bool CanCloseUndeliveries => CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_undeliveries");
		public bool CanEditUndeliveries => (CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_undeliveries")
										   || Entity.Id == 0)
										   && Entity.OldOrder != null
										   && Entity.UndeliveryStatus != UndeliveryStatus.Closed;
		public Action RemoveItemsFromStatusEnumAction { get; set; }
		public bool CanChangeProblemSource => CommonServices.PermissionService.ValidateUserPresetPermission("can_changeEntity_problem_source", CommonServices.UserService.CurrentUserId);
		public IEntityAutocompleteSelectorFactory OrderSelector { get; set; }
		public string Info => Entity.GetOldOrderInfo(_orderRepository);
		public bool RouteListDoesNotExist => Entity.OldOrder != null && (Entity.OldOrderStatus == OrderStatus.NewOrder
		                                                                 || Entity.OldOrderStatus == OrderStatus.Accepted
		                                                                 || Entity.OldOrderStatus == OrderStatus.WaitForPayment);
		public string NewResultText
		{
			get => _newResultText;
			set
			{
				SetField(ref _newResultText, value);
				OnPropertyChanged(nameof(CanEditUndeliveries));
			}
		}

		public bool CanEditReference => CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");
		public IDeliveryScheduleJournalFactory DeliveryScheduleJournalFactory { get; }
		public Func<bool> IsSaved;
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
				Entity.AddCommentToTheField(
					UoW,
					CommentedFields.Reason,
					String.Format(
						"сменил(а) \"в работе у отдела\" \nс \"{0}\" на \"{1}\"",
						_initialProcDepartmentName,
						Entity.InProcessAtDepartment.Name
					)
				);
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
				AddAutocomment();
				Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.LastEditedTime = DateTime.Now;

				if(Entity.DriverCallType == DriverCallType.NoCall)
				{
					Entity.DriverCallTime = null;
					Entity.DriverCallNr = null;
				}
			}));

		public DelegateCommand NewOrderCommand => _newOrderSelectCommand ?? (_newOrderSelectCommand = new DelegateCommand(
			() =>
			{
				if(Entity.NewOrder == null)
				{
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
				if(UoW.IsNew && Entity.OldOrder == null)
				{
					//открыть окно выбора недовезённого заказа
					var orderJournal = _orderSelectorFactory.CreateOrderJournalViewModel(CreateDefaultFilter());
					orderJournal.SelectionMode = JournalSelectionMode.Single;

					_tab.TabParent.AddTab(orderJournal, _tab, false);

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



		public DelegateCommand ChooseOrderCommand => _chooseOrderCommand ?? (_chooseOrderCommand = new DelegateCommand(
			() =>
			{
				var
					filter = _scope
						.Resolve<OrderJournalFilterViewModel>(); // new OrderJournalFilterViewModel(new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()), new DeliveryPointJournalFactory(), new EmployeeJournalFactory());
				filter.SetAndRefilterAtOnce(
					x => x.RestrictCounterparty = Entity.OldOrder?.Client,
					x => x.HideStatuses = new Enum[] { OrderStatus.WaitForPayment }
				);

				var orderJournal = _orderSelectorFactory.CreateOrderJournalViewModel(filter);
				orderJournal.SelectionMode = JournalSelectionMode.Single;

				_tab.TabParent.AddTab(orderJournal, _tab, false);

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
						_chooseOrderCommand.Execute();
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
						Entity.NewOrder.PaymentType = Entity.OldOrder.PaymentType;
						Entity.NewOrder.OnlineOrder = Entity.OldOrder.OnlineOrder;
						Entity.NewOrder.PaymentByCardFrom = Entity.OldOrder.PaymentByCardFrom;
					}
				};
			}));

		public DelegateCommand AddFineCommand => _addFineCommand ?? (_addFineCommand = new DelegateCommand(
			() =>
			{
				if(Entity.Id == 0)
				{
					if(!CommonServices.InteractiveService.Question("Требуется сохранить недовоз. Сохранить?"))
					{
						return;
					}

					var saved = IsSaved?.Invoke();
					if(!saved.HasValue || !saved.Value)
					{
						return;
					}
				}

				var entityUoWBuilder = EntityUoWBuilder.ForCreate();
				var fineViewModel = _navigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(null, entityUoWBuilder).ViewModel;

				using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					fineViewModel.UndeliveredOrder = uow.GetById<UndeliveredOrder>(Entity.Id);
				}

				var address = new RouteListItemRepository().GetRouteListItemForOrder(UoW, Entity.OldOrder);

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
