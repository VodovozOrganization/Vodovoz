using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using EmailService;
using fyiReporting.RDL;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using NHibernate.Proxy;
using NLog;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QS.Tdi;
using QS.Validation;
using QSDocTemplates;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSWidgetLib;
using RdlEngine;
using Vodovoz.Core;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Email;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Sms;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalFilters;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Repositories;
using Vodovoz.Repositories.Client;
using Vodovoz.Repository;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using IntToStringConverter = Vodovoz.Infrastructure.Converters.IntToStringConverter;

namespace Vodovoz
{
	public partial class OrderDlg : EntityDialogBase<Order>,
		ICounterpartyInfoProvider,
		IDeliveryPointInfoProvider,
		IContractInfoProvider,
		ITdiTabAddedNotifier,
		IEmailsInfoProvider,
		ICallTaskProvider,
		ITDICloseControlTab,
		ISmsSendProvider
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		Order templateOrder;

		private readonly IEmployeeService employeeService = VodovozGtkServicesConfig.EmployeeService;
		private readonly IUserRepository userRepository = UserSingletonRepository.GetInstance();
		private readonly DateTime date = new DateTime(2020, 11, 09, 11, 0, 0);
		private bool isEditOrderClicked;

		private IEmployeeRepository employeeRepository { get; set; } = EmployeeSingletonRepository.GetInstance();
		private IOrderRepository orderRepository { get; set;} = OrderSingletonRepository.GetInstance();
		private IRouteListItemRepository routeListItemRepository { get; set; } = new RouteListItemRepository();
		private IEmailRepository emailRepository { get; set; } = new EmailRepository();
		private ICashRepository cashRepository { get; } = new CashRepository();
		private SendDocumentByEmailViewModel SendDocumentByEmailViewModel { get; set; }

		private  INomenclatureRepository nomenclatureRepository;
		public virtual INomenclatureRepository NomenclatureRepository {
			get {
				if (nomenclatureRepository == null) {
					nomenclatureRepository = new EntityRepositories.Goods.NomenclatureRepository();
				}
				return nomenclatureRepository;
			}
		}
		
		private IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		public virtual IEntityAutocompleteSelectorFactory CounterpartySelectorFactory {
			get {
				if(counterpartySelectorFactory == null) {
					counterpartySelectorFactory =
						new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
							CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);
				};
				return counterpartySelectorFactory;
			}
		}
		
		private IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory;
		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory {
			get {
				if(nomenclatureSelectorFactory == null) {
					nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, new NomenclatureFilterViewModel(), CounterpartySelectorFactory,
							NomenclatureRepository, userRepository);
				}
				return nomenclatureSelectorFactory;
			}
		}
		
		#region Работа с боковыми панелями

		public PanelViewType[] InfoWidgets {
			get {
				return new[]{
					PanelViewType.AdditionalAgreementPanelView,
					PanelViewType.CounterpartyView,
					PanelViewType.DeliveryPricePanelView,
					PanelViewType.DeliveryPointView,
					PanelViewType.EmailsPanelView,
					PanelViewType.CallTaskPanelView,
					PanelViewType.SmsSendPanelView
				};
			}
		}

		public Counterparty Counterparty => Entity.Client;

		public DeliveryPoint DeliveryPoint => Entity.DeliveryPoint;

		public CounterpartyContract Contract => Entity.Contract;

		public bool CanHaveEmails => Entity.Id != 0;

		public Order Order => Entity;

		public List<StoredEmail> GetEmails() => Entity.Id != 0 ? emailRepository.GetAllEmailsForOrder(UoW, Entity.Id) : null;

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						orderRepository,
						employeeRepository,
						new BaseParametersProvider(),
						ServicesConfig.CommonServices.UserService,
						SingletonErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		#endregion

		#region Конструкторы, настройка диалога

		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Destroy();
		}

		public OrderDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Order>();
			SendDocumentByEmailViewModel = new SendDocumentByEmailViewModel(emailRepository, employeeRepository);
			Entity.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать заказы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}
			Entity.OrderStatus = OrderStatus.NewOrder;
			TabName = "Новый заказ";
			ConfigureDlg();
		}

		public OrderDlg(Counterparty client) :this()
		{
			Entity.Client = UoW.GetById<Counterparty>(client.Id);
		}
		public OrderDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order>(id);
			SendDocumentByEmailViewModel = new SendDocumentByEmailViewModel(emailRepository, employeeRepository);
			ConfigureDlg();
		}

		public OrderDlg(Order sub) : this(sub.Id)
		{ }

		/// <summary>
		/// Конструктор создан изначально для Mango-Интеграции, 
		/// </summary>
		/// <param name="copiedOrder">Конструктор копирует заказ по Id заказа</param>
		/// <param name="NeedCopy"><c>true</c> копировать заказ, <c>false</c> работает как обычный конструктор.</param>
		public OrderDlg(Order copiedOrder, bool NeedCopy) : this()
		{
			if(NeedCopy) {
				Entity.Client = UoW.GetById<Counterparty>(copiedOrder.Client.Id);
				Entity.DeliveryPoint = UoW.GetById<DeliveryPoint>(copiedOrder.DeliveryPoint.Id);
				Entity.PaymentType = Entity.Client.PaymentMethod;
				FillOrderItems(copiedOrder);
			}
		}

		public void CopyOrderFrom(int id)
		{
			templateOrder = UoW.GetById<Order>(id);
			Entity.Client = templateOrder.Client;
			Entity.Author = templateOrder.Author;
			Entity.DeliveryPoint = templateOrder.DeliveryPoint;
			Entity.Comment = templateOrder.Comment;
			Entity.CommentLogist = templateOrder.CommentLogist;
			Entity.ClientPhone = templateOrder.ClientPhone;
			Entity.TareNonReturnReason = templateOrder.TareNonReturnReason;
			Entity.BillDate = templateOrder.BillDate;
			Entity.BottlesReturn = templateOrder.BottlesReturn;
			Entity.CollectBottles = templateOrder.CollectBottles;
			Entity.CommentManager = templateOrder.CommentManager;
			Entity.DocumentType = templateOrder.DocumentType;
			Entity.ExtraMoney = templateOrder.ExtraMoney;
			Entity.InformationOnTara = templateOrder.InformationOnTara;
			Entity.PreviousOrder = templateOrder.PreviousOrder;
			Entity.ReturnedTare = templateOrder.ReturnedTare;
			Entity.SelfDelivery = templateOrder.SelfDelivery;
			Entity.SignatureType = templateOrder.SignatureType;
			Entity.SumDifferenceReason = templateOrder.SumDifferenceReason;
			Entity.Trifle = templateOrder.Trifle;
			Entity.IsService = templateOrder.IsService;
			Entity.Contract = templateOrder.Contract;
			Entity.ReturnTareReasonCategory = templateOrder.ReturnTareReasonCategory;
			Entity.ReturnTareReason = templateOrder.ReturnTareReason;

			if (templateOrder.PaymentType == PaymentType.ByCard) {
				if (MessageDialogHelper.RunQuestionDialog("Перенести на выбранный заказ Оплату по Карте?")) {
					Entity.PaymentType = templateOrder.PaymentType;
					Entity.OnlineOrder = templateOrder.OnlineOrder;
					Entity.PaymentByCardFrom = templateOrder.PaymentByCardFrom;
				}
			} else {
				Entity.PaymentType = templateOrder.PaymentType;
			}

			Entity.CopyPromotionalSetsFrom(templateOrder);
			Entity.CopyItemsFrom(templateOrder);
			Entity.CopyDocumentsFrom(templateOrder);
			Entity.CopyEquipmentFrom(templateOrder);
			Entity.CopyDepositItemsFrom(templateOrder);
			Entity.UpdateDocuments();
		}
		
		//Копирование меньшего количества полей чем в CopyOrderFrom для пункта "Повторить заказ" в журнале заказов
		public void CopyLesserOrderFrom(int id)
		{
			templateOrder = UoW.GetById<Order>(id);
			Entity.Client = templateOrder.Client;
			Entity.DeliveryPoint = templateOrder.DeliveryPoint;
			Entity.ClientPhone = templateOrder.ClientPhone;
			Entity.IsService = templateOrder.IsService;
			Entity.CopyPromotionalSetsFrom(templateOrder);
			Entity.CopyItemsFrom(templateOrder);
			Entity.CopyDocumentsFrom(templateOrder);
			Entity.CopyEquipmentFrom(templateOrder);
			Entity.CopyDepositItemsFrom(templateOrder);
			Entity.UpdateDocuments();
		}

		public void ConfigureDlg()
		{
			ConfigureTrees();
			ConfigureButtonActions();
			ConfigureSendDocumentByEmailWidget();

			enumDiscountUnit.SetEnumItems((DiscountUnits[])Enum.GetValues(typeof(DiscountUnits)));
			spinDiscount.Adjustment.Upper = 100;

			if(Entity.PreviousOrder != null) {
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
				//TODO Make it clickable.
			} else
				labelPreviousOrder.Visible = false;
			hboxStatusButtons.Visible = orderRepository.GetStatusesForOrderCancelation().Contains(Entity.OrderStatus)
				|| Entity.OrderStatus == OrderStatus.Canceled
				|| Entity.OrderStatus == OrderStatus.Closed
				|| Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.OnLoading;

			orderEquipmentItemsView.Configure(UoWGeneric, Entity);
			orderEquipmentItemsView.OnDeleteEquipment += OrderEquipmentItemsView_OnDeleteEquipment;
			//TODO FIXME Добавить в таблицу закрывающие заказы.

			//Подписывемся на изменения листов для засеривания клиента
			Entity.ObservableOrderDocuments.ListChanged += ObservableOrderDocuments_ListChanged;
			Entity.ObservableOrderDocuments.ElementRemoved += ObservableOrderDocuments_ElementRemoved;
			Entity.ObservableOrderDocuments.ElementAdded += ObservableOrderDocuments_ElementAdded;
			Entity.ObservableOrderDocuments.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableFinalOrderService.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableInitialOrderService.ElementAdded += Entity_UpdateClientCanChange;

			Entity.ObservableOrderItems.ElementAdded += Entity_ObservableOrderItems_ElementAdded;
			Entity.ObservableOrderItems.ElementRemoved += ObservableOrderItems_ElementRemoved;

			//Подписывемся на изменения листа для сокрытия колонки промо-наборов
			Entity.ObservablePromotionalSets.ListChanged += ObservablePromotionalSets_ListChanged;
			Entity.ObservablePromotionalSets.ElementAdded += ObservablePromotionalSets_ElementAdded;
			Entity.ObservablePromotionalSets.ElementRemoved += ObservablePromotionalSets_ElementRemoved;

			//Подписываемся на изменение товара, для обновления количества оборудования в доп. соглашении
			Entity.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged_ChangeCount;
			Entity.ObservableOrderEquipments.ElementChanged += ObservableOrderEquipments_ElementChanged_ChangeCount;
			
			Entity.ObservableOrderDepositItems.ElementAdded += ObservableOrderDepositItemsOnElementAdded;
			Entity.ObservableOrderDepositItems.ElementRemoved += ObservableOrderDepositItemsOnElementRemoved;
			
			Entity.ObservableOrderEquipments.ElementAdded += ObservableOrderEquipmentsOnElementAdded;
			Entity.ObservableOrderEquipments.ElementRemoved += ObservableOrderEquipmentsOnElementRemoved;

			enumSignatureType.ItemsEnum = typeof(OrderSignatureType);
			enumSignatureType.Binding.AddBinding(Entity, s => s.SignatureType, w => w.SelectedItem).InitializeFromSource();

			labelCreationDateValue.Binding.AddFuncBinding(Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			ylabelOrderStatus.Binding.AddFuncBinding(Entity, e => e.OrderStatus.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelNumber.Binding.AddFuncBinding(Entity, e => e.Code1c + (e.DailyNumber.HasValue ? $" ({e.DailyNumber})" : ""), w => w.LabelProp).InitializeFromSource();

			enumDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDocumentType.Binding.AddBinding(Entity, s => s.DocumentType, w => w.SelectedItem).InitializeFromSource();

			chkContractCloser.Binding.AddBinding(Entity, c => c.IsContractCloser, w => w.Active).InitializeFromSource();

			chkCommentForDriver.Binding.AddBinding(Entity, c => c.HasCommentForDriver, w => w.Active).InitializeFromSource();

			pickerDeliveryDate.Binding.AddBinding(Entity, s => s.DeliveryDate, w => w.DateOrNull).InitializeFromSource();
			pickerDeliveryDate.DateChanged += PickerDeliveryDate_DateChanged;
			pickerBillDate.Visible = labelBillDate.Visible = Entity.PaymentType == PaymentType.cashless;
			pickerBillDate.Binding.AddBinding(Entity, s => s.BillDate, w => w.DateOrNull).InitializeFromSource();

			textComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			textCommentsLogistic.Binding.AddBinding(Entity, s => s.CommentLogist, w => w.Buffer.Text).InitializeFromSource();

			checkSelfDelivery.Binding.AddBinding(Entity, s => s.SelfDelivery, w => w.Active).InitializeFromSource();
			checkPayAfterLoad.Binding.AddBinding(Entity, s => s.PayAfterShipment, w => w.Active).InitializeFromSource();
			checkDelivered.Binding.AddBinding(Entity, s => s.Shipped, w => w.Active).InitializeFromSource();
			ylabelloadAllowed.Binding.AddFuncBinding(Entity, s => s.LoadAllowedBy != null ? s.LoadAllowedBy.ShortName : string.Empty, w => w.Text).InitializeFromSource();
			entryBottlesToReturn.ValidationMode = ValidationType.numeric;
			entryBottlesToReturn.Binding.AddBinding(Entity, e => e.BottlesReturn, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			entryBottlesToReturn.Changed += OnEntryBottlesToReturnChanged;

			yChkActionBottle.Binding.AddBinding(Entity, e => e.IsBottleStock, w => w.Active).InitializeFromSource();
			yEntTareActBtlFromClient.ValidationMode = ValidationType.numeric;
			yEntTareActBtlFromClient.Binding.AddBinding(Entity, e => e.BottlesByStockCount, w => w.Text, new IntToStringValuableConverter()).InitializeFromSource();
			yEntTareActBtlFromClient.Changed += OnYEntTareActBtlFromClientChanged;

			if(Entity.OrderStatus == OrderStatus.Closed) {
				entryTareReturned.Text = new BottlesRepository().GetEmptyBottlesFromClientByOrder(UoW, NomenclatureRepository, Entity).ToString();
				entryTareReturned.Visible = lblTareReturned.Visible = true;
			}

			entryTrifle.ValidationMode = ValidationType.numeric;
			entryTrifle.Binding.AddBinding(Entity, e => e.Trifle, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			ylabelContract.Binding.AddFuncBinding(Entity, e => e.Contract != null ? e.Contract.Title : string.Empty, w => w.Text).InitializeFromSource();

			OldFieldsConfigure();

			entOnlineOrder.ValidationMode = ValidationType.numeric;
			entOnlineOrder.Binding.AddBinding(Entity, e => e.OnlineOrder, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			var excludedPaymentFromId = new BaseParametersProvider().GetSmsPaymentByCardFromId;
			if (Entity.PaymentByCardFrom?.Id != excludedPaymentFromId)
				ySpecPaymentFrom.ItemsList = UoW.Session.QueryOver<PaymentFrom>().Where(x => x.Id != excludedPaymentFromId).List();
			else
				ySpecPaymentFrom.ItemsList = UoW.GetAll<PaymentFrom>();
			ySpecPaymentFrom.Binding.AddBinding(Entity, e => e.PaymentByCardFrom, w => w.SelectedItem).InitializeFromSource();

			enumTax.ItemsEnum = typeof(TaxType);
			Enum[] hideTaxTypeEnums = { TaxType.None };
			enumTax.AddEnumToHideList(hideTaxTypeEnums);
			enumTax.ChangedByUser += (sender, args) => { Entity.Client.TaxType = (TaxType)enumTax.SelectedItem; }; 
			
			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.SetAndRefilterAtOnce(x => x.RestrictIncludeArhive = false);
			entityVMEntryClient.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);
			entityVMEntryClient.Binding.AddBinding(Entity, s => s.Client, w => w.Subject).InitializeFromSource();
			entityVMEntryClient.CanEditReference = true;

			referenceDeliverySchedule.ItemsQuery = DeliveryScheduleRepository.AllQuery();
			referenceDeliverySchedule.SetObjectDisplayFunc<DeliverySchedule>(e => e.Name);
			referenceDeliverySchedule.Binding.AddBinding(Entity, s => s.DeliverySchedule, w => w.Subject).InitializeFromSource();
			referenceDeliverySchedule.Binding.AddBinding(Entity, s => s.DeliverySchedule1c, w => w.TooltipText).InitializeFromSource();

			var filterAuthor = new EmployeeFilterViewModel {
				Status = EmployeeStatus.IsWorking
			};
			referenceAuthor.RepresentationModel = new ViewModel.EmployeesVM(filterAuthor);
			referenceAuthor.Binding.AddBinding(Entity, s => s.Author, w => w.Subject).InitializeFromSource();
			referenceAuthor.Sensitive = false;

			referenceDeliveryPoint.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceDeliveryPoint.CanEditReference = true;
			chkContractCloser.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_contract_closer");

			buttonViewDocument.Sensitive = false;
			btnDeleteOrderItem.Sensitive = false;
			notebook1.ShowTabs = false;
			notebook1.Page = 0;

			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);

			commentsview4.UoW = UoWGeneric;

			enumAddRentButton.ItemsEnum = typeof(OrderAgreementType);
			enumAddRentButton.EnumItemClicked += (sender, e) => AddRentAgreement((OrderAgreementType)e.ItemEnum);

			checkSelfDelivery.Toggled += (sender, e) => {
				referenceDeliverySchedule.Sensitive = labelDeliverySchedule.Sensitive = !checkSelfDelivery.Active;
				lblDeliveryPoint.Sensitive = referenceDeliveryPoint.Sensitive = !checkSelfDelivery.Active;
				buttonAddMaster.Sensitive = !checkSelfDelivery.Active;

				Enum[] hideEnums = { PaymentType.Terminal };
				
				if(Entity.SelfDelivery)
					enumPaymentType.AddEnumToHideList(hideEnums);
				else
					enumPaymentType.ClearEnumHideList();
				
				Entity.UpdateClientDefaultParam();
				enumPaymentType.SelectedItem = Entity.PaymentType;
				
				if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
					OnFormOrderActions();
			};

			dataSumDifferenceReason.Binding.AddBinding(Entity, s => s.SumDifferenceReason, w => w.Text).InitializeFromSource();
			dataSumDifferenceReason.Completion = new EntryCompletion {
				Model = GetListStoreSumDifferenceReasons(UoWGeneric),
				TextColumn = 0
			};

			spinSumDifference.Binding.AddBinding(Entity, e => e.ExtraMoney, w => w.ValueAsDecimal).InitializeFromSource();

			labelSum.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.TotalSum), w => w.LabelProp).InitializeFromSource();
			labelCashToReceive.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.OrderCashSum), w => w.LabelProp).InitializeFromSource();

			enumPaymentType.ItemsEnum = typeof(PaymentType);
			enumPaymentType.Binding.AddBinding(Entity, s => s.PaymentType, w => w.SelectedItem).InitializeFromSource();
			SetSensitivityOfPaymentType();

			textManagerComments.Binding.AddBinding(Entity, s => s.CommentManager, w => w.Buffer.Text).InitializeFromSource();
			enumDiverCallType.ItemsEnum = typeof(DriverCallType);
			enumDiverCallType.Binding.AddBinding(Entity, s => s.DriverCallType, w => w.SelectedItem).InitializeFromSource();

			referenceDriverCallId.Binding.AddBinding(Entity, e => e.DriverCallId, w => w.Subject).InitializeFromSource();

			ySpecCmbNonReturnReason.ItemsList = UoW.Session.QueryOver<NonReturnReason>().List();
			ySpecCmbNonReturnReason.Binding.AddBinding(Entity, e => e.TareNonReturnReason, w => w.SelectedItem).InitializeFromSource();
			ySpecCmbNonReturnReason.ItemSelected += (sender, e) => Entity.IsTareNonReturnReasonChangedByUser = true;

			if(Entity.DeliveryPoint == null && !string.IsNullOrWhiteSpace(Entity.Address1c)) {
				var deliveryPoint = Counterparty.DeliveryPoints.FirstOrDefault(d => d.Address1c == Entity.Address1c);
				if(deliveryPoint != null)
					Entity.DeliveryPoint = deliveryPoint;
			}

			OrderItemEquipmentCountHasChanges = false;
			ShowOrderColumnInDocumentsList();

			SetSensitivityOfPaymentType();
			depositrefunditemsview.Configure(UoWGeneric, Entity);
			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = UoW.Session.QueryOver<DiscountReason>().List();

			yCmbReturnTareReasonCategories.SetRenderTextFunc<ReturnTareReasonCategory>(x => x.Name);
			yCmbReturnTareReasonCategories.ItemsList = UoW.Session.QueryOver<ReturnTareReasonCategory>().List();
			yCmbReturnTareReasonCategories.Binding.AddBinding(Entity, e => e.ReturnTareReasonCategory, w => w.SelectedItem).InitializeFromSource();
			yCmbReturnTareReasonCategories.Changed += YCmbReturnTareReasonCategoriesOnChanged;
			HboxReturnTareReasonCategoriesShow();

			yCmbReturnTareReasons.SetRenderTextFunc<ReturnTareReason>(x => x.Name);
			
			if(Entity.ReturnTareReasonCategory != null)
				ChangeHboxReasonsVisibility();
			
			yCmbReturnTareReasons.Binding.AddBinding(Entity, e => e.ReturnTareReason, w => w.SelectedItem).InitializeFromSource();
			
			yCmbPromoSets.SetRenderTextFunc<PromotionalSet>(x => x.ShortTitle);
			yCmbPromoSets.ItemSelected += YCmbPromoSets_ItemSelected;

			yvalidatedentryEShopOrder.ValidationMode = ValidationType.numeric;
			yvalidatedentryEShopOrder.Binding.AddBinding(Entity, c => c.EShopOrder, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			chkAddCertificates.Binding.AddBinding(Entity, c => c.AddCertificates, w => w.Active).InitializeFromSource();

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<WaterSalesAgreement>(WaterSalesAgreement_ObjectUpdatedGeneric);
			ToggleVisibilityOfDeposits(Entity.ObservableOrderDepositItems.Any());
			SetDiscountEditable();
			SetDiscountUnitEditable();

			spinSumDifference.Hide();
			labelSumDifference.Hide();
			dataSumDifferenceReason.Hide();
			labelSumDifferenceReason.Hide();

			UpdateUIState();

			yChkActionBottle.Toggled += (sender, e) => {
				IStandartDiscountsService standartDiscountsService = new BaseParametersProvider();
				Entity.RecalculateStockBottles(standartDiscountsService);
				ControlsActionBottleAccessibility();
			};
			ycheckContactlessDelivery.Binding.AddBinding(Entity, e => e.ContactlessDelivery, w => w.Active).InitializeFromSource();
			ycheckPaymentBySms.Binding.AddBinding(Entity, e => e.PaymentBySms, w => w.Active).InitializeFromSource();
			
			Entity.InteractiveService = ServicesConfig.InteractiveService;

			Entity.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(Order.OrderStatus)) {
					CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.OrderStatus));
				} 
				else if(args.PropertyName == nameof(Order.Contract)) {
					CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.Contract));
				} 
			};
		}

		public ListStore GetListStoreSumDifferenceReasons(IUnitOfWork uow)
		{
			Order order = null;

			var reasons = uow.Session.QueryOver(() => order)
				.Select(NHibernate.Criterion.Projections.Distinct(NHibernate.Criterion.Projections.Property(() => order.SumDifferenceReason)))
				.List<string>();

			var store = new ListStore(typeof(string));
			foreach(string s in reasons) {
				store.AppendValues(s);
			}
			return store;
		}

		void ControlsActionBottleAccessibility()
		{
			bool canAddAction = Entity.CanAddStockBottle(OrderSingletonRepository.GetInstance()) || Entity.IsBottleStock;
			hboxBottlesByStock.Visible = canAddAction;
			lblActionBtlTareFromClient.Visible = yEntTareActBtlFromClient.Visible = yChkActionBottle.Active;
			hboxReturnTare.Visible = !canAddAction;
			yEntTareActBtlFromClient.Sensitive = canAddAction;
		}

		private void ConfigureTrees()
		{
			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorBlue = new Gdk.Color(0, 0, 0xff);
			var colorGreen = new Gdk.Color(0, 0xff, 0);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightYellow = new Gdk.Color(0xe1, 0xd6, 0x70);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderItem>()
				.AddColumn("Номенклатура")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn(!orderRepository.GetStatusesForActualCount(Entity).Contains(Entity.OrderStatus) ? "Кол-во" : "Кол-во [Факт]")
				.SetTag("Count")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddSetter((c, node) => c.Editable = node.CanEditAmount).WidthChars(10)
				.AddTextRenderer(node => node.ActualCount.HasValue ? string.Format("[{0}]", node.ActualCount) : string.Empty)
				.AddTextRenderer(node => node.CanShowReturnedCount ? string.Format("({0})", node.ReturnedCount) : string.Empty)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? string.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.IsRentCategory ? node.RentString : string.Empty)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, node) => c.Editable = node.CanEditPrice)
					.AddSetter((NodeCellRendererSpin<OrderItem> c, OrderItem node) => {
						if(Entity.OrderStatus == OrderStatus.NewOrder || (Entity.OrderStatus == OrderStatus.WaitForPayment && !Entity.SelfDelivery))//костыль. на Win10 не видна цветная цена, если виджет засерен
						{
							c.ForegroundGdk = colorBlack;
							if(node.AdditionalAgreement == null) {
								return;
							}
							AdditionalAgreement aa = node.AdditionalAgreement.Self;
							if(aa is WaterSalesAgreement wsa && wsa.HasFixedPrice && wsa.FixedPrices.Any(x => x.Nomenclature.Id == node.Nomenclature.Id)) {
								c.ForegroundGdk = colorGreen;
							} else if(node.IsUserPrice && Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category)) {
								c.ForegroundGdk = colorBlue;
							}
						}
					})
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS))
					.AddSetter((c, n) => c.Visible = Entity.PaymentType == PaymentType.cashless)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.ActualSum))
					.AddSetter((c, n) => {
						if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
							c.Text = CurrencyWorks.GetShortCurrencyString(n.OriginalSum);
						}
					)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ManualChangingDiscount)
					.AddSetter((c, n) => c.Editable = n.PromoSet == null)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)(n.Price * n.CurrentCount), 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.AddSetter((c, n) => {
						if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
							c.Text = n.ManualChangingOriginalDiscount.ToString();
					}) 
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
					.AddSetter((c, n) => c.Activatable = n.PromoSet == null)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(node => node.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(orderRepository.GetDiscountReasons(UoW))
					.AddSetter((c, n) => c.Editable = n.Discount > 0 && n.PromoSet == null)
					.AddSetter(
						(c, n) => c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null
						? colorLightRed
						: colorWhite
					)
					.AddSetter((c, n) => {
						if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
							c.Text = n.OriginalDiscountReason?.Name ?? n.DiscountReason?.Name;
						}
					)
				.AddColumn("Доп. соглашение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.AgreementString)
				.AddColumn("Промо-наборы").SetTag(nameof(Entity.PromotionalSets))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.PromoSet == null ? "" : node.PromoSet.Name)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = Entity.ObservableOrderItems;
			treeItems.Selection.Changed += TreeItems_Selection_Changed;
			treeItems.ColumnsConfig.GetColumnsByTag(nameof(Entity.PromotionalSets)).FirstOrDefault().Visible = Entity.PromotionalSets.Count > 0;

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<OrderDocument>()
				.AddColumn("Документ").SetDataProperty(node => node.Name)
				.AddColumn("Дата документа").AddTextRenderer(node => node.DocumentDateText)
				.AddColumn("Заказ №").SetTag("OrderNumberColumn").AddTextRenderer(node => node.Order.Id != node.AttachedToOrder.Id ? node.Order.Id.ToString() : "")
				.AddColumn("Без рекламы").AddToggleRenderer(x => x is IAdvertisable && (x as IAdvertisable).WithoutAdvertising)
				.Editing().ChangeSetProperty(PropertyUtil.GetPropertyInfo<IAdvertisable>(x => x.WithoutAdvertising))
				.AddSetter((c, n) => c.Visible = n.Type == OrderDocumentType.Invoice || n.Type == OrderDocumentType.InvoiceContractDoc)
				.AddColumn("Без подписей и печати").AddToggleRenderer(x => x is ISignableDocument && (x as ISignableDocument).HideSignature)
				.Editing().ChangeSetProperty(PropertyUtil.GetPropertyInfo<ISignableDocument>(x => x.HideSignature))
				.AddSetter((c, n) => c.Visible = n is ISignableDocument)
				.AddSetter((toggle, document) =>
				{
					if (document.Type == OrderDocumentType.UPD)
					{
						toggle.Activatable =
							ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
								"can_edit_seal_and_signature_UPD");
					}
					else
					{
						toggle.Activatable = true;  // по умолчанию Activatable false
					}
					
				} ) // Сделать только для  ISignableDocument и UDP
				.AddColumn("")
				.RowCells().AddSetter<CellRenderer>((c, n) => {
					c.CellBackgroundGdk = colorWhite;
					if(n.Order.Id != n.AttachedToOrder.Id && !(c is CellRendererToggle)) {
						c.CellBackgroundGdk = colorLightYellow;
					}
				})
				.Finish();
			treeDocuments.Selection.Mode = SelectionMode.Multiple;
			treeDocuments.ItemsDataSource = Entity.ObservableOrderDocuments;
			treeDocuments.Selection.Changed += Selection_Changed;

			treeDocuments.RowActivated += (o, args) => OrderDocumentsOpener();

			treeServiceClaim.ColumnsConfig = ColumnsConfigFactory.Create<ServiceClaim>()
				.AddColumn("Статус заявки").SetDataProperty(node => node.Status.GetEnumTitle())
				.AddColumn("Номенклатура оборудования").SetDataProperty(node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn("Серийный номер").SetDataProperty(node => node.Equipment != null && node.Equipment.Nomenclature.IsSerial ? node.Equipment.Serial : "-")
				.AddColumn("Причина").SetDataProperty(node => node.Reason)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();

			treeServiceClaim.ItemsDataSource = Entity.ObservableInitialOrderService;
			treeServiceClaim.Selection.Changed += TreeServiceClaim_Selection_Changed;
		}

		MenuItem menuItemCloseOrder = null;
		MenuItem menuItemSelfDeliveryToLoading = null;
		MenuItem menuItemSelfDeliveryPaid = null;
		MenuItem menuItemReturnToAccepted = null;

		/// <summary>
		/// Конфигурирование меню кнопок с дополнительными действиями заказа
		/// </summary>
		public void ConfigureButtonActions()
		{
			menubuttonActions.MenuAllocation = ButtonMenuAllocation.Top;
			menubuttonActions.MenuAlignment = ButtonMenuAlignment.Right;
			Menu menu = new Menu();

			menuItemCloseOrder = new MenuItem("Закрыть без доставки");
			menuItemCloseOrder.Activated += OnButtonCloseOrderClicked;
			menu.Add(menuItemCloseOrder);

			menuItemReturnToAccepted = new MenuItem("Вернуть в Принят");
			menuItemReturnToAccepted.Activated += OnButtonReturnToAcceptedClicked;
			menu.Add(menuItemReturnToAccepted);

			menuItemSelfDeliveryToLoading = new MenuItem("Самовывоз на погрузку");
			menuItemSelfDeliveryToLoading.Activated += OnButtonSelfDeliveryToLoadingClicked;
			menu.Add(menuItemSelfDeliveryToLoading);

			menuItemSelfDeliveryPaid = new MenuItem("Принять оплату самовывоза");
			menuItemSelfDeliveryPaid.Activated += OnButtonSelfDeliveryAcceptPaidClicked;
			menu.Add(menuItemSelfDeliveryPaid);

			menubuttonActions.Menu = menu;
			menubuttonActions.LabelXAlign = 0.5f;
			menu.ShowAll();
		}
		
		private void ConfigureSendDocumentByEmailWidget()
		{
			var sendEmailView = new SendDocumentByEmailView(SendDocumentByEmailViewModel);
			hbox19.Add(sendEmailView);
			sendEmailView.Show();
		}

		/// <summary>
		/// Старые поля, оставлены для отображения информации в старых заказах. В новых скрыты.
		/// Не удаляем полностью а только скрываем, чтобы можно было увидеть адрес в старых заказах, загруженных из 1с.
		/// </summary>
		private void OldFieldsConfigure()
		{
			textTaraComments.Binding.AddBinding(Entity, e => e.InformationOnTara, w => w.Buffer.Text).InitializeFromSource();
			hbxTareComments.Visible = !string.IsNullOrWhiteSpace(Entity.InformationOnTara);

			
			if (Entity.Client != null)
			{
				if (Entity.Client.IsChainStore)
				{
					textODZComments.Binding.AddBinding(Entity, e => e.ODZComment, w => w.Buffer.Text)
						.InitializeFromSource();
					textOPComments.Binding.AddBinding(Entity, e => e.OPComment, w => w.Buffer.Text)
						.InitializeFromSource();
				}
				else
				{
					hbxODZComments.Visible = false;
					hbxOPComments.Visible = false;
				}
			}
			
			int currentUserId = userRepository.GetCurrentUser(UoW).Id;
			bool canChangeCommentOdz = ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_change_odz_op_comment", currentUserId);
			textODZComments.Sensitive = canChangeCommentOdz;
			textOPComments.Sensitive = canChangeCommentOdz;
		}

		void WaterSalesAgreement_ObjectUpdatedGeneric(EntityChangeEvent[] changeEvents)
		{
			foreach(var ad in changeEvents.Select(x => x.GetEntity<WaterSalesAgreement>())) {
				foreach(var item in Entity.OrderItems) {
					if(item.AdditionalAgreement?.Id == ad.Id)
						UoW.Session.Refresh(item.AdditionalAgreement);
				}
				Entity.UpdatePrices(ad);
			}
		}

		#endregion

		#region Сохранение, закрытие заказа

		bool SaveOrderBeforeContinue<T>()
		{
			if(UoWGeneric.IsNew) {
				if(CommonDialogs.SaveBeforeCreateSlaveEntity(EntityObject.GetType(), typeof(T))) {
					if(!Save())
						return false;
				} else
					return false;
			}
			return true;
		}

		private bool canClose = true;
		public bool CanClose()
		{
			if(!canClose)
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения задачи и повторите");
			return canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
		}

		public override bool Save()
		{
			try {
				SetSensetivity(false);
				Entity.CheckAndSetOrderIsService();

				var valid = new QSValidator<Order>(
					Entity, new Dictionary<object, object>{
					{ "IsCopiedFromUndelivery", templateOrder != null } //индикатор того, что заказ - копия, созданная из недовозов
					}
				);

				if(valid.RunDlgIfNotValid((Window)this.Toplevel))
					return false;

				if(Entity.OrderStatus == OrderStatus.NewOrder) {
					if(!MessageDialogHelper.RunQuestionDialog("Вы не подтвердили заказ. Вы уверены что хотите оставить его в качестве черновика?"))
						return false;
				}
				
				if (Entity.Id == 0 && 
					Entity.PaymentType == PaymentType.cashless) {
					Entity.OrderPaymentStatus = OrderPaymentStatus.UnPaid;
				}

				if(OrderItemEquipmentCountHasChanges) {
					MessageDialogHelper.RunInfoDialog("Было изменено количество оборудования в заказе, оно также будет изменено в дополнительном соглашении");
				}

				logger.Info("Сохраняем заказ...");
				
				if(EmailServiceSetting.SendingAllowed && Entity.NeedSendBill(emailRepository)) {
					bool sendEmail = true;
					var emailAddressForBill = Entity.GetEmailAddressForBill();
					if(emailAddressForBill == null) {
						sendEmail = false;
						if(!MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?")) {
							return false;
						}
					}
					Entity.SaveEntity(UoWGeneric);
					if(sendEmail)
						SendBillByEmail(emailAddressForBill);
				} else {
					Entity.SaveEntity(UoWGeneric);
				}
				logger.Info("Ok.");
				UpdateUIState();
				return true;
			} finally {
				SetSensetivity(true);
			}
		}

		protected void OnBtnSaveCommentClicked(object sender, EventArgs e)
		{
			Entity.SaveOrderComment();
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			AcceptOrder();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			isEditOrderClicked = true;
			EditOrder();
		}

		private void EditOrder()
		{
			if(!Entity.CanSetOrderAsEditable) {
				return;
			}
			Entity.EditOrder(CallTaskWorker);
			UpdateUIState();
		}

		private void AcceptOrder()
		{
			if(!Entity.CanSetOrderAsAccepted) {
				return;
			}

			var canContinue = Entity.DefaultWaterCheck(ServicesConfig.InteractiveService);
			if(canContinue.HasValue && !canContinue.Value) {
				toggleGoods.Activate();
				return;
			}

			if(!ValidateAndFormOrder() || !CheckCertificates(canSaveFromHere: true)) {
				return;
			}

			PromosetDuplicateFinder promosetDuplicateFinder = new PromosetDuplicateFinder(ServicesConfig.InteractiveService);
			List<Phone> phones = new List<Phone>();
			phones.AddRange(Entity.Client.Phones);
			if(Entity.DeliveryPoint != null) {
				phones.AddRange(Entity.DeliveryPoint.Phones);
			}
			if(Entity.OrderItems.Any(x => x.PromoSet != null)) {
				if(!promosetDuplicateFinder.RequestDuplicatePromosets(UoW, Entity.DeliveryPoint, phones)) {
					return;
				}
			}

			if(Contract == null && !Entity.IsLoadedFrom1C) {
				Entity.Contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
				if(Entity.Contract == null)
					Entity.CreateDefaultContract();
			}

			Entity.AcceptOrder(employeeRepository.GetEmployeeForCurrentUser(UoW), CallTaskWorker);

			treeItems.Selection.UnselectAll();
			Save();
			ProcessSmsNotification();
			UpdateUIState();
		}

		private void ProcessSmsNotification()
		{
			SmsNotifier smsNotifier = new SmsNotifier(new BaseParametersProvider());
 			smsNotifier.NotifyIfNewClient(Entity);
		}

		private bool ValidateAndFormOrder()
		{
			Entity.CheckAndSetOrderIsService();

			var valid = new QSValidator<Order>(
				Entity, new Dictionary<object, object>{
					{ "NewStatus", OrderStatus.Accepted },
					{ "IsCopiedFromUndelivery", templateOrder != null } //индикатор того, что заказ - копия, созданная из недовозов
				}
			);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return false;

			if(Entity.DeliveryPoint != null && !Entity.DeliveryPoint.CalculateDistricts(UoW).Any())
				MessageDialogHelper.RunWarningDialog("Точка доставки не попадает ни в один из наших районов доставки. Пожалуйста, согласуйте стоимость доставки с руководителем и клиентом.");

			OnFormOrderActions();
			return true;
		}

		/// <summary>
		/// Действия обрабатываемые при формировании заказа
		/// </summary>
		private void OnFormOrderActions()
		{
			//проверка и добавление платной доставки в товары
			Entity.CalculateDeliveryPrice();
		}

		/// <summary>
		/// Ручное закрытие заказа
		/// </summary>
		protected void OnButtonCloseOrderClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.Accepted && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_orders")) {
				if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите закрыть заказ?")) {
					return;
				}
				IStandartNomenclatures standartNomenclatures = new BaseParametersProvider();
				Entity.UpdateBottlesMovementOperationWithoutDelivery(UoW, standartNomenclatures, new EntityRepositories.Logistic.RouteListItemRepository(), new CashRepository());
				Entity.UpdateDepositOperations(UoW);

				Entity.ChangeStatusAndCreateTasks(OrderStatus.Closed, CallTaskWorker);
				foreach(OrderItem i in Entity.ObservableOrderItems) {
					i.ActualCount = i.Count;
				}
			}
			UpdateUIState();
		}
		/// <summary>
		/// Возврат в принят из ручного закрытия
		/// </summary>
		protected void OnButtonReturnToAcceptedClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted) {
				if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите вернуть заказ в статус \"Принят\"?")) {
					return;
				}
				Entity.ChangeStatusAndCreateTasks(OrderStatus.Accepted, CallTaskWorker);
			}
			UpdateUIState();
		}


		/// <summary>
		/// Отправка самовывоза на погрузку
		/// </summary>
		protected void OnButtonSelfDeliveryToLoadingClicked(object sender, EventArgs e)
		{
			Entity.SelfDeliveryToLoading(ServicesConfig.CommonServices.CurrentPermissionService, CallTaskWorker);
			UpdateUIState();
		}

		/// <summary>
		/// Принятие оплаты самовывоза
		/// </summary>
		protected void OnButtonSelfDeliveryAcceptPaidClicked(object sender, EventArgs e)
		{
			Entity.SelfDeliveryAcceptCashlessPaid(CallTaskWorker);
			UpdateUIState();
		}

		#endregion

		#region Документы заказа

		protected void OnBtnRemExistingDocumentClicked(object sender, EventArgs e)
		{
			if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите удалить выделенные документы?")) return;
			var documents = treeDocuments.GetSelectedObjects<OrderDocument>();
			var notDeletedDocs = Entity.RemoveAdditionalDocuments(documents);
			if(notDeletedDocs != null && notDeletedDocs.Any()) {
				string strDocuments = "";
				foreach(OrderDocument doc in notDeletedDocs) {
					strDocuments += string.Format("\n\t{0}", doc.Name);
				}
				MessageDialogHelper.RunWarningDialog(string.Format("Документы{0}\nудалены не были, так как относятся к текущему заказу.", strDocuments));
			}
		}

		protected void OnBtnAddM2ProxyForThisOrderClicked(object sender, EventArgs e)
		{
			if(!new QSValidator<Order>(
				Entity, new Dictionary<object, object>{
					{ "IsCopiedFromUndelivery", templateOrder != null } //индикатор того, что заказ - копия, созданная из недовозов
				}
			).RunDlgIfNotValid((Window)this.Toplevel)
			   && SaveOrderBeforeContinue<M2ProxyDocument>()) {
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<M2ProxyDocument>(0),
					() => OrmMain.CreateObjectDialog(typeof(M2ProxyDocument), EntityUoWBuilder.ForCreateInChildUoW(UoW), UnitOfWorkFactory.GetDefaultFactory)
				);
			}
		}

		protected void OnButtonAddExistingDocumentClicked(object sender, EventArgs e)
		{
			if(Entity.Client == null) {
				MessageDialogHelper.RunWarningDialog("Для добавления дополнительных документов должен быть выбран клиент.");
				return;
			}

			TabParent.OpenTab(
				TdiTabBase.GenerateHashName<AddExistingDocumentsDlg>(),
				() => new AddExistingDocumentsDlg(UoWGeneric, Entity.Client)
			);
		}

		protected void OnButtonViewDocumentClicked(object sender, EventArgs e)
		{
			OrderDocumentsOpener();
		}

		/// <summary>
		/// Открытие соответствующего документу заказа окна.
		/// </summary>
		void OrderDocumentsOpener()
		{
			if(!treeDocuments.GetSelectedObjects().Any())
				return;

			var rdlDocs = treeDocuments.GetSelectedObjects()
									   .Cast<OrderDocument>()
									   .Where(d => d.PrintType == PrinterType.RDL)
									   .ToList();

			if(rdlDocs.Any()) {
				string whatToPrint = rdlDocs.ToList().Count > 1
											? "документов"
											: "документа \"" + rdlDocs.Cast<OrderDocument>().First().Type.GetEnumTitle() + "\"";
				if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Order), whatToPrint))
					UoWGeneric.Save();
				rdlDocs.ForEach(
					doc => {
						if(doc is IPrintableRDLDocument)
							TabParent.AddTab(DocumentPrinter.GetPreviewTab(doc as IPrintableRDLDocument), this, false);
					}
				);
			}

			var odtDocs = treeDocuments.GetSelectedObjects()
									   .Cast<OrderDocument>()
									   .Where(d => d.PrintType == PrinterType.ODT)
									   .ToList();
			if(odtDocs.Any())
				foreach(var doc in odtDocs) {
					if(doc is OrderAgreement) {
						var agreement = (doc as OrderAgreement).AdditionalAgreement;
						var type = NHibernateProxyHelper.GuessClass(agreement);
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName(type, agreement.Id),
							() => {
								var dialog = OrmMain.CreateObjectDialog(type, EntityUoWBuilder.ForOpenInChildUoW(agreement.Id, UoW), UnitOfWorkFactory.GetDefaultFactory);
								if(dialog is IAgreementSaved)
									(dialog as IAgreementSaved).AgreementSaved += AgreementSaved;
								return dialog;
							}
						);
					} else if(doc is OrderContract)
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<CounterpartyContract>((doc as OrderContract).Contract.Id),
							() => {
								var dialog = OrmMain.CreateObjectDialog((doc as OrderContract).Contract);
								if(dialog != null)
									(dialog as IEditableDialog).IsEditable = false;
								return dialog;
							}
						);
					else if(doc is OrderM2Proxy) {
						if(doc.Id == 0) {
							MessageDialogHelper.RunInfoDialog("Перед просмотром документа необходимо сохранить заказ");
							return;
						}
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<M2ProxyDocument>((doc as OrderM2Proxy).M2Proxy.Id),
							() => {
								var dialog = OrmMain.CreateObjectDialog((doc as OrderM2Proxy).M2Proxy);
								if(dialog != null)
									(dialog as IEditableDialog).IsEditable = false;
								return dialog;
							}
						);
					}
				}
		}

		/// <summary>
		/// Распечатать документы.
		/// </summary>
		/// <param name="docList">Лист документов.</param>
		private void PrintDocuments(IList<OrderDocument> docList)
		{
			if(docList.Any()) {
				new DocumentPrinter().PrintAll(docList);
			}
		}

		/// <summary>
		/// Добавление сертификатов, проверка наличия актуального и, в случае провала при проверке, выдача предупреждения.
		/// </summary>
		/// <returns><c>true</c>, если все сертификаты актуальны, либо не найден ни один,
		/// <c>false</c> если все найденные сертификаты архивные, либо просрочены</returns>
		/// <param name="canSaveFromHere"><c>true</c>Если вызывается проверка при сохранении заказа 
		/// и <c>false</c> если после проверки нужно только предупреждение</param>
		bool CheckCertificates(bool canSaveFromHere = false)
		{
			Entity.UpdateCertificates(out List<Nomenclature> needUpdateCertificatesFor);
			if(needUpdateCertificatesFor.Any()) {
				string msg = "Для следующих номенклатур устарели сертификаты продукции и добавлены в список документов не были:\n\n";
				msg += string.Join(
					"\t*",
					needUpdateCertificatesFor.Select(
						n => string.Format(
							" - {0} (код номенклатуры: {1})",
							n.Name,
							n.Id
						)
					)
				);
				msg += "\n\nПожалуйста обновите.";
				ButtonsType btns = ButtonsType.Ok;
				if(canSaveFromHere) {
					msg += "\nПродолжить сохранение заказа?";
					btns = ButtonsType.YesNo;
				}
				return MessageDialogHelper.RunWarningDialog("Сертификаты не добавлены", msg, btns);
			}
			return true;
		}

		#endregion

		#region Toggle buttons

		protected void OnToggleInformationToggled(object sender, EventArgs e)
		{
			if(toggleInformation.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnToggleCommentsToggled(object sender, EventArgs e)
		{
			if(toggleComments.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnToggleTareControlToggled(object sender, EventArgs e)
		{
			if(toggleTareControl.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnToggleGoodsToggled(object sender, EventArgs e)
		{
			if(toggleGoods.Active)
				notebook1.CurrentPage = 3;
		}

		protected void OnToggleEquipmentToggled(object sender, EventArgs e)
		{
			if(toggleEquipment.Active)
				notebook1.CurrentPage = 4;
		}

		protected void OnToggleServiceToggled(object sender, EventArgs e)
		{
			if(toggleService.Active)
				notebook1.CurrentPage = 5;
		}

		protected void OnToggleDocumentsToggled(object sender, EventArgs e)
		{
			if(toggleDocuments.Active)
				notebook1.CurrentPage = 6;
			btnOpnPrnDlg.Sensitive = Entity.OrderDocuments.Any(doc => doc.PrintType == PrinterType.RDL
															   || doc.PrintType == PrinterType.ODT);
		}

		#endregion

		#region Сервисный ремонт

		protected void OnTreeServiceClaimRowActivated(object o, RowActivatedArgs args)
		{
			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null)
				return;

			ServiceClaimDlg dlg = new ServiceClaimDlg((treeServiceClaim.GetSelectedObjects()[0] as ServiceClaim).Id);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonAddServiceClaimClicked(object sender, EventArgs e)
		{
			if(!SaveOrderBeforeContinue<ServiceClaim>())
				return;
			var dlg = new ServiceClaimDlg(Entity);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonAddDoneServiceClicked(object sender, EventArgs e)
		{
			if(!SaveOrderBeforeContinue<ServiceClaim>())
				return;
			OrmReference SelectDialog = new OrmReference(
				typeof(ServiceClaim),
				UoWGeneric,
				ServiceClaimRepository
					.GetDoneClaimsForClient(Entity)
					.GetExecutableQueryOver(UoWGeneric.Session)
					.RootCriteria
			) {
				Mode = OrmReferenceMode.Select,
				ButtonMode = ReferenceButtonMode.CanEdit
			};
			SelectDialog.ObjectSelected += DoneServiceSelected;

			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void DoneServiceSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			ServiceClaim selected = (e.Subject as ServiceClaim);
			var contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(
							   UoWGeneric,
							   Entity.Client,
							   Entity.Client.PersonType,
							   Entity.PaymentType);
			selected.FinalOrder = Entity;
			Entity.ObservableFinalOrderService.Add(selected);
			//TODO Add service nomenclature with price.
		}

		void TreeServiceClaim_Selection_Changed(object sender, EventArgs e)
		{
			buttonOpenServiceClaim.Sensitive = treeServiceClaim.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonOpenServiceClaimClicked(object sender, EventArgs e)
		{
			var claim = treeServiceClaim.GetSelectedObject<ServiceClaim>();
			OpenTab(
				EntityDialogBase<ServiceClaim>.GenerateHashName(claim.Id),
				() => new ServiceClaimDlg(claim)
			);
		}

		#endregion

		#region Добавление номенклатур

		void YCmbPromoSets_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if(!(e.SelectedItem is PromotionalSet proSet))
				return;

			if(CanAddNomenclaturesToOrder() && Entity.CanAddPromotionalSet(proSet))
				ActivatePromotionalSet(proSet);
			if(!yCmbPromoSets.IsSelectedNot)
				yCmbPromoSets.SelectedItem = SpecialComboState.Not;
		}

		bool CanAddNomenclaturesToOrder()
		{
			if(Entity.Client == null) {
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return false;
			}

			if(Entity.DeliveryPoint == null && !Entity.SelfDelivery) {
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должна быть выбрана точка доставки.");
				return false;
			}

			if(Entity.DeliveryDate == null) {
				MessageDialogHelper.RunWarningDialog("Введите дату доставки");
				return false;
			}
			return true;
		}

		protected void OnButtonAddMasterClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = new [] { NomenclatureCategory.master },
				x => x.RestrictCategory = NomenclatureCategory.master,
				x => x.RestrictArchive = false
			);

			NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				employeeService,
				NomenclatureSelectorFactory,
				CounterpartySelectorFactory,
				NomenclatureRepository,
				userRepository
			) {
				SelectionMode = JournalSelectionMode.Single,
			};
			journalViewModel.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
			journalViewModel.TabName = "Выезд мастера";
			journalViewModel.OnEntitySelectedResult += (s, ea) => {
				var selectedNode = ea.SelectedNodes.FirstOrDefault();
				if(selectedNode == null)
					return;
				TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
			};
			this.TabParent.AddSlaveTab(this, journalViewModel);
		}

		protected void OnButtonAddForSaleClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var defaultCategory = NomenclatureCategory.water;
			if(CurrentUserSettings.Settings.DefaultSaleCategory.HasValue)
				defaultCategory = CurrentUserSettings.Settings.DefaultSaleCategory.Value;

			var nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
				x => x.SelectCategory = defaultCategory,
				x => x.SelectSaleCategory = SaleCategory.forSale,
				x => x.RestrictArchive = false
			);

			NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				employeeService,
				NomenclatureSelectorFactory,
				CounterpartySelectorFactory,
				NomenclatureRepository,
				userRepository
			) {
				SelectionMode = JournalSelectionMode.Single,
			};
			journalViewModel.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
			journalViewModel.TabName = "Номенклатура на продажу";
			journalViewModel.OnEntitySelectedResult += (s, ea) => {
				var selectedNode = ea.SelectedNodes.FirstOrDefault();
				if(selectedNode == null)
					return;
				TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
			};
			this.TabParent.AddSlaveTab(this, journalViewModel);
		}

		#region Рекламные наборы

		void ActivatePromotionalSet(PromotionalSet proSet)
		{
			//Добавление спец. действий промо-набора
			foreach(var action in proSet.PromotionalSetActions) {
				action.Activate(Entity);
			}
			//Добавление номенклатур из промо-набора
			TryAddNomenclatureFromPromoSet(proSet);

			Entity.ObservablePromotionalSets.Add(proSet);
		}

		#endregion

		void NomenclatureSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			TryAddNomenclature(e.Subject as Nomenclature);
		}

		void TryAddNomenclature(Nomenclature nomenclature, decimal count = 0, decimal discount = 0, DiscountReason discountReason = null)
		{
			if(Entity.IsLoadedFrom1C)
				return;

			if(Entity.OrderItems.Any(x => !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
			   && nomenclature.Category == NomenclatureCategory.master) {
				MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
				return;
			}

			if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
			   && !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category)) {
				MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}
			if(nomenclature.ProductGroup != null)
				if(nomenclature.ProductGroup.IsOnlineStore && !ServicesConfig.CommonServices.CurrentPermissionService
					.ValidatePresetPermission("can_add_online_store_nomenclatures_to_order")) {
					MessageDialogHelper.RunWarningDialog("У вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
					return;
				}

			Entity.AddNomenclature(nomenclature, count, discount, false, discountReason);
		}

		void TryAddNomenclatureFromPromoSet(PromotionalSet proSet)
		{
			if(Entity.IsLoadedFrom1C)
				return;

			if (proSet != null && !proSet.IsArchive && proSet.PromotionalSetItems.Any()) {
				foreach (var proSetItem in proSet.PromotionalSetItems) {
					var nomenclature = proSetItem.Nomenclature;
					if (Entity.OrderItems.Any(x =>
						    !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
					    && nomenclature.Category == NomenclatureCategory.master) {
						MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
						return;
					}

					if (Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
					    && !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category)) {
						MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
						return;
					}

					Entity.AddNomenclature(
						proSetItem.Nomenclature,
						proSetItem.Count,
						proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
						proSetItem.IsDiscountInMoney,
						proSetItem.PromoSet.PromoSetDiscountReason,
						proSetItem.PromoSet
					);
				}

				OnFormOrderActions();
			}
		}

		private void AddRentAgreement(OrderAgreementType type)
		{
			if(Entity.IsLoadedFrom1C) {
				return;
			}

			if(Entity.Client == null || (Entity.DeliveryPoint == null && !Entity.SelfDelivery)) {
				MessageDialogHelper.RunWarningDialog("Для добавления оборудования должна быть выбрана точка доставки.");
				return;
			}

			if(Entity.ObservableOrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)) {
				MessageDialogHelper.RunWarningDialog("Нельзя добавлять аренду в сервисный заказ");
				return;
			}

			if(Entity.Contract == null) {
				Entity.Contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
			}
			if(Contract == null) {
				switch(type) {
					case OrderAgreementType.NonfreeRent:
						lastChosenAction = LastChosenAction.NonFreeRentAgreement;
						break;
					default:
						lastChosenAction = LastChosenAction.FreeRentAgreement;
						break;
				}
				RunContractCreateDialog(type);
				Entity.Contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
				if(Entity.Contract == null) {
					return;
				}
			}
			CreateRentAgreementDialogs(Entity.Contract, type);
		}

		protected void OnButtonbuttonAddEquipmentToClientClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.equipment
			);
			PermissionControlledRepresentationJournal SelectDialog = new PermissionControlledRepresentationJournal(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter)) {
				Mode = JournalSelectMode.Single,
				ShowFilter = true
			};
			SelectDialog.CustomTabName("Оборудование к клиенту");
			SelectDialog.ObjectSelected += NomenclatureToClient;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureToClient(object sender, JournalObjectSelectedEventArgs e)
		{
			var selectedId = e.GetSelectedIds().FirstOrDefault();
			if(selectedId == 0) {
				return;
			}
			AddNomenclatureToClient(UoWGeneric.Session.Get<Nomenclature>(selectedId));
		}

		void AddNomenclatureToClient(Nomenclature nomenclature)
		{
			Entity.AddEquipmentNomenclatureToClient(nomenclature, UoWGeneric);
		}

		protected void OnButtonAddEquipmentFromClientClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.equipment
			);
			PermissionControlledRepresentationJournal SelectDialog = new PermissionControlledRepresentationJournal(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter)) {
				Mode = JournalSelectMode.Single,
				ShowFilter = true
			};
			SelectDialog.CustomTabName("Оборудование от клиента");
			SelectDialog.ObjectSelected += NomenclatureFromClient;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureFromClient(object sender, JournalObjectSelectedEventArgs e)
		{
			var selectedId = e.GetSelectedIds().FirstOrDefault();
			if(selectedId == 0) {
				return;
			}
			AddNomenclatureFromClient(UoWGeneric.Session.Get<Nomenclature>(selectedId));
		}

		void AddNomenclatureFromClient(Nomenclature nomenclature)
		{
			Entity.AddEquipmentNomenclatureFromClient(nomenclature, UoWGeneric);
		}

		public void FillOrderItems(Order order)
		{
			if(Entity.OrderStatus != OrderStatus.NewOrder
			   || Entity.ObservableOrderItems.Any() && !MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите удалить все позиции текущего из заказа и заполнить его позициями из выбранного?")) {
				return;
			}

			Entity.ClearOrderItemsList();
			foreach(OrderItem orderItem in order.OrderItems) {
				switch(orderItem.Nomenclature.Category) {
					case NomenclatureCategory.additional:
						Entity.AddNomenclatureForSaleFromPreviousOrder(orderItem, UoWGeneric);
						continue;
					case NomenclatureCategory.water:
						TryAddNomenclature(orderItem.Nomenclature, orderItem.Count);
						continue;
					default:
						//Entity.AddAnyGoodsNomenclatureForSaleFromPreviousOrder(orderItem);
						continue;
				}
			}
		}
		#endregion

		#region Удаление номенклатур

		private void RemoveOrderItem(OrderItem item)
		{
			var types = new AgreementType[] {
					AgreementType.FreeRent,
					AgreementType.NonfreeRent
				};
			if(item.AdditionalAgreement != null && types.Contains(item.AdditionalAgreement.Type)) {
				RemoveAgreementBeingCreateForEachAdding(item);
			} else {
				Entity.RemoveItem(item);
			}
		}

		/// <summary>
		/// Удаляет доп соглашения которые создаются на каждое добавление в товарах.
		/// </summary>
		public virtual void RemoveAgreementBeingCreateForEachAdding(OrderItem item)
		{
			if(item.AdditionalAgreement == null) {
				return;
			}

			var agreement = item.AdditionalAgreement.Self;

			var deletedOrderItems = Entity.ObservableOrderItems.Where(x => x.AdditionalAgreement != null
																			&& x.AdditionalAgreement.Self == agreement)
															   .ToList();
			var deletedOrderDocuments = Entity.ObservableOrderDocuments.OfType<OrderAgreement>()
																	   .Where(x => x.AdditionalAgreement != null
																					&& x.AdditionalAgreement.Self == agreement)
																	   .ToList();

			if(Entity.Id != 0) {
				var valid = new QSValidator<Order>(
					Entity, new Dictionary<object, object>{
						{ "IsCopiedFromUndelivery", templateOrder != null } //индикатор того, что заказ - копия, созданная из недовозов
					}
				);

				if(!MessageDialogHelper.RunQuestionDialog("Заказ будет сохранен после удаления товара, продолжить?")
				   || valid.RunDlgIfNotValid((Window)this.Toplevel)) {
					return;
				}

				Type agreementType = null;
				switch(agreement.Type) {
					case AgreementType.NonfreeRent:
						agreementType = typeof(NonfreeRentAgreement);
						break;
					case AgreementType.FreeRent:
						agreementType = typeof(FreeRentAgreement);
						break;
					default:
						return;
				}

				var deletionObjects = OrmMain.GetDeletionObjects(agreementType, agreement.Id, UoW);

				var delAgreement = deletionObjects.FirstOrDefault(x => x.Type == agreementType && x.Id == agreement.Id);
				if(delAgreement != null) {
					deletionObjects.Remove(delAgreement);
				}

				foreach(var oi in deletedOrderItems) {
					var delObject = deletionObjects.FirstOrDefault(x => x.Type == typeof(OrderItem) && x.Id == oi.Id);
					if(delObject != null) {
						deletionObjects.Remove(delObject);
					}
				}
				foreach(var od in deletedOrderDocuments) {
					var delObject = deletionObjects.FirstOrDefault(x => x.Type == typeof(OrderAgreement) && x.Id == od.Id);
					if(delObject != null) {
						deletionObjects.Remove(delObject);
					}
				}
				var autoDeletionTypes = new Type[] { typeof(PaidRentEquipment), typeof(FreeRentEquipment), typeof(SalesEquipment) };
				if(deletionObjects.Any(x => !autoDeletionTypes.Contains(x.Type))) {
					MessageDialogHelper.RunErrorDialog("Невозможно удалить дополнительное соглашение из-за связанных документов не относящихся к текущему заказу.");
					return;
				}
			}

			deletedOrderItems.ForEach(Entity.RemoveItem);
			var agreementProxy = Entity.Contract.AdditionalAgreements.FirstOrDefault(x => x.Id == agreement.Id);
			if(agreementProxy != null) {
				Entity.Contract.AdditionalAgreements.Remove(agreementProxy);
			}

			//Принудительно сохраняем только, уже сохраненный в базе, заказ, 
			//чтобы пользователь не смог вернуть товары связанные с не существующем доп соглашением, 
			//отменив сохранение заказа
			if(Entity.Id != 0) {
				UoW.Delete(agreement);
				UoW.Save();
				UoW.Commit();
			} else {
				using(var deletionUoW = UnitOfWorkFactory.CreateWithoutRoot()) {
					var deletedAgreement = deletionUoW.GetById<AdditionalAgreement>(agreement.Id);
					deletionUoW.Delete(deletedAgreement);
					deletionUoW.Commit();
				}
			}
			Entity.UpdateDocuments();
		}

		void OrderEquipmentItemsView_OnDeleteEquipment(object sender, OrderEquipment e)
		{
			if(e.OrderItem != null) {
				RemoveOrderItem(e.OrderItem);
			} else {
				Entity.RemoveEquipment(e);
			}
		}

		protected void OnBtnDeleteOrderItemClicked(object sender, EventArgs e)
		{
			if(treeItems.GetSelectedObject() is OrderItem orderItem) {
				RemoveOrderItem(orderItem);
				Entity.TryToRemovePromotionalSet(orderItem);
				//при удалении номенклатуры выделение снимается и при последующем удалении exception
				//для исправления делаем кнопку удаления не активной, если объект не выделился в списке
				btnDeleteOrderItem.Sensitive = treeItems.GetSelectedObject() != null;
			}
		}
		#endregion

		#region Создание договоров, доп соглашений

		protected void CreateContractWithAgreement(Nomenclature nomenclature, int count)
		{
			var contract = GetActualInstanceContract(ClientDocumentsRepository.CreateDefaultContract(UoW, Entity.Client, Entity.PaymentType, Entity.DeliveryDate));
			Entity.Contract = contract;
			Entity.AddContractDocument(contract);
			AdditionalAgreement agreement = contract.GetWaterSalesAgreement(Entity.DeliveryPoint, nomenclature);
			if(agreement == null) {
				agreement = ClientDocumentsRepository.CreateDefaultWaterAgreement(UoW, Entity.DeliveryPoint, Entity.DeliveryDate, contract);
				contract.AdditionalAgreements.Add(agreement);
				Entity.CreateOrderAgreementDocument(agreement);
				TryAddNomenclature(nomenclature, count);
			}
		}

		CounterpartyContract GetActualInstanceContract(CounterpartyContract anotherSessionContract)
		{
			return UoW.GetById<CounterpartyContract>(anotherSessionContract.Id);
		}

		protected void OnReferenceContractChanged(object sender, EventArgs e)
		{
			OnReferenceDeliveryPointChanged(sender, e);
		}

		protected void OnYcomboboxReasonItemSelected(object sender, ItemSelectedEventArgs e)
		{
			SetDiscountUnitEditable();
		}

		void CreateRentAgreementDialogs(CounterpartyContract contract, OrderAgreementType type)
		{
			if(contract == null) {
				return;
			}
			ITdiDialog dlg = null;
			OrmReference refWin;
			switch(type) {
				case OrderAgreementType.NonfreeRent:
					refWin = new OrmReference(typeof(PaidRentPackage)) {
						Mode = OrmReferenceMode.Select
					};
					refWin.ObjectSelected += (sender, e) => {
						dlg = new NonFreeRentAgreementDlg(contract, Entity.DeliveryPoint, Entity.DeliveryDate, (e.Subject as PaidRentPackage));
						RunAgreementDialog(dlg);
					};
					TabParent.AddTab(refWin, this);
					break;
				case OrderAgreementType.FreeRent:
					refWin = new OrmReference(typeof(FreeRentPackage)) {
						Mode = OrmReferenceMode.Select
					};
					refWin.ObjectSelected += (sender, e) => {
						dlg = new FreeRentAgreementDlg(contract, Entity.DeliveryPoint, Entity.DeliveryDate, (e.Subject as FreeRentPackage));
						RunAgreementDialog(dlg);
					};
					TabParent.AddTab(refWin, this);
					break;
			}
		}

		void RunAgreementDialog(ITdiDialog dlg)
		{
			(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
			TabParent.AddSlaveTab(this, dlg);
		}

		void AgreementSaved(object sender, AgreementSavedEventArgs e)
		{
			var agreement = UoWGeneric.GetById<AdditionalAgreement>(e.Agreement.Id);

			Entity.CreateOrderAgreementDocument(agreement);
			Entity.FillItemsFromAgreement(agreement);
			/*CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType)
										  .AdditionalAgreements
										  .Add(agreement);*/
			if(!Entity.Contract.ObservableAdditionalAgreements.Contains(agreement))
				Entity.Contract.ObservableAdditionalAgreements.Add(agreement);

			var waterSalesAgreementList = Contract.AdditionalAgreements
									   .Where(x => !x.IsCancelled)
									   .Select(x => x.Self)
									   .OfType<WaterSalesAgreement>();
		}

		void RunContractCreateDialog(OrderAgreementType type)
		{
			ITdiTab dlg;
			var response = AskCreateContract();
			if(response == (int)ResponseType.Yes) {
				dlg = new CounterpartyContractDlg(Entity.Client, Entity.PaymentType,
					OrganizationRepository.GetOrganizationByPaymentType(UoWGeneric, Entity.Client.PersonType, Entity.PaymentType),
					Entity.DeliveryDate);
				(dlg as IContractSaved).ContractSaved += (sender, e) => {
					OnContractSaved(sender, e);
					var contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
					CreateRentAgreementDialogs(contract, type);
				};
				TabParent.AddSlaveTab(this, dlg);
			} else if(response == (int)ResponseType.Accept) {
				var contract = GetActualInstanceContract(ClientDocumentsRepository.CreateDefaultContract(UoW, Entity.Client, Entity.PaymentType, Entity.DeliveryDate));
				Entity.AddContractDocument(contract);
				Entity.Contract = contract;
			}
		}

		protected int AskCreateContract()
		{
			MessageDialog md = new MessageDialog(
				null,
				DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.YesNo,
				$"Отсутствует договор с клиентом для формы оплаты '{Entity.PaymentType.GetEnumTitle()}'. Создать?"
			);
			md.SetPosition(WindowPosition.Center);
			md.AddButton("Автоматически", ResponseType.Accept);
			md.ShowAll();
			//var result = md.Run();
			md.Destroy();
			//TODO Временно сделан выбор создания договора автоматически. 
			//Если не понадобится возвращатся к выбору создания договора, убрать 
			//диалог и проверить создание диалогов для доп соглашений которые должны 
			//будут запускаться после создания договора
			return (int)ResponseType.Accept;
		}

		protected void RunContractAndWaterAgreementDialog(Nomenclature nomenclature, int count = 0)
		{
			ITdiTab dlg = new CounterpartyContractDlg(Entity.Client, Entity.PaymentType,
							  OrganizationRepository.GetOrganizationByPaymentType(UoWGeneric, Entity.Client.PersonType, Entity.PaymentType),
							  Entity.DeliveryDate);
			(dlg as IContractSaved).ContractSaved += OnContractSaved;
			dlg.TabClosed += (sender, e) => {
				CounterpartyContract contract =
					CounterpartyContractRepository.GetCounterpartyContractByPaymentType(
						UoWGeneric,
						Entity.Client,
						Entity.Client.PersonType,
						Entity.PaymentType);
				if(contract != null) {
					bool hasWaterAgreement = contract.GetWaterSalesAgreement(Entity.DeliveryPoint, nomenclature) != null;
					if(!hasWaterAgreement)
						RunAdditionalAgreementWaterDialog(nomenclature, count);
				}
			};
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnContractSaved(object sender, ContractSavedEventArgs args)
		{
			CounterpartyContract contract =
					CounterpartyContractRepository.GetCounterpartyContractByPaymentType(
						UoWGeneric,
						Entity.Client,
						Entity.Client.PersonType,
						Entity.PaymentType);
			Entity.ObservableOrderDocuments.Add(new OrderContract {
				Order = Entity,
				AttachedToOrder = Entity,
				Contract = contract
			});

			Entity.Contract = contract;
		}

		protected void RunAdditionalAgreementWaterDialog(Nomenclature nom = null, int count = 0)
		{
			ITdiDialog dlg = new WaterAgreementDlg(CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType), Entity.DeliveryPoint, Entity.DeliveryDate);
			(dlg as IAgreementSaved).AgreementSaved +=
				(sender, e) => {
					AgreementSaved(sender, e);
					if(nom != null) {
						TryAddNomenclature(nom, count);
					}
				};
			TabParent.AddSlaveTab(this, dlg);
		}
		#endregion

		#region Изменение диалога

		/// <summary>
		/// Ширина первой колонки списка товаров или оборудования
		/// (создано для хранения ширины колонки до автосайза ячейки по 
		/// содержимому, чтобы отобразить по правильному положению ввод 
		/// количества при добавлении нового товара)
		/// </summary>
		int treeAnyGoodsFirstColWidth;

		/// <summary>
		/// Активирует редактирование ячейки количества
		/// </summary>
		private void EditGoodsCountCellOnAdd(yTreeView treeView)
		{
			try {
				int index = treeView.Model.IterNChildren() - 1;
				TreePath path;

				treeView.Model.IterNthChild(out TreeIter iter, index);
				path = treeView.Model.GetPath(iter);

				var column = treeView.ColumnsConfig.ConfiguredColumns.FirstOrDefault(x => (x.tag as string) == "Count")?.TreeViewColumn;
				if(column == null) {
					return;
				}
				var renderer = column.CellRenderers.First();
				Application.Invoke(delegate {
					treeView.SetCursorOnCell(path, column, renderer, true);
				});
				treeView.GrabFocus();
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка при попытке установки состояния редактирования на ячейку");
				return;
			}

		}

		void TreeAnyGoods_ExposeEvent(object o, ExposeEventArgs args)
		{
			var newColWidth = ((yTreeView)o).Columns.First().Width;
			if(treeAnyGoodsFirstColWidth != newColWidth) {
				EditGoodsCountCellOnAdd((yTreeView)o);
				((yTreeView)o).ExposeEvent -= TreeAnyGoods_ExposeEvent;
			}
		}

		#endregion

		#region Методы событий виджетов

		void PickerDeliveryDate_DateChanged(object sender, EventArgs e)
		{
			if(pickerDeliveryDate.Date < DateTime.Today && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_can_create_order_in_advance"))
				pickerDeliveryDate.ModifyBase(StateType.Normal, new Gdk.Color(255, 0, 0));
			else
				pickerDeliveryDate.ModifyBase(StateType.Normal, new Gdk.Color(255, 255, 255));
			
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		protected void OnEntityVMEntryClientChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(entityVMEntryClient.Subject));
			if(Entity.Client != null) {
				referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, Entity.Client);
				referenceDeliveryPoint.Sensitive = Entity.OrderStatus == OrderStatus.NewOrder;

				PaymentType? previousPaymentType = enumPaymentType.SelectedItem as PaymentType?;

				Enum[] hideEnums = { PaymentType.cashless };

				if(Entity.Client.PersonType == PersonType.natural) {
					chkContractCloser.Active = false;
					chkContractCloser.Visible = false;
					enumPaymentType.AddEnumToHideList(hideEnums);
				} else {
					chkContractCloser.Visible = true;
					enumPaymentType.ClearEnumHideList();
				}

				var promoSets = UoW.Session.QueryOver<PromotionalSet>().Where(s => !s.IsArchive).List();
				yCmbPromoSets.ItemsList = promoSets.Where(s => s.IsValidForOrder(Entity));

				if(previousPaymentType.HasValue) {
					if(previousPaymentType.Value == Entity.PaymentType) {
						enumPaymentType.SelectedItem = previousPaymentType.Value;
					} else if(Entity.Id == 0 || hideEnums.Contains(Entity.PaymentType)) {
						enumPaymentType.SelectedItem = Entity.Client.PaymentMethod;
						OnEnumPaymentTypeChanged(null, e);
						Entity.ChangeOrderContract();
					} else {
						enumPaymentType.SelectedItem = Entity.PaymentType;
					}
				}

				enumTax.SelectedItem = Entity.Client.TaxType;
				enumTax.Visible = lblTax.Visible = IsEnumTaxVisible();
			} else {
				referenceDeliveryPoint.Sensitive = false;
			}
			Entity.SetProxyForOrder();
			UpdateProxyInfo();

			SetSensitivityOfPaymentType();
		}
		
		private bool IsEnumTaxVisible() => Entity.Client != null &&
                                           (!Entity.CreateDate.HasValue || Entity.CreateDate > date) &&
                                           Entity.Client.PersonType == PersonType.legal &&
                                           Entity.Client.TaxType == TaxType.None;

		protected void OnButtonFillCommentClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(CommentTemplate), UoWGeneric) {
				Mode = OrmReferenceMode.Select,
				ButtonMode = ReferenceButtonMode.CanAdd
			};
			SelectDialog.ObjectSelected += (s, ea) => {
				if(ea.Subject != null) {
					Entity.Comment = (ea.Subject as CommentTemplate).Comment;
				}
			};
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		protected void OnSpinSumDifferenceValueChanged(object sender, EventArgs e)
		{
			string text;
			if(spinSumDifference.Value > 0)
				text = "Сумма <b>переплаты</b>/недоплаты:";
			else if(spinSumDifference.Value < 0)
				text = "Сумма переплаты/<b>недоплаты</b>:";
			else
				text = "Сумма переплаты/недоплаты:";
			labelSumDifference.Markup = text;
		}

		protected void OnEnumSignatureTypeChanged(object sender, EventArgs e)
		{
			UpdateProxyInfo();
		}

		protected void OnReferenceDeliveryPointChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(referenceDeliveryPoint.Subject));
			if(Entity.DeliveryPoint != null) {
				UpdateProxyInfo();
				Entity.SetProxyForOrder();
			}
			
			if(Entity.DeliveryDate.HasValue && Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		protected void OnReferenceDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			if(!HasAgreementForDeliveryPoint()) {
				Order originalOrder = UoW.GetById<Order>(Entity.Id);
				Entity.DeliveryPoint = originalOrder?.DeliveryPoint;
			}
			CheckSameOrders();
			Entity.ChangeOrderContract();
			Entity.ChangeWaterAgreementDeliveryPointChanged();
			
			if(Entity.DeliveryDate.HasValue && Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		protected void OnButtonPrintSelectedClicked(object c, EventArgs args)
		{
			try {
				SetSensetivity(false);
				var allList = treeDocuments.GetSelectedObjects().Cast<OrderDocument>().ToList();
				if(allList.Count <= 0)
					return;

				allList.OfType<ITemplateOdtDocument>().ToList().ForEach(x => x.PrepareTemplate(UoW));

				string whatToPrint = allList.Count > 1
					? "документов"
					: "документа \"" + allList.First().Type.GetEnumTitle() + "\"";
				if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Order), whatToPrint))
					UoWGeneric.Save();

				var selectedPrintableRDLDocuments = treeDocuments.GetSelectedObjects().Cast<OrderDocument>()
					.Where(doc => doc.PrintType == PrinterType.RDL).ToList();
				if(selectedPrintableRDLDocuments.Any()) {
					new DocumentPrinter().PrintAll(selectedPrintableRDLDocuments);
				}

				var selectedPrintableODTDocuments = treeDocuments.GetSelectedObjects()
					.OfType<IPrintableOdtDocument>().ToList();
				if(selectedPrintableODTDocuments.Any()) {
					TemplatePrinter.PrintAll(selectedPrintableODTDocuments);
				}
			} finally {
				SetSensetivity(true);
			}
		}

		protected void OnBtnOpnPrnDlgClicked(object sender, EventArgs e)
		{
			if(Entity.OrderDocuments.Any(doc => doc.PrintType == PrinterType.RDL || doc.PrintType == PrinterType.ODT))
				TabParent.AddSlaveTab(this, new DocumentsPrinterDlg(Entity));
		}

		protected void OnEnumPaymentTypeChanged(object sender, EventArgs e)
		{
			//при изменении типа платежа вкл/откл кнопку "ожидание оплаты"
			buttonWaitForPayment.Sensitive = IsPaymentTypeBarterOrCashless();

			//при изменении типа платежа вкл/откл кнопку "закрывашка по контракту"
			chkContractCloser.Visible = IsPaymentTypeCashless();

			checkDelivered.Visible = enumDocumentType.Visible = labelDocumentType.Visible = IsPaymentTypeCashless();
			
			if (Entity.PaymentType != PaymentType.cash) {
				ycheckPaymentBySms.Visible = ycheckPaymentBySms.Active = false;
			}
			else {
				ycheckPaymentBySms.Visible = true;
			}
			
			if (Entity.PaymentType == PaymentType.Terminal) {
				checkSelfDelivery.Visible = checkSelfDelivery.Active = false;
			}
			else {
				checkSelfDelivery.Visible = true;
			}

			enumSignatureType.Visible = labelSignatureType.Visible =
				(Entity.Client != null &&
				 (Entity.Client.PersonType == PersonType.legal || Entity.PaymentType == PaymentType.cashless)
				);
			
			hbxOnlineOrder.Visible = UpdateVisibilityHboxOnlineOrder();
			ySpecPaymentFrom.Visible = Entity.PaymentType == PaymentType.ByCard;
			
			if(treeItems.Columns.Any())
				treeItems.Columns.First(x => x.Title == "В т.ч. НДС").Visible = Entity.PaymentType == PaymentType.cashless;
			spinSumDifference.Visible = labelSumDifference.Visible = labelSumDifferenceReason.Visible =
				dataSumDifferenceReason.Visible = (Entity.PaymentType == PaymentType.cash || Entity.PaymentType == PaymentType.BeveragesWorld);
			spinSumDifference.Visible = spinSumDifference.Visible && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_order_extra_cash");
			pickerBillDate.Visible = labelBillDate.Visible = Entity.PaymentType == PaymentType.cashless;
			Entity.SetProxyForOrder();
			UpdateProxyInfo();
			UpdateUIState();
		}

		private bool UpdateVisibilityHboxOnlineOrder() {
			switch (Entity.PaymentType) {
				case PaymentType.ByCard:
					return true;
				case PaymentType.Terminal:
					return Entity.OnlineOrder != null;
				default:
					return false;
			}
		}

		protected void OnPickerDeliveryDateDateChanged(object sender, EventArgs e)
		{
			Entity.SetProxyForOrder();
			UpdateProxyInfo();
		}

		protected void OnPickerDeliveryDateDateChangedByUser(object sender, EventArgs e)
		{
			if(Entity.DeliveryDate.HasValue) {
				if(Entity.DeliveryDate.Value.Date != DateTime.Today.Date || MessageDialogHelper.RunWarningDialog("Подтвердите дату доставки", "Доставка сегодня? Вы уверены?", ButtonsType.YesNo)) {
					CheckSameOrders();
					Entity.ChangeOrderContract();
					return;
				}
				Entity.DeliveryDate = null;
			}
		}

		protected void OnEntityVMEntryClientChangedByUser(object sender, EventArgs e)
		{
			chkContractCloser.Active = false;
			if(Entity.Client != null && Entity.Client.IsDeliveriesClosed) {
				string message = "Стоп отгрузки!!!" + Environment.NewLine + "Комментарий от фин.отдела: " + Entity.Client?.CloseDeliveryComment;
				MessageDialogHelper.RunInfoDialog(message);
				Enum[] hideEnums = {
					PaymentType.barter,
					PaymentType.BeveragesWorld,
					PaymentType.ContractDoc,
					PaymentType.cashless
				};
				enumPaymentType.AddEnumToHideList(hideEnums);
			}
			
			Entity.UpdateClientDefaultParam();

			//Проверяем возможность добавления Акции "Бутыль"
			ControlsActionBottleAccessibility();
		}

		protected void OnButtonCancelOrderClicked(object sender, EventArgs e) {
			
			bool isShipped = !orderRepository.IsSelfDeliveryOrderWithoutShipment(UoW, Entity.Id);
			bool orderHasIncome = cashRepository.OrderHasIncome(UoW, Entity.Id);

			if (Entity.SelfDelivery && (orderHasIncome || isShipped)) {
				MessageDialogHelper.RunErrorDialog(
					"Вы не можете отменить отгруженный или оплаченный самовывоз. " +
					"Для продолжения необходимо удалить отгрузку или приходник.");
				return;
			}
			
			var valid = new QSValidator<Order>(Entity,
				new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.Canceled },
				{ "IsCopiedFromUndelivery", templateOrder != null } //индикатор того, что заказ - копия, созданная из недовозов
			});
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;

			OpenDlgToCreateNewUndeliveredOrder();
		}

		/// <summary>
		/// Открытие окна создания нового недовоза при отмене заказа
		/// </summary>
		void OpenDlgToCreateNewUndeliveredOrder()
		{
			UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(Entity, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (sender, e) => {
				Entity.SetUndeliveredStatus(UoW, new BaseParametersProvider(), CallTaskWorker);
				UpdateUIState();

				var routeListItem = routeListItemRepository.GetRouteListItemForOrder(UoW, Entity);
				if(routeListItem != null && routeListItem.Status != RouteListItemStatus.Canceled) {
					routeListItem.SetStatusWithoutOrderChange(RouteListItemStatus.Canceled);
					routeListItem.StatusLastUpdate = DateTime.Now;
					routeListItem.FillCountsOnCanceled();
					UoW.Save(routeListItem);
				}

				if(Save())
					this.OnCloseTab(false);
			};
		}

		protected void OnEnumPaymentTypeChangedByUser(object sender, EventArgs e)
		{
			Entity.ChangeOrderContract();
			if(Entity.PaymentType != PaymentType.ByCard)
				entOnlineOrder.Text = string.Empty;//костыль, т.к. Entity.OnlineOrder = null не убирает почему-то текст из виджета
		}

		protected void OnSpinDiscountValueChanged(object sender, EventArgs e)
		{
			SetDiscount();
		}

		protected void OnButtonWaitForPaymentClicked(object sender, EventArgs e)
		{
			var valid = new QSValidator<Order>(Entity,
				new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.WaitForPayment },
				{ "IsCopiedFromUndelivery", templateOrder != null } //индикатор того, что заказ - копия, созданная из недовозов
			});
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;

			Entity.ChangeStatusAndCreateTasks(OrderStatus.WaitForPayment, CallTaskWorker);
			UpdateUIState();
		}

		protected void OnEnumDiverCallTypeChanged(object sender, EventArgs e)
		{
			var listDriverCallType = UoW.Session.QueryOver<Order>()
										.Where(x => x.Id == Entity.Id)
										.Select(x => x.DriverCallType).List<DriverCallType>().FirstOrDefault();

			if(listDriverCallType != (DriverCallType)enumDiverCallType.SelectedItem) {
				var max = UoW.Session.QueryOver<Order>().Select(NHibernate.Criterion.Projections.Max<Order>(x => x.DriverCallId)).SingleOrDefault<int>();
				Entity.DriverCallId = max != 0 ? max + 1 : 1;
			}
		}

		protected void OnYEntTareActBtlFromClientChanged(object sender, EventArgs e)
		{
			IStandartDiscountsService standartDiscountsService = new BaseParametersProvider();
			Entity.CalculateBottlesStockDiscounts(standartDiscountsService);
		}

		protected void OnEntryTrifleChanged(object sender, EventArgs e)
		{
			if(int.TryParse(entryTrifle.Text, out int result)) {
				Entity.Trifle = result;
			}
		}

		protected void OnShown(object sender, EventArgs e)
		{
			//Скрывает журнал заказов при открытии заказа, чтобы все элементы умещались на экране
			if(TabParent is TdiSliderTab slider)
				slider.IsHideJournal = true;
		}

		protected void OnButtonDepositsClicked(object sender, EventArgs e)
		{
			ToggleVisibilityOfDeposits();
		}

		protected void OnChkContractCloserToggled(object sender, EventArgs e)
		{
			SetSensitivityOfPaymentType();
		}

		protected void OnEnumDiscountUnitEnumItemSelected(object sender, EnumItemClickedEventArgs e)
		{
			var sum = Entity.ObservableOrderItems.Sum(i => i.CurrentCount * i.Price);
			var unit = (DiscountUnits)e.ItemEnum;
			spinDiscount.Adjustment.Upper = unit == DiscountUnits.money ? (double)sum : 100d;
			if(unit == DiscountUnits.percent && spinDiscount.Value > 100)
				spinDiscount.Value = 100;
			if((SpecialComboState)enumDiscountUnit.SelectedItem != SpecialComboState.None) {
				Entity.SetDiscountUnitsForAll(unit);
				SetDiscountEditable();
				SetDiscount();
			}
		}

		private void OnEntryBottlesToReturnChanged(object sender, EventArgs e)
		{
			HboxReturnTareReasonCategoriesShow();
			
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void HboxReturnTareReasonCategoriesShow()
		{
			if (Entity.BottlesReturn.HasValue && Entity.BottlesReturn > 0)
			{
				hboxReturnTareReason.Visible = Entity.GetTotalWater19LCount() == 0;

				if(!hboxReturnTareReason.Visible) {
					hboxReasons.Visible = false;
					Entity.RemoveReturnTareReason();
				}
			}
			else
			{
				hboxReturnTareReason.Visible = hboxReasons.Visible = false;
				Entity.RemoveReturnTareReason();
			}
		}
		
		private void YCmbReturnTareReasonCategoriesOnChanged(object sender, EventArgs e)
		{
			ChangeHboxReasonsVisibility();
		}

		private void ChangeHboxReasonsVisibility()
		{
			if (yCmbReturnTareReasonCategories.SelectedItem is ReturnTareReasonCategory category)
			{
				if (!hboxReasons.Visible)
					hboxReasons.Visible = true;

				yCmbReturnTareReasons.ItemsList = category.ChildReasons;
			}
		}

		#endregion

		#region Service functions

		bool HasAgreementForDeliveryPoint()
		{
			bool a = Entity.HasActualWaterSaleAgreementByDeliveryPoint();
			if(Entity.ObservableOrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.water && !x.Nomenclature.IsDisposableTare) &&
			   !a) {
				//У выбранной точки доставки нет соглашения о доставке воды, предлагаем создать.
				//Если пользователь создаст соглашение, то запишется выбранная точка доставки
				//если не создаст то ничего не произойдет и точка доставки останется прежней
				CounterpartyContract contract = Entity.Contract ?? CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
				if(MessageDialogHelper.RunQuestionDialog("В заказе добавлена вода, а для данной точки доставки нет дополнительного соглашения о доставке воды, создать?")) {
					ITdiDialog dlg = new WaterAgreementDlg(contract, Entity.DeliveryPoint, Entity.DeliveryDate);
					(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
					TabParent.AddSlaveTab(this, dlg);
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Is the payment type barter or cashless?
		/// </summary>
		private bool IsPaymentTypeBarterOrCashless() => Entity.PaymentType == PaymentType.barter || Entity.PaymentType == PaymentType.cashless;

		/// <summary>
		/// Is the payment type cashless?
		/// </summary>
		private bool IsPaymentTypeCashless() => Entity.PaymentType == PaymentType.cashless;
		#endregion

		LastChosenAction lastChosenAction = LastChosenAction.None;

		private enum LastChosenAction
		{
			None,
			NonFreeRentAgreement,
			DailyRentAgreement,
			FreeRentAgreement,
		}

		//реализация метода интерфейса ITdiTabAddedNotifier
		public void OnTabAdded()
		{
			//если новый заказ и не создан из недовоза (templateOrder заполняется только из недовоза)
			if(UoW.IsNew && templateOrder == null && Entity.Client == null)
				//открыть окно выбора контрагента
				entityVMEntryClient.OpenSelectDialog();
		}

		public virtual bool HideItemFromDirectionReasonComboInEquipment(OrderEquipment node, DirectionReason item)
		{
			switch(item) {
				case DirectionReason.None:
					return true;
				case DirectionReason.Rent:
					return node.Direction == Domain.Orders.Direction.Deliver;
				case DirectionReason.Repair:
				case DirectionReason.Cleaning:
				case DirectionReason.RepairAndCleaning:
				default:
					return false;
			}
		}

		void Entity_UpdateClientCanChange(object aList, int[] aIdx)
		{
			entityVMEntryClient.IsEditable = Entity.CanChangeContractor();
		}

		void Entity_ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			FixPrice(aIdx[0]);
			
			if (Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder) {
				Entity.CheckAndSetOrderIsService();
				OnFormOrderActions();
			}

			treeAnyGoodsFirstColWidth = treeItems.Columns.First(x => x.Title == "Номенклатура").Width;
			treeItems.ExposeEvent += TreeAnyGoods_ExposeEvent;
			//Выполнение в случае если размер не поменяется
			EditGoodsCountCellOnAdd(treeItems);
		}
		
		void ObservableOrderItems_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			HboxReturnTareReasonCategoriesShow();
			
			if (Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder) {
				Entity.CheckAndSetOrderIsService();
				OnFormOrderActions();
			}
		}

		void ObservableOrderDocuments_ListChanged(object aList)
		{
			ShowOrderColumnInDocumentsList();
		}

		void ObservableOrderDocuments_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ShowOrderColumnInDocumentsList();
		}

		void ObservableOrderDocuments_ElementAdded(object aList, int[] aIdx)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ShowOrderColumnInDocumentsList()
		{
			var column = treeDocuments.ColumnsConfig.GetColumnsByTag("OrderNumberColumn").FirstOrDefault();
			column.Visible = Entity.ObservableOrderDocuments.Any(x => x.Order.Id != x.AttachedToOrder.Id);
		}

		void Entity_ObservableOrderDocuments_ElementAdded(object aList, int[] aIdx)
		{
			switch(lastChosenAction) {
				case LastChosenAction.NonFreeRentAgreement:
					AddRentAgreement(OrderAgreementType.NonfreeRent);
					break;
				case LastChosenAction.FreeRentAgreement:
					AddRentAgreement(OrderAgreementType.FreeRent);
					break;
				default:
					break;
			}
			lastChosenAction = LastChosenAction.None;
		}

		void FixPrice(int id)
		{
			OrderItem item = Entity.ObservableOrderItems[id];
			if(item.Nomenclature.Category == NomenclatureCategory.deposit && item.Price != 0)
				return;
			item.RecalculatePrice();
		}

		void TreeItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeItems.GetSelectedObjects();

			if(items.Length == 0) {
				return;
			}

			var deleteTypes = new AgreementType[] {
				AgreementType.WaterSales,
				AgreementType.FreeRent,
				AgreementType.NonfreeRent
			};
			btnDeleteOrderItem.Sensitive = items.Length > 0 && ((items[0] as OrderItem).AdditionalAgreement == null
														   || deleteTypes.Contains((items[0] as OrderItem).AdditionalAgreement.Type)
														  );
		}

		/// <summary>
		/// Для хранения состояния, было ли изменено количество оборудования в товарах, 
		/// для информирования пользователя о том, что изменения сохранятся также и в 
		/// дополнительном соглашении
		/// </summary>
		private bool OrderItemEquipmentCountHasChanges;

		/// <summary>
		/// При изменении количества оборудования в списке товаров меняет его
		/// также в доп. соглашении и списке оборудования заказа
		/// + если меняем количество у 19л бут, то меняем видимость у hboxReturnTareReason
		/// </summary>
		void ObservableOrderItems_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<OrderItem>) {
				foreach(var i in aIdx) {
					OrderItem oItem = (aList as GenericObservableList<OrderItem>)[aIdx] as OrderItem;
					
					FixPrice(aIdx[0]);
					
					if(oItem != null && oItem.Nomenclature.IsWater19L)
						HboxReturnTareReasonCategoriesShow();
					
					if(oItem != null && oItem.Count > 0 && Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
						OnFormOrderActions();
					
					if(oItem?.PaidRentEquipment == null) {
						return;
					}
					
					if(oItem.Nomenclature.Category == NomenclatureCategory.equipment) {
						ChangeEquipmentsCount(oItem, (int)oItem.Count);
					}
				}
			}
		}

		/// <summary>
		/// При изменении количества оборудования в списке оборудования меняет его 
		/// также в доп. соглашении и списке товаров заказа
		/// </summary>
		void ObservableOrderEquipments_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<OrderEquipment>) {
				foreach(var i in aIdx) {
					OrderEquipment oEquip = (aList as GenericObservableList<OrderEquipment>)[aIdx] as OrderEquipment;
					if(oEquip == null
					   || oEquip.OrderItem == null
					   || oEquip.OrderItem.PaidRentEquipment == null) {
						return;
					}
					if(oEquip.Count != oEquip.OrderItem.Count) {
						ChangeEquipmentsCount(oEquip.OrderItem, oEquip.Count);
					}
				}
			}
		}
		
		private void ObservableOrderDepositItemsOnElementRemoved(object alist, int[] aidx, object aobject) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void ObservableOrderDepositItemsOnElementAdded(object alist, int[] aidx) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void ObservableOrderEquipmentsOnElementRemoved(object alist, int[] aidx, object aobject) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void ObservableOrderEquipmentsOnElementAdded(object alist, int[] aidx) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}
		
		/// <summary>
		/// Меняет количество оборудования в списке оборудования заказа, в списке 
		/// товаров заказа, в списке оборудования дополнительного соглашения и 
		/// меняет количество залогов за оборудование в списке товаров заказа
		/// </summary>
		void ChangeEquipmentsCount(OrderItem orderItem, int newCount)
		{
			orderItem.Count = newCount;

			OrderEquipment orderEquip = Entity.OrderEquipments.FirstOrDefault(x => x.OrderItem == orderItem);
			if(orderEquip != null) {
				orderEquip.Count = newCount;
			}

			OrderItem depositItem;
			if(orderItem.PaidRentEquipment != null) {
				if(orderItem.PaidRentEquipment.Count != newCount) {
					orderItem.PaidRentEquipment.Count = newCount;
					OrderItemEquipmentCountHasChanges = true;
				}
				depositItem = Entity.OrderItems.FirstOrDefault
								(x => x.Nomenclature.Category == NomenclatureCategory.deposit
								 && x.AdditionalAgreement == orderItem.AdditionalAgreement
								 && x.PaidRentEquipment == orderItem.PaidRentEquipment);
				if(depositItem != null) {
					depositItem.Count = newCount;
				}
			}
			if(orderItem.FreeRentEquipment != null) {
				if(orderItem.FreeRentEquipment.Count != newCount) {
					orderItem.FreeRentEquipment.Count = newCount;
					OrderItemEquipmentCountHasChanges = true;
				}
				depositItem = Entity.OrderItems.FirstOrDefault
								(x => x.Nomenclature.Category == NomenclatureCategory.deposit
								 && x.AdditionalAgreement == orderItem.AdditionalAgreement
								 && x.FreeRentEquipment == orderItem.FreeRentEquipment);
				if(depositItem != null) {
					depositItem.Count = newCount;
				}
			}
		}
		private void UpdateUIState()
		{
			bool val = Entity.CanEditOrder;
			enumPaymentType.Sensitive = (Entity.Client != null) && val && !chkContractCloser.Active;
			referenceDeliverySchedule.Sensitive = referenceDeliveryPoint.IsEditable =
				entityVMEntryClient.IsEditable = val;
			referenceDeliverySchedule.Sensitive = labelDeliverySchedule.Sensitive = !checkSelfDelivery.Active && val;
			lblDeliveryPoint.Sensitive = referenceDeliveryPoint.Sensitive = !checkSelfDelivery.Active && val && Entity.Client != null;
			buttonAddMaster.Sensitive = !checkSelfDelivery.Active && val && !Entity.IsLoadedFrom1C;
			enumAddRentButton.Sensitive = enumSignatureType.Sensitive =
				enumDocumentType.Sensitive = val;
			buttonAddDoneService.Sensitive = buttonAddServiceClaim.Sensitive =
				buttonAddForSale.Sensitive = val;
			checkDelivered.Sensitive = checkSelfDelivery.Sensitive = val;
			//pickerDeliveryDate.Sensitive = val; // оно повторно устанавливается в ChangeOrderEditable(val) -> SetPadInfoSensitive(val)
			dataSumDifferenceReason.Sensitive = val;
			treeItems.Sensitive = val;
			ycheckContactlessDelivery.Sensitive = val;
			ycheckPaymentBySms.Sensitive = val;
			enumDiscountUnit.Visible = spinDiscount.Visible = labelDiscont.Visible = vseparatorDiscont.Visible = val;
			ChangeOrderEditable(val);
			checkPayAfterLoad.Sensitive = checkSelfDelivery.Active && val;
			buttonAddForSale.Sensitive = enumAddRentButton.Sensitive = !Entity.IsLoadedFrom1C;
			UpdateButtonState();
			ControlsActionBottleAccessibility();
			chkContractCloser.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_contract_closer") && val && !Entity.SelfDelivery;
			hbxTareNonReturnReason.Sensitive = val;
			lblTax.Visible = enumTax.Visible = val && IsEnumTaxVisible();

			if(Entity != null)
				yCmbPromoSets.Sensitive = val;
		}

		void ChangeOrderEditable(bool val)
		{
			SetPadInfoSensitive(val);
			vboxGoods.Sensitive = val;
			buttonAddExistingDocument.Sensitive = val;
			btnAddM2ProxyForThisOrder.Sensitive = val;
			btnRemExistingDocument.Sensitive = val;
			RouteListStatus? rlStatus = null;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				if(Entity.Id != 0)
					rlStatus = OrderSingletonRepository.GetInstance().GetAllRLForOrder(uow, Entity).FirstOrDefault()?.Status;
				tblDriverControl.Sensitive = rlStatus.HasValue && !new[] { RouteListStatus.MileageCheck, RouteListStatus.OnClosing, RouteListStatus.Closed }.Contains(rlStatus.Value);
			}
		}

		void SetPadInfoSensitive(bool value)
		{
			foreach(var widget in table1.Children)
				widget.Sensitive = widget.Name == vboxOrderComment.Name || value;
			
			if(chkContractCloser.Active)
				enumPaymentType.Sensitive = false;

			if (isEditOrderClicked)
			{
				pickerDeliveryDate.Sensitive = false;
			}
		}

		void SetSensitivityOfPaymentType()
		{
			if(chkContractCloser.Active) {
				Entity.PaymentType = PaymentType.cashless;
				UpdateUIState();
			} else {
				UpdateUIState();
			}
		}

		public void SetDlgToReadOnly()
		{
			buttonSave.Sensitive = buttonCancel.Sensitive =
			hboxStatusButtons.Visible = false;
		}

		void UpdateButtonState()
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_order")) {
				buttonEditOrder.Sensitive = false;
				buttonEditOrder.TooltipText = "Нет права на редактирование";
			}

			if(Entity.CanSetOrderAsAccepted) {
				buttonAcceptOrder.Visible = true;
				buttonEditOrder.Visible = false;
			} else if(Entity.CanSetOrderAsEditable) {
				buttonEditOrder.Visible = true;
				buttonAcceptOrder.Visible = false;
			} else {
				buttonAcceptOrder.Visible = false;
				buttonEditOrder.Visible = false;
			}

			btnSaveComment.Sensitive = Entity.OrderStatus != OrderStatus.NewOrder;

			//если новый заказ и тип платежа бартер или безнал, то вкл кнопку
			buttonWaitForPayment.Sensitive = Entity.OrderStatus == OrderStatus.NewOrder && IsPaymentTypeBarterOrCashless() && !Entity.SelfDelivery;

			buttonCancelOrder.Sensitive = (orderRepository.GetStatusesForOrderCancelation().Contains(Entity.OrderStatus) || (Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.OnLoading)) && Entity.OrderStatus != OrderStatus.NewOrder;

			menuItemSelfDeliveryToLoading.Sensitive = Entity.SelfDelivery
				&& Entity.OrderStatus == OrderStatus.Accepted
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("allow_load_selfdelivery");
			menuItemSelfDeliveryPaid.Sensitive = Entity.SelfDelivery
				&& (Entity.PaymentType == PaymentType.cashless || Entity.PaymentType == PaymentType.ByCard)
				&& Entity.OrderStatus == OrderStatus.WaitForPayment
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("accept_cashless_paid_selfdelivery");

			menuItemCloseOrder.Sensitive = Entity.OrderStatus == OrderStatus.Accepted && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_orders") && !Entity.SelfDelivery;
			menuItemReturnToAccepted.Sensitive = Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted;
		}

		void UpdateProxyInfo()
		{
			bool canShow = Entity.Client != null && Entity.DeliveryDate.HasValue &&
								 (Entity.Client?.PersonType == PersonType.legal || Entity.PaymentType == PaymentType.cashless);

			labelProxyInfo.Visible = canShow;

			DBWorks.SQLHelper text = new DBWorks.SQLHelper("");
			if(canShow) {
				var proxies = Entity.Client.Proxies.Where(p => p.IsActiveProxy(Entity.DeliveryDate.Value) && (p.DeliveryPoints == null || !p.DeliveryPoints.Any() || p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, Entity.DeliveryPoint))));
				foreach(var proxy in proxies) {
					if(!string.IsNullOrWhiteSpace(text.Text))
						text.Add("\n");
					text.Add(string.Format("Доверенность{2} №{0} от {1:d}", proxy.Number, proxy.IssueDate,
						proxy.DeliveryPoints == null ? "(общая)" : ""));
					text.StartNewList(": ");
					foreach(var pers in proxy.Persons) {
						text.AddAsList(pers.NameWithInitials);
					}
				}
			}
			if(string.IsNullOrWhiteSpace(text.Text))
				labelProxyInfo.Markup = "<span foreground=\"red\">Нет активной доверенности</span>";
			else
				labelProxyInfo.LabelProp = text.Text;
		}

		private void CheckSameOrders()
		{
			if(!Entity.DeliveryDate.HasValue || Entity.DeliveryPoint == null) {
				return;
			}

			var sameOrder = orderRepository.GetOrderOnDateAndDeliveryPoint(UoW, Entity.DeliveryDate.Value, Entity.DeliveryPoint);
			if(sameOrder != null && templateOrder == null) {
				MessageDialogHelper.RunWarningDialog("На выбранную дату и точку доставки уже есть созданный заказ!");
			}
		}

		void SetDiscountEditable(bool? canEdit = null)
		{
			spinDiscount.Sensitive = canEdit ?? enumDiscountUnit.SelectedItem != null;
		}

		void SetDiscountUnitEditable(bool? canEdit = null)
		{
			enumDiscountUnit.Sensitive = canEdit ?? ycomboboxReason.SelectedItem != null;
		}

		/// <summary>
		/// Переключает видимость элементов управления депозитами
		/// </summary>
		/// <param name="visibly"><see langword="true"/>если хотим принудительно сделать видимым;
		/// <see langword="false"/>если хотим принудительно сделать невидимым;
		/// <see langword="null"/>переключает видимость с невидимого на видимый и обратно.</param>
		private void ToggleVisibilityOfDeposits(bool? visibly = null)
		{
			depositrefunditemsview.Visible = visibly ?? !depositrefunditemsview.Visible;
			labelDeposit1.Visible = visibly ?? !labelDeposit1.Visible;
		}

		private void SetDiscount()
		{
			DiscountReason reason = (ycomboboxReason.SelectedItem as DiscountReason);
			DiscountUnits unit = (DiscountUnits)enumDiscountUnit.SelectedItem;
			if(decimal.TryParse(spinDiscount.Text, out decimal discount)) {
				if(reason == null && discount > 0) {
					MessageDialogHelper.RunErrorDialog("Необходимо выбрать основание для скидки");
					return;
				}
				Entity.SetDiscountUnitsForAll(unit);
				Entity.SetDiscount(reason, discount, unit);
			}
		}

		private bool HaveEmailForBill()
		{
			Vodovoz.Domain.Contacts.Email clientEmail = Entity.Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
			return clientEmail != null || MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?");
		}

		private void SendBillByEmail(Vodovoz.Domain.Contacts.Email emailAddressForBill)
		{
			if(emailAddressForBill == null) {
				throw new ArgumentNullException(nameof(emailAddressForBill));
			}

			if(!EmailServiceSetting.SendingAllowed || emailRepository.HaveSendedEmail(Entity.Id, OrderDocumentType.Bill)) {
				return;
			}

			if(!(Entity.OrderDocuments.FirstOrDefault(x => x.Type == OrderDocumentType.Bill) is BillDocument billDocument)) {
				MessageDialogHelper.RunErrorDialog("Невозможно отправить счет по электронной почте. Счет не найден.");
				return;
			}

			var organization = UoW.GetById<Organization>(new BaseParametersProvider().GetCashlessOrganisationId);
			if(organization == null) {
				MessageDialogHelper.RunErrorDialog("Невозможно отправить счет по электронной почте. В параметрах базы не определена организация для безналичного расчета");
				return;
			}
			var wasHideSignature = billDocument.HideSignature;
			billDocument.HideSignature = false;
			ReportInfo ri = billDocument.GetReportInfo();
			billDocument.HideSignature = wasHideSignature;

			var billTemplate = billDocument.GetEmailTemplate();
			EmailService.Email email = new EmailService.Email {
				Title = string.Format("{0} {1}", billTemplate.Title, billDocument.Title),
				Text = billTemplate.Text,
				HtmlText = billTemplate.TextHtml,
				Recipient = new EmailContact("", emailAddressForBill.Address),
				Sender = new EmailContact("vodovoz-spb.ru", ParametersProvider.Instance.GetParameterValue("email_for_email_delivery")),
				Order = Entity.Id,
				OrderDocumentType = OrderDocumentType.Bill
			};
			foreach(var item in billTemplate.Attachments) {
				email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
			}
			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
				string billDate = billDocument.DocumentDate.HasValue ? "_" + billDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
				email.AddAttachment($"Bill_{billDocument.Order.Id}{billDate}.pdf", stream);
			}
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var employee = employeeRepository.GetEmployeeForCurrentUser(uow);
				email.AuthorId = employee != null ? employee.Id : 0;
				email.ManualSending = false;
			}
			IEmailService service = EmailServiceSetting.GetEmailService();
			if(service == null) {
				return;
			}
			var result = service.SendEmail(email);

			//Если произошла ошибка и письмо не отправлено
			string resultMessage = "";
			if(!result.Item1) {
				resultMessage = "Письмо не было отправлено! Причина:\n";
			}
			MessageDialogHelper.RunInfoDialog(resultMessage + result.Item2);
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonViewDocument.Sensitive = treeDocuments.Selection.CountSelectedRows() > 0;

			var selectedDoc = treeDocuments.GetSelectedObjects().Cast<OrderDocument>().FirstOrDefault();
			if(selectedDoc == null) {
				return;
			}
			string email = "";
			if(!Entity.Client.Emails.Any()) {
				email = "";
			} else {
				Vodovoz.Domain.Contacts.Email clientEmail = Entity.Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
				if(clientEmail == null) {
					clientEmail = Entity.Client.Emails.FirstOrDefault();
				}
				email = clientEmail.Address;
			}
			SendDocumentByEmailViewModel.Update(selectedDoc, email);
		}

		protected void OnCheckSelfDeliveryToggled(object sender, EventArgs e)
		{
			UpdateUIState();

			if(!checkSelfDelivery.Active) {
				checkPayAfterLoad.Active = false;
			}
		}

		void ObservablePromotionalSets_ListChanged(object aList)
		{
			ShowPromoSetsColumn();
		}

		void ObservablePromotionalSets_ElementAdded(object aList, int[] aIdx)
		{
			ShowPromoSetsColumn();
		}

		void ObservablePromotionalSets_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ShowPromoSetsColumn();
		}

		private void ShowPromoSetsColumn()
		{
			var promoSetColumn = treeItems.ColumnsConfig.GetColumnsByTag(nameof(Entity.PromotionalSets)).FirstOrDefault();
			promoSetColumn.Visible = Entity.PromotionalSets.Count > 0;
		}

		protected void OnYBtnAddCurrentContractClicked(object sender, EventArgs e)
		{
			Order.AddContractDocument(Order.Contract);
		}
	}
}