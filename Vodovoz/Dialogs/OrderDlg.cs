using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using EmailService;
using fyiReporting.RDL;
using fyiReporting.RdlGtkViewer;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using NHibernate.Proxy;
using NHibernate.Util;
using NLog;
using QS.Print;
using QSDocTemplates;
using QSEmailSending;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSSupportLib;
using QSTDI;
using QSValidation;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Client;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.JournalFilters;
using Vodovoz.Repositories;
using Vodovoz.Repositories.Client;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;
using Vodovoz.Repository.Operations;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz
{
	public partial class OrderDlg : OrmGtkDialogBase<Order>,
		ICounterpartyInfoProvider, 
		IDeliveryPointInfoProvider, 
		IContractInfoProvider, 
		ITdiTabAddedNotifier, 
		IEmailsInfoProvider
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		Order templateOrder = null;

		#region Работа с боковыми панелями

		public PanelViewType[] InfoWidgets {
			get {
				return new[]{
					PanelViewType.AdditionalAgreementPanelView,
					PanelViewType.CounterpartyView,
					PanelViewType.DeliveryPointView,
					PanelViewType.DeliveryPricePanelView,
					PanelViewType.EmailsPanelView
				};
			}
		}

		public Counterparty Counterparty => Entity.Client;

		public DeliveryPoint DeliveryPoint => Entity.DeliveryPoint;

		public CounterpartyContract Contract => Entity.Contract;

		public bool CanHaveEmails => Entity.Id != 0;

		public List<StoredEmail> GetEmails()
		{
			if(Entity.Id == 0) {
				return null;
			}
			return EmailRepository.GetAllEmailsForOrder(UoW, Entity.Id);
		}

		#endregion

		#region Конструкторы, настройка диалога

		public override void Destroy()
		{
			OrmMain.GetObjectDescription<WaterSalesAgreement>().ObjectUpdatedGeneric -= WaterSalesAgreement_ObjectUpdatedGeneric;
			base.Destroy();
		}

		public OrderDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Order>();
			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать создавать заказы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}
			Entity.OrderStatus = OrderStatus.NewOrder;
			TabName = "Новый заказ";
			ConfigureDlg();
		}

		public OrderDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order>(id);
			ConfigureDlg();
		}

		public OrderDlg(Order sub) : this(sub.Id)
		{ }

		public void CopyOrderFrom(int id)
		{
			templateOrder = UoW.GetById<Order>(id);
			Entity.Client = templateOrder.Client;
			Entity.DeliveryPoint = templateOrder.DeliveryPoint;
			Entity.Comment = templateOrder.Comment;
			Entity.CommentLogist = templateOrder.CommentLogist;
			Entity.PaymentType = templateOrder.PaymentType;
			Entity.ClientPhone = templateOrder.ClientPhone;
			Entity.ReasonType = templateOrder.ReasonType;
			Entity.BillDate = templateOrder.BillDate;
			Entity.BottlesReturn = templateOrder.BottlesReturn;
			Entity.CollectBottles = templateOrder.CollectBottles;
			Entity.CommentManager = templateOrder.CommentManager;
			Entity.DocumentType = templateOrder.DocumentType;
			Entity.ExtraMoney = templateOrder.ExtraMoney;
			Entity.InformationOnTara = templateOrder.InformationOnTara;
			Entity.OnlineOrder = templateOrder.OnlineOrder;
			Entity.PreviousOrder = templateOrder.PreviousOrder;
			Entity.ReturnedTare = templateOrder.ReturnedTare;
			Entity.SelfDelivery = templateOrder.SelfDelivery;
			Entity.SignatureType = templateOrder.SignatureType;
			Entity.SumDifferenceReason = templateOrder.SumDifferenceReason;
			Entity.Trifle = templateOrder.Trifle;
			Entity.IsService = templateOrder.IsService;
			Entity.Contract = templateOrder.Contract;
			Entity.CopyItemsFrom(templateOrder);
			Entity.CopyDocumentsFrom(templateOrder);
			Entity.CopyEquipmentFrom(templateOrder);
			Entity.CopyDepositItemsFrom(templateOrder);
			Entity.UpdateDocuments();

			ConfigureDlg();
		}

		public void ConfigureDlg()
		{
			ConfigureTrees();

			enumDiscountUnit.SetEnumItems((DiscountUnits[])Enum.GetValues(typeof(DiscountUnits)));
			spinDiscount.Adjustment.Upper = 100;

			if(Entity.PreviousOrder != null) {
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
				//TODO Make it clickable.
			} else
				labelPreviousOrder.Visible = false;
			hboxStatusButtons.Visible = OrderRepository.GetStatusesForOrderCancelation().Contains(Entity.OrderStatus)
				|| Entity.OrderStatus == OrderStatus.Canceled
				|| Entity.OrderStatus == OrderStatus.Closed;

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

			//Подписываемся на изменение товара, для обновления количества оборудования в доп. соглашении
			Entity.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged_ChangeCount;
			Entity.ObservableOrderEquipments.ElementChanged += ObservableOrderEquipments_ElementChanged_ChangeCount;

			enumSignatureType.ItemsEnum = typeof(OrderSignatureType);
			enumSignatureType.Binding.AddBinding(Entity, s => s.SignatureType, w => w.SelectedItem).InitializeFromSource();

			labelCreationDateValue.Binding.AddFuncBinding(Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			ylabelOrderStatus.Binding.AddFuncBinding(Entity, e => e.OrderStatus.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelNumber.Binding.AddFuncBinding(Entity, e => e.Code1c + (e.DailyNumber.HasValue ? $" ({e.DailyNumber})" : ""), w => w.LabelProp).InitializeFromSource();

			enumDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDocumentType.Binding.AddBinding(Entity, s => s.DocumentType, w => w.SelectedItem).InitializeFromSource();

			chkContractCloser.Binding.AddBinding(Entity, c => c.IsContractCloser, w => w.Active).InitializeFromSource();

			pickerDeliveryDate.Binding.AddBinding(Entity, s => s.DeliveryDate, w => w.DateOrNull).InitializeFromSource();
			pickerDeliveryDate.DateChanged += PickerDeliveryDate_DateChanged;
			pickerBillDate.Visible = labelBillDate.Visible = Entity.PaymentType == PaymentType.cashless;
			pickerBillDate.Binding.AddBinding(Entity, s => s.BillDate, w => w.DateOrNull).InitializeFromSource();

			textComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			textCommentsLogistic.Binding.AddBinding(Entity, s => s.CommentLogist, w => w.Buffer.Text).InitializeFromSource();

			checkSelfDelivery.Binding.AddBinding(Entity, s => s.SelfDelivery, w => w.Active).InitializeFromSource();
			checkDelivered.Binding.AddBinding(Entity, s => s.Shipped, w => w.Active).InitializeFromSource();

			entryBottlesToReturn.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryBottlesToReturn.Binding.AddBinding(Entity, e => e.BottlesReturn, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			if(Entity.OrderStatus == OrderStatus.Closed) {
				entryTareReturned.Text = BottlesRepository.GetEmptyBottlesFromClientByOrder(UoW, Entity).ToString();
				entryTareReturned.Visible = lblTareReturned.Visible = true;
			}

			entryTrifle.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryTrifle.Binding.AddBinding(Entity, e => e.Trifle, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			referenceContract.Binding.AddBinding(Entity, e => e.Contract, w => w.Subject).InitializeFromSource();

			OldFieldsConfigure();

			txtOnRouteEditReason.Binding.AddBinding(Entity, e => e.OnRouteEditReason, w => w.Buffer.Text).InitializeFromSource();

			entryOnlineOrder.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryOnlineOrder.Binding.AddBinding(Entity, e => e.OnlineOrder, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.SetAndRefilterAtOnce(x => x.RestrictIncludeArhive = false);
			referenceClient.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceClient.Binding.AddBinding(Entity, s => s.Client, w => w.Subject).InitializeFromSource();
			referenceClient.CanEditReference = true;

			referenceDeliverySchedule.ItemsQuery = DeliveryScheduleRepository.AllQuery();
			referenceDeliverySchedule.SetObjectDisplayFunc<DeliverySchedule>(e => e.Name);
			referenceDeliverySchedule.Binding.AddBinding(Entity, s => s.DeliverySchedule, w => w.Subject).InitializeFromSource();
			referenceDeliverySchedule.Binding.AddBinding(Entity, s => s.DeliverySchedule1c, w => w.TooltipText).InitializeFromSource();

			var filterAuthor = new EmployeeFilter(UoW);
			filterAuthor.RestrictFired = false;
			referenceAuthor.RepresentationModel = new ViewModel.EmployeesVM(filterAuthor);
			referenceAuthor.Binding.AddBinding(Entity, s => s.Author, w => w.Subject).InitializeFromSource();
			referenceAuthor.Sensitive = false;

			referenceDeliveryPoint.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceDeliveryPoint.Sensitive = (Entity.Client != null);
			referenceDeliveryPoint.CanEditReference = true;
			chkContractCloser.Sensitive = QSMain.User.Permissions["can_set_contract_closer"];

			buttonViewDocument.Sensitive = false;
			buttonDelete1.Sensitive = false;
			notebook1.ShowTabs = false;
			notebook1.Page = 0;

			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);

			commentsview4.UoW = UoWGeneric;

			enumAddRentButton.ItemsEnum = typeof(OrderAgreementType);
			enumAddRentButton.EnumItemClicked += (sender, e) => AddRentAgreement((OrderAgreementType)e.ItemEnum);

			checkSelfDelivery.Toggled += (sender, e) => {
				referenceDeliverySchedule.Sensitive = labelDeliverySchedule.Sensitive = !checkSelfDelivery.Active;
			};

			Entity.ObservableOrderItems.ElementChanged += (aList, aIdx) => {
				FixPrice(aIdx[0]);
			};

			Entity.ObservableOrderItems.ElementAdded += (aList, aIdx) => {
				FixPrice(aIdx[0]);
			};

			dataSumDifferenceReason.Binding.AddBinding(Entity, s => s.SumDifferenceReason, w => w.Text).InitializeFromSource();
			dataSumDifferenceReason.Completion = new EntryCompletion();
			dataSumDifferenceReason.Completion.Model = OrderRepository.GetListStoreSumDifferenceReasons(UoWGeneric);
			dataSumDifferenceReason.Completion.TextColumn = 0;

			spinSumDifference.Binding.AddBinding(Entity, e => e.ExtraMoney, w => w.ValueAsDecimal).InitializeFromSource();

			labelSum.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.TotalSum), w => w.LabelProp).InitializeFromSource();
			labelCashToReceive.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.SumToReceive), w => w.LabelProp).InitializeFromSource();

			enumPaymentType.ItemsEnum = typeof(PaymentType);
			enumPaymentType.Binding.AddBinding(Entity, s => s.PaymentType, w => w.SelectedItem).InitializeFromSource();
			SetSensitivityOfPaymentType();

			textManagerComments.Binding.AddBinding(Entity, s => s.CommentManager, w => w.Buffer.Text).InitializeFromSource();
			enumDiverCallType.ItemsEnum = typeof(DriverCallType);
			enumDiverCallType.Binding.AddBinding(Entity, s => s.DriverCallType, w => w.SelectedItem).InitializeFromSource();

			referenceDriverCallId.Binding.AddBinding(Entity, e => e.DriverCallId, w => w.Subject).InitializeFromSource();
			enumareRasonType.ItemsEnum = typeof(ReasonType);
			enumareRasonType.Binding.AddBinding(Entity, s => s.ReasonType, w => w.SelectedItem).InitializeFromSource();

			UpdateButtonState();

			if(Entity.DeliveryPoint == null && !string.IsNullOrWhiteSpace(Entity.Address1c)) {
				var deliveryPoint = Counterparty.DeliveryPoints.FirstOrDefault(d => d.Address1c == Entity.Address1c);
				if(deliveryPoint != null)
					Entity.DeliveryPoint = deliveryPoint;
			}

			if(Entity.OrderStatus != OrderStatus.NewOrder)
				IsUIEditable(CanChange);
			tableTareControl.Sensitive = !(Entity.OrderStatus == OrderStatus.NewOrder || Entity.OrderStatus == OrderStatus.Accepted);

			OrderItemEquipmentCountHasChanges = false;
			ShowOrderColumnInDocumentsList();
			ButtonCloseOrderAccessibilityAndAppearance();
			SetSensitivityOfPaymentType();
			depositrefunditemsview.Configure(UoWGeneric, Entity);
			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = UoW.Session.QueryOver<DiscountReason>().List();

			OrmMain.GetObjectDescription<WaterSalesAgreement>().ObjectUpdatedGeneric += WaterSalesAgreement_ObjectUpdatedGeneric;
			ToggleVisibilityOfDeposits(Entity.ObservableOrderDepositItems.Any());
			SetDiscountEditable();
			SetDiscountUnitEditable();

			spinSumDifference.Hide();
			labelSumDifference.Hide();
			dataSumDifferenceReason.Hide();
			labelSumDifferenceReason.Hide();
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
				.AddColumn(!OrderRepository.GetStatusesForActualCount().Contains(Entity.OrderStatus) ? "Кол-во" : "Кол-во [Факт]")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddSetter((c, node) => c.Editable = node.CanEditAmount).WidthChars(10)
				.AddTextRenderer(node => OrderRepository.GetStatusesForActualCount().Contains(Entity.OrderStatus) ? String.Format("[{0}]", node.ActualCount) : "")
				.AddTextRenderer(node => (node.CanShowReturnedCount) ? String.Format("({0})", node.ReturnedCount) : "")
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.IsRentCategory ? node.RentString : "")
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, node) => c.Editable = node.CanEditPrice())
					.AddSetter((NodeCellRendererSpin<OrderItem> c, OrderItem node) => {
						c.ForegroundGdk = colorBlack;
						if(node.AdditionalAgreement == null) {
							return;
						}
						AdditionalAgreement aa = node.AdditionalAgreement.Self;
						if(aa is WaterSalesAgreement &&
						  (aa as WaterSalesAgreement).HasFixedPrice) {
							c.ForegroundGdk = colorGreen;
						} else if(node.IsUserPrice &&
								  Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category)) {
							c.ForegroundGdk = colorBlue;
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
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.DiscountForPreview).Editing(true)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)n.Price * n.CurrentCount, 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?").AddToggleRenderer(x => x.IsDiscountInMoney)
					.Editing()
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(node => node.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(OrderRepository.GetDiscountReasons(UoW))
					.AddSetter((c, n) => c.Editable = n.Discount > 0)
					.AddSetter(
						(c, n) => c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null
						? colorLightRed
						: colorWhite
					)
				.AddColumn("Доп. соглашение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.AgreementString)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = Entity.ObservableOrderItems;
			treeItems.Selection.Changed += TreeItems_Selection_Changed;

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<OrderDocument>()
				.AddColumn("Документ").SetDataProperty(node => node.Name)
				.AddColumn("Дата документа").AddTextRenderer(node => node.DocumentDateText)
				.AddColumn("Заказ №").SetTag("OrderNumberColumn").AddTextRenderer(node => node.Order.Id != node.AttachedToOrder.Id ? node.Order.Id.ToString() : "")
				.AddColumn("Без рекламы").AddToggleRenderer(x => x is IAdvertisable ? (x as IAdvertisable).WithoutAdvertising : false)
				.Editing().ChangeSetProperty(PropertyUtil.GetPropertyInfo<IAdvertisable>(x => x.WithoutAdvertising))
				.AddSetter((c, n) => c.Visible = n.Type == OrderDocumentType.Invoice || n.Type == OrderDocumentType.InvoiceContractDoc)
				.AddColumn("Без подписей и печати").AddToggleRenderer(x => x is BillDocument ? (x as BillDocument).HideSignature : false)
				.Editing().ChangeSetProperty(PropertyUtil.GetPropertyInfo<BillDocument>(x => x.HideSignature))
				.AddSetter((c, n) => c.Visible = n.Type == OrderDocumentType.Bill)
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

		/// <summary>
		/// Старые поля, оставлены для отображения информации в старых заказах. В новых скрыты.
		/// Не удаляем полностью а только скрываем, чтобы можно было увидеть адрес в старых заказах, загруженных из 1с.
		/// </summary>
		private void OldFieldsConfigure()
		{
			yentryAddress1cDeliveryPoint.Binding.AddBinding(Entity, e => e.Address1c, w => w.Text).InitializeFromSource();
			yentryAddress1cDeliveryPoint.Binding.AddBinding(Entity, e => e.Address1c, w => w.TooltipText).InitializeFromSource();
			labelAddress1c.Visible = yentryAddress1cDeliveryPoint.Visible = buttonCreateDeliveryPoint.Visible = !String.IsNullOrWhiteSpace(Entity.Address1c);
			textTaraComments.Binding.AddBinding(Entity, e => e.InformationOnTara, w => w.Buffer.Text).InitializeFromSource();
			labelTaraComments.Visible = GtkScrolledWindowTaraComments.Visible = !String.IsNullOrWhiteSpace(Entity.InformationOnTara);
		}

		void WaterSalesAgreement_ObjectUpdatedGeneric(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<WaterSalesAgreement> e)
		{
			foreach(var ad in e.UpdatedSubjects) {
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

		public override bool Save()
		{
			Entity.CheckAndSetOrderIsService();
			var valid = new QSValidator<Order>(
				Entity, new Dictionary<object, object>{
					{ "IsCopiedFromUndelivery", templateOrder != null } //индикатор того, что заказ - копия, созданная из недовозов
				}
			);

			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return false;

			if(Entity.OrderStatus == OrderStatus.NewOrder) {
				if(!MessageDialogWorks.RunQuestionDialog("Вы не подтвердили заказ. Вы уверены что хотите оставить его в качестве черновика?"))
					return false;
			}

			if(OrderItemEquipmentCountHasChanges) {
				MessageDialogWorks.RunInfoDialog("Было изменено количество оборудования в заказе, оно также будет изменено в дополнительном соглашении");
			}

			logger.Info("Сохраняем заказ...");

			if(EmailServiceSetting.CanSendEmail && Entity.NeedSendBill()){
				var emailAddressForBill = Entity.GetEmailAddressForBill();
				if(emailAddressForBill == null) {
					if(!MessageDialogWorks.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?")) {
						return false;
					}
				}
				Entity.SaveEntity(UoWGeneric);
				SendBillByEmail(emailAddressForBill);
			}else {
				Entity.SaveEntity(UoWGeneric);
			}

			UoW.Session.Refresh(Entity);
			logger.Info("Ok.");
			ButtonCloseOrderAccessibilityAndAppearance();
			return true;
		}

		protected void OnBtnSaveCommentClicked(object sender, EventArgs e)
		{
			Entity.SaveOrderComment();
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.OnTheWay) {
				if(buttonAccept.Label == "Редактировать") {
					IsUIEditable(true);
					var icon = new Image();
					icon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu);
					buttonAccept.Image = icon;
					buttonAccept.Label = "Подтвердить";
					buttonSave.Sensitive = false;
				} else if(buttonAccept.Label == "Подтвердить") {
					if(AcceptOrder()) {
						IsUIEditable(false);
						var icon = new Image();
						icon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu);
						buttonAccept.Image = icon;
						buttonAccept.Label = "Редактировать";
						buttonSave.Sensitive = true;
					}
				}
				return;
			}

			if((Entity.OrderStatus == OrderStatus.NewOrder
				|| Entity.OrderStatus == OrderStatus.WaitForPayment)
			   && !DefaultWaterCheck()) {
				toggleGoods.Activate();
				return;
			}

			if(Entity.OrderStatus == OrderStatus.NewOrder
			   || Entity.OrderStatus == OrderStatus.WaitForPayment) {
				AcceptOrder();
				UpdateButtonState();
				return;
			}
			if(Entity.OrderStatus == OrderStatus.Accepted
			   || Entity.OrderStatus == OrderStatus.Canceled) {
				Entity.ChangeStatus(OrderStatus.NewOrder);
				UpdateButtonState();
				return;
			}
		}

		bool AcceptOrder()
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

			if(Contract == null && !Entity.IsLoadedFrom1C) {
				Entity.Contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
				if(Entity.Contract == null)
					Entity.CreateDefaultContract();
			}

			if(Entity.OrderStatus == OrderStatus.NewOrder
			   || Entity.OrderStatus == OrderStatus.WaitForPayment) {
				Entity.ChangeStatus(OrderStatus.Accepted);
			}
			treeItems.Selection.UnselectAll();
			var successfullySaved = Save();
			PrintOrderDocuments();
			return successfullySaved;
		}

		/// <summary>
		/// Ручное закрытие заказа
		/// </summary>
		protected void OnButtonCloseOrderClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted) {
				if(!MessageDialogWorks.RunQuestionDialog("Вы уверены, что хотите вернуть заказ в статус \"Принят\"?"))
					return;

				Entity.ChangeStatus(OrderStatus.Accepted);
			} else if(Entity.OrderStatus == OrderStatus.Accepted && QSMain.User.Permissions["can_close_orders"]) {
				if(!MessageDialogWorks.RunQuestionDialog("Вы уверены, что хотите закрыть заказ?"))
					return;

				Entity.UpdateBottlesMovementOperation(UoW);
				Entity.UpdateDepositOperations(UoW);

				Entity.ChangeStatus(OrderStatus.Closed);
				Entity.ObservableOrderItems.ForEach(i => i.ActualCount = i.Count);
			}
			ButtonCloseOrderAccessibilityAndAppearance();
		}

		void ButtonCloseOrderAccessibilityAndAppearance()
		{
			buttonCloseOrder.Sensitive = Entity.OrderStatus == OrderStatus.Accepted && QSMain.User.Permissions["can_close_orders"]
				|| Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted;

			if(Entity.OrderStatus == OrderStatus.Accepted)
				buttonCloseOrder.Label = "Закрыть без доставки";
			else
				buttonCloseOrder.Label = "Вернуть в \"Принят\"";
		}

		#endregion

		#region Документы заказа

		public void PrintOrderDocuments()
		{
			if(Entity.OrderDocuments.Any()) {
				if(MessageDialogWorks.RunQuestionDialog("Открыть документы для печати?")) {
					var documentPrinterDlg = new OrderDocumentsPrinterDlg(Entity);
					TabParent.AddSlaveTab(this, documentPrinterDlg);
				}
			}
		}

		protected void OnBtnRemExistingDocumentClicked(object sender, EventArgs e)
		{
			if(!MessageDialogWorks.RunQuestionDialog("Вы уверены, что хотите удалить выделенные документы?")) return;
			var documents = treeDocuments.GetSelectedObjects<OrderDocument>();
			var notDeletedDocs = Entity.RemoveAdditionalDocuments(documents);
			if(notDeletedDocs != null && notDeletedDocs.Any()) {
				String strDocuments = "";
				foreach(OrderDocument doc in notDeletedDocs) {
					strDocuments += String.Format("\n\t{0}", doc.Name);
				}
				MessageDialogWorks.RunWarningDialog(String.Format("Документы{0}\nудалены не были, так как относятся к текущему заказу.", strDocuments));
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
				var dlgM2 = OrmMain.CreateObjectDialog(typeof(M2ProxyDocument), UoWGeneric);
				TabParent.AddSlaveTab(this, dlgM2);
			}
		}

		protected void OnButtonAddExistingDocumentClicked(object sender, EventArgs e)
		{
			if(Entity.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления дополнительных документов должен быть выбран клиент.");
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
			if(treeDocuments.GetSelectedObjects().Any()) {
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
				if(odtDocs.Any()) {
					foreach(var doc in odtDocs) {
						ITdiDialog dlg = null;
						if(doc is OrderAgreement) {
							var agreement = (doc as OrderAgreement).AdditionalAgreement;
							var type = NHibernateProxyHelper.GuessClass(agreement);
							var dialog = OrmMain.CreateObjectDialog(type, agreement.Id);
							if(dialog is IAgreementSaved) {
								(dialog as IAgreementSaved).AgreementSaved += AgreementSaved;
							}
							TabParent.OpenTab(
								OrmMain.GenerateDialogHashName(type, agreement.Id),
								() => dialog
							);
						} else if(doc is OrderContract) {
							var contract = (doc as OrderContract).Contract;
							dlg = OrmMain.CreateObjectDialog(contract);
						} else if(doc is OrderM2Proxy) {
							var m2Proxy = (doc as OrderM2Proxy).M2Proxy;
							dlg = OrmMain.CreateObjectDialog(m2Proxy);
						}
						if(dlg != null) {
							(dlg as IEditableDialog).IsEditable = false;
							TabParent.AddSlaveTab(this, dlg);
						}
					}
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
				DocumentPrinter.PrintAll(docList);
			}
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
			ITdiTab mytab = TdiHelper.FindMyTab(this);
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
			OrmReference SelectDialog = new OrmReference(typeof(ServiceClaim), UoWGeneric,
											ServiceClaimRepository.GetDoneClaimsForClient(Entity)
				.GetExecutableQueryOver(UoWGeneric.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanEdit;
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
			if(!contract.RepairAgreementExists()) {
				RunRepairAgreementCreateDialog(contract);
				return;
			}
			selected.FinalOrder = Entity;
			Entity.ObservableFinalOrderService.Add(selected);
			//TODO Add service nomenclature with price.
		}

		private void RunRepairAgreementCreateDialog(CounterpartyContract contract)
		{
			ITdiTab dlg;
			string question = "Отсутствует доп. соглашение сервиса с клиентом в текущем договоре. Создать?";
			if(MessageDialogWorks.RunQuestionDialog(question)) {
				dlg = new RepairAgreementDlg(contract);
				(dlg as IAgreementSaved).AgreementSaved += (sender, e) =>
					Entity.CreateOrderAgreementDocument(e.Agreement);
				TabParent.AddSlaveTab(this, dlg);
			}
		}

		void TreeServiceClaim_Selection_Changed(object sender, EventArgs e)
		{
			buttonOpenServiceClaim.Sensitive = treeServiceClaim.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonOpenServiceClaimClicked(object sender, EventArgs e)
		{
			var claim = treeServiceClaim.GetSelectedObject<ServiceClaim>();
			OpenTab(
				OrmGtkDialogBase<ServiceClaim>.GenerateHashName(claim.Id),
				() => new ServiceClaimDlg(claim)
			);
		}


		#endregion

		#region Добавление номенклатур

		protected void OnButtonAddMasterClicked(object sender, EventArgs e)
		{
			if(Entity.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			if(Entity.DeliveryDate == null) {
				MessageDialogWorks.RunErrorDialog("Введите дату доставки");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = new NomenclatureCategory[] { NomenclatureCategory.master },
				x => x.DefaultSelectedCategory = NomenclatureCategory.master
			);
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Выезд мастера";
			SelectDialog.ObjectSelected += NomenclatureForSaleSelected;
			SelectDialog.ShowFilter = true;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		protected void OnButtonAddForSaleClicked(object sender, EventArgs e)
		{
			if(Entity.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			if(Entity.DeliveryPoint == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должна быть выбрана точка доставки.");
				return;
			}

			if(Entity.DeliveryDate == null) {
				MessageDialogWorks.RunWarningDialog("Введите дату доставки");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.water,
				x => x.DefaultSelectedSubCategory = SubtypeOfEquipmentCategory.forSale
			);
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Номенклатура на продажу";
			SelectDialog.ObjectSelected += NomenclatureForSaleSelected;
			SelectDialog.ShowFilter = true;
			TabParent.AddSlaveTab(this, SelectDialog);

		}

		void NomenclatureForSaleSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			AddNomenclature(UoWGeneric.Session.Get<Nomenclature>(e.ObjectId));
		}

		void NomenclatureSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			AddNomenclature(e.Subject as Nomenclature);
		}

		void AddNomenclature(Nomenclature nomenclature, int count = 0)
		{
			if(Entity.IsLoadedFrom1C) {
				return;
			}

			if(Entity.OrderItems.Any(x => !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
			   && nomenclature.Category == NomenclatureCategory.master) {
				MessageDialogWorks.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
				return;
			}

			if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
			   && !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category)) {
				MessageDialogWorks.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}

			switch(nomenclature.Category) {
				case NomenclatureCategory.equipment://Оборудование
					RunAdditionalAgreementSalesEquipmentDialog(nomenclature);
					break;
				case NomenclatureCategory.disposableBottleWater://Вода в одноразовой таре
				case NomenclatureCategory.water://Вода в многооборотной таре
					CounterpartyContract contract = Entity.Contract;
					if(contract == null) {
						contract = CounterpartyContractRepository.
							GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
						Entity.Contract = contract;
					}
					if(contract == null) {
						var result = AskCreateContract();
						switch(result) {
							case (int)ResponseType.Yes:
								RunContractAndWaterAgreementDialog(nomenclature, count);
								break;
							case (int)ResponseType.Accept:
								CreateContractWithAgreement(nomenclature, count);
								break;
							default:
								break;
						}
						return;
					}
					UoWGeneric.Session.Refresh(contract);
					WaterSalesAgreement wsa = contract.GetWaterSalesAgreement(Entity.DeliveryPoint, nomenclature);
					if(wsa == null) {
						wsa = ClientDocumentsRepository.CreateDefaultWaterAgreement(UoW, Entity.DeliveryPoint, Entity.DeliveryDate, contract);
						contract.AdditionalAgreements.Add(wsa);
						Entity.CreateOrderAgreementDocument(wsa);
					}
					Entity.AddWaterForSale(nomenclature, wsa, count);
					Entity.RecalcBottlesDeposits(UoWGeneric);
					break;
				case NomenclatureCategory.master:
					contract = null;
					if(Entity.Contract == null) {
						contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
						if(contract == null) {
							contract = ClientDocumentsRepository.CreateDefaultContract(UoW, Entity.Client, Entity.PaymentType, Entity.DeliveryDate);
							Entity.Contract = contract;
							Entity.AddContractDocument(contract);
						}
					} else {
						contract = Entity.Contract;
					}
					Entity.AddMasterNomenclature(nomenclature, 1, 1);
					break;
				case NomenclatureCategory.deposit://Залог
				default://rest
					Entity.AddAnyGoodsNomenclatureForSale(nomenclature);
					break;
			}
		}

		private void AddRentAgreement(OrderAgreementType type)
		{
			if(Entity.IsLoadedFrom1C) {
				return;
			}

			if(Entity.Client == null || Entity.DeliveryPoint == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления оборудования должна быть выбрана точка доставки.");
				return;
			}

			if(Entity.ObservableOrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)) {
				MessageDialogWorks.RunWarningDialog("Нельзя добавлять аренду в сервисный заказ");
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
					case OrderAgreementType.DailyRent:
						lastChosenAction = LastChosenAction.DailyRentAgreement;
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
			if(Entity.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.equipment
			);
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование к клиенту";
			SelectDialog.ObjectSelected += NomenclatureToClient;
			SelectDialog.ShowFilter = true;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureToClient(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			AddNomenclatureToClient(UoWGeneric.Session.Get<Nomenclature>(e.ObjectId));
		}

		void AddNomenclatureToClient(Nomenclature nomenclature)
		{
			Entity.AddEquipmentNomenclatureToClient(nomenclature, UoWGeneric);
		}

		protected void OnButtonAddEquipmentFromClientClicked(object sender, EventArgs e)
		{
			if(Entity.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.equipment
			);
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование от клиента";
			SelectDialog.ObjectSelected += NomenclatureFromClient;
			SelectDialog.ShowFilter = true;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureFromClient(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			AddNomenclatureFromClient(UoWGeneric.Session.Get<Nomenclature>(e.ObjectId));
		}

		void AddNomenclatureFromClient(Nomenclature nomenclature)
		{
			Entity.AddEquipmentNomenclatureFromClient(nomenclature, UoWGeneric);
		}

		public void FillOrderItems(Order order)
		{
			if(Entity.OrderStatus != OrderStatus.NewOrder
			   || Entity.ObservableOrderItems.Any() && !MessageDialogWorks.RunQuestionDialog("Вы уверены, что хотите удалить все позиции текущего из заказа и заполнить его позициями из выбранного?")) {
				return;
			}

			Entity.ClearOrderItemsList();
			foreach(OrderItem orderItem in order.OrderItems) {
				switch(orderItem.Nomenclature.Category) {
					case NomenclatureCategory.additional:
						Entity.AddNomenclatureForSaleFromPreviousOrder(orderItem, UoWGeneric);
						continue;
					case NomenclatureCategory.disposableBottleWater:
					case NomenclatureCategory.water:
						AddNomenclature(orderItem.Nomenclature, orderItem.Count);
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
					AgreementType.EquipmentSales,
					AgreementType.DailyRent,
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

				if(!MessageDialogWorks.RunQuestionDialog("Заказ будет сохранен после удаления товара, продолжить?")
				   || valid.RunDlgIfNotValid((Window)this.Toplevel)) {
					return;
				}

				Type agreementType = null;
				switch(agreement.Type) {
					case AgreementType.NonfreeRent:
						agreementType = typeof(NonfreeRentAgreement);
						break;
					case AgreementType.DailyRent:
						agreementType = typeof(DailyRentAgreement);
						break;
					case AgreementType.FreeRent:
						agreementType = typeof(FreeRentAgreement);
						break;
					case AgreementType.EquipmentSales:
						agreementType = typeof(SalesEquipmentAgreement);
						break;
					default:
						return;
				}

				var deletionObjects = OrmMain.GetDeletionObjects(agreementType, agreement.Id);

				//Нахождение, есть объекты которые не связаны с текущим заказом,
				//но которые необходимо удалить вместе с доп соглашением
				bool canDelete = true;

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
					MessageDialogWorks.RunErrorDialog("Невозможно удалить дополнительное соглашение из-за связанных документов не относящихся к текущему заказу.");
					return;
				}
			}

			deletedOrderItems.ForEach(x => Entity.RemoveItem(x));
			var agreementProxy = Entity.Contract.AdditionalAgreements.FirstOrDefault(x => x.Id == agreement.Id);
			if(agreementProxy != null) {
				Entity.Contract.AdditionalAgreements.Remove(agreementProxy);
			}

			//Принудительно сохраняем только, уже сохраненный в базе, заказ, 
			//чтобы пользователь не смог вернуть товары связанные с не существующем доп соглашением, 
			//отменив сохранение заказа
			if(Entity.Id != 0) {
				UoW.Delete<AdditionalAgreement>(agreement);
				UoW.Save();
				UoW.Commit();
			} else {
				using(var deletionUoW = UnitOfWorkFactory.CreateWithoutRoot()) {
					var deletedAgreement = deletionUoW.GetById<AdditionalAgreement>(agreement.Id);
					deletionUoW.Delete<AdditionalAgreement>(deletedAgreement);
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

		protected void OnButtonDelete1Clicked(object sender, EventArgs e)
		{
			OrderItem orderItem = treeItems.GetSelectedObject() as OrderItem;
			if(orderItem == null) {
				return;
			}
			RemoveOrderItem(orderItem);
			//при удалении номенклатуры выделение снимается и при последующем удалении exception
			//для исправления делаем кнопку удаления не активной, если объект не выделился в списке
			buttonDelete1.Sensitive = treeItems.GetSelectedObject() != null;
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
				AddNomenclature(nomenclature, count);
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

		protected void OnYcomboboxReasonItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
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
					refWin = new OrmReference(typeof(PaidRentPackage));
					refWin.Mode = OrmReferenceMode.Select;
					refWin.ObjectSelected += (sender, e) => {
						dlg = new NonFreeRentAgreementDlg(contract, Entity.DeliveryPoint, Entity.DeliveryDate, (e.Subject as PaidRentPackage));
						RunAgreementDialog(dlg);
					};
					TabParent.AddTab(refWin, this);
					break;
				case OrderAgreementType.DailyRent:
					refWin = new OrmReference(typeof(PaidRentPackage));
					refWin.Mode = OrmReferenceMode.Select;
					refWin.ObjectSelected += (sender, e) => {
						dlg = new DailyRentAgreementDlg(contract, Entity.DeliveryPoint, Entity.DeliveryDate, (e.Subject as PaidRentPackage));
						RunAgreementDialog(dlg);
					};
					TabParent.AddTab(refWin, this);
					break;
				case OrderAgreementType.FreeRent:
					refWin = new OrmReference(typeof(FreeRentPackage));
					refWin.Mode = OrmReferenceMode.Select;
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
			var agreement = UoWGeneric.Session.Load<AdditionalAgreement>(e.Agreement.Id);
			Entity.CreateOrderAgreementDocument(agreement);
			Entity.FillItemsFromAgreement(agreement);
			CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType)
										  .AdditionalAgreements
			                              .Add(agreement);
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
			MessageDialog md = new MessageDialog(null,
				DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.YesNo,
												 $"Отсутствует договор с клиентом для формы оплаты '{Entity.PaymentType.GetEnumTitle()}'. Создать?");
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
			dlg.CloseTab += (sender, e) => {
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
						AddNomenclature(nom, count);
					}
				};
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void RunAdditionalAgreementSalesEquipmentDialog(Nomenclature nom = null)
		{
			CounterpartyContract contract = null;
			if(Entity.Contract == null) {
				contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
				if(contract == null) {
					contract = ClientDocumentsRepository.CreateDefaultContract(UoW, Entity.Client, Entity.PaymentType, Entity.DeliveryDate);
					Entity.Contract = contract;
					Entity.AddContractDocument(contract);
				}
			} else {
				contract = Entity.Contract;
			}
			ITdiDialog dlg = new EquipSalesAgreementDlg(
				contract,
				Entity.DeliveryPoint,
				Entity.DeliveryDate,
				nom
			);

			(dlg as IAgreementSaved).AgreementSaved +=
				(sender, e) => {
					AgreementSaved(sender, e);
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
			int index = treeView.Model.IterNChildren() - 1;
			Gtk.TreeIter iter;
			Gtk.TreePath path;

			treeView.Model.IterNthChild(out iter, index);
			path = treeView.Model.GetPath(iter);

			var column = treeView.Columns.First(x => x.Title == "Кол-во");
			var renderer = column.CellRenderers.First();
			Application.Invoke(delegate {
				treeView.SetCursorOnCell(path, column, renderer, true);
			});
			treeView.GrabFocus();
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
			if(pickerDeliveryDate.Date < DateTime.Today && !QSMain.User.Permissions["can_can_create_order_in_advance"])
				pickerDeliveryDate.ModifyBase(StateType.Normal, new Gdk.Color(255, 0, 0));
			else
				pickerDeliveryDate.ModifyBase(StateType.Normal, new Gdk.Color(255, 255, 255));
		}

		protected void OnReferenceClientChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(referenceClient.Subject));
			if(Entity.Client != null) {
				referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, Entity.Client);
				referenceDeliveryPoint.Sensitive = referenceContract.Sensitive = Entity.OrderStatus == OrderStatus.NewOrder;
				referenceContract.RepresentationModel = new ViewModel.ContractsVM(UoW, Entity.Client);

				PaymentType? previousEnum = enumPaymentType.SelectedItem is PaymentType ? ((PaymentType?)enumPaymentType.SelectedItem) : null; 
				var hideEnums = new Enum[] { PaymentType.cashless };
				if(Entity.Client.PersonType == PersonType.natural)
					enumPaymentType.AddEnumToHideList(hideEnums);
				else
					enumPaymentType.ClearEnumHideList();
				if(previousEnum.HasValue) {
					if(previousEnum.Value == Entity.PaymentType) {
						enumPaymentType.SelectedItem = previousEnum.Value;
					} else if(Entity.Id == 0 || hideEnums.Contains(Entity.PaymentType)) {
						enumPaymentType.SelectedItem = Entity.Client.PaymentMethod;
						OnEnumPaymentTypeChanged(null, e);
						Entity.ChangeOrderContract();
					} else {
						enumPaymentType.SelectedItem = Entity.PaymentType;
					}
				}
			} else {
				referenceDeliveryPoint.Sensitive = referenceContract.Sensitive = false;
			}
			SetProxyForOrder();
			UpdateProxyInfo();

			SetSensitivityOfPaymentType();
		}

		protected void OnButtonFillCommentClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(CommentTemplate), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
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
			if(CurrentObjectChanged != null)
				CurrentObjectChanged(this, new CurrentObjectChangedArgs(referenceDeliveryPoint.Subject));
			if(Entity.DeliveryPoint != null) {
				UpdateProxyInfo();
				SetProxyForOrder();
			}
		}

		protected void OnReferenceDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			if(!HaveAgreementForDeliveryPoint()) {
				Order originalOrder = UoW.GetById<Order>(Entity.Id);
				if(originalOrder != null && originalOrder.DeliveryPoint != null) {
					Entity.DeliveryPoint = originalOrder.DeliveryPoint;
				} else {
					Entity.DeliveryPoint = null;
				}
			}

			CheckSameOrders();
		}

		protected void OnButtonPrintSelectedClicked(object c, EventArgs args)
		{
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
				DocumentPrinter.PrintAll(selectedPrintableRDLDocuments);
			}

			var selectedPrintableODTDocuments = treeDocuments.GetSelectedObjects()
				.OfType<IPrintableOdtDocument>().ToList();
			if(selectedPrintableODTDocuments.Any()) {
				TemplatePrinter.PrintAll(selectedPrintableODTDocuments);
			}
		}

		protected void OnBtnOpnPrnDlgClicked(object sender, EventArgs e)
		{
			if(Entity.OrderDocuments.Any(doc => doc.PrintType == PrinterType.RDL || doc.PrintType == PrinterType.ODT))
				TabParent.AddSlaveTab(this, new OrderDocumentsPrinterDlg(Entity));
		}

		protected void OnEnumPaymentTypeChanged(object sender, EventArgs e)
		{
			//при изменении типа платежа вкл/откл кнопку "ожидание оплаты"
			buttonWaitForPayment.Sensitive = IsPaymentTypeBarterOrCashless();

			checkDelivered.Visible = enumDocumentType.Visible = labelDocumentType.Visible =
				(Entity.PaymentType == PaymentType.cashless);

			enumSignatureType.Visible = labelSignatureType.Visible =
				(Entity.Client != null &&
				 (Entity.Client.PersonType == PersonType.legal || Entity.PaymentType == PaymentType.cashless)
				);
			labelOnlineOrder.Visible = entryOnlineOrder.Visible = (Entity.PaymentType == PaymentType.ByCard);
			if(treeItems.Columns.Any())
				treeItems.Columns.First(x => x.Title == "В т.ч. НДС").Visible = Entity.PaymentType == PaymentType.cashless;
			spinSumDifference.Visible = labelSumDifference.Visible = labelSumDifferenceReason.Visible =
				dataSumDifferenceReason.Visible = (Entity.PaymentType == PaymentType.cash || Entity.PaymentType == PaymentType.BeveragesWorld);
			pickerBillDate.Visible = labelBillDate.Visible = Entity.PaymentType == PaymentType.cashless;
			SetProxyForOrder();
			UpdateProxyInfo();
		}

		protected void OnPickerDeliveryDateDateChanged(object sender, EventArgs e)
		{
			SetProxyForOrder();
			UpdateProxyInfo();
		}

		protected void OnPickerDeliveryDateDateChangedByUser(object sender, EventArgs e)
		{
			if(Entity.DeliveryDate.HasValue && Entity.DeliveryDate.Value.Date == DateTime.Today.Date) {
				MessageDialogWorks.RunWarningDialog("Сегодня? Уверены?");
			}
			CheckSameOrders();
			Entity.ChangeOrderContract();
		}

		protected void OnReferenceClientChangedByUser(object sender, EventArgs e)
		{
			//Заполняем точку доставки если она одна.
			if(Entity.Client != null && Entity.Client.DeliveryPoints != null
				&& Entity.OrderStatus == OrderStatus.NewOrder && !Entity.SelfDelivery
				&& Entity.Client.DeliveryPoints.Count == 1) {
				Entity.DeliveryPoint = Entity.Client.DeliveryPoints[0];
			} else {
				Entity.DeliveryPoint = null;
			}
			//Устанавливаем тип документа
			if(Entity.Client != null && Entity.Client.DefaultDocumentType != null) {
				Entity.DocumentType = Entity.Client.DefaultDocumentType;
			} else if(Entity.Client != null) {
				Entity.DocumentType = DefaultDocumentType.upd;
			}

			//Очищаем время доставки
			Entity.DeliverySchedule = null;

			//Устанавливаем тип оплаты
			if(Entity.Client != null) {
				Entity.PaymentType = Entity.Client.PaymentMethod;
				Entity.ChangeOrderContract();
			} else {
				Entity.Contract = null;
			}
		}

		protected void OnButtonCancelOrderClicked(object sender, EventArgs e)
		{
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
		void OpenDlgToCreateNewUndeliveredOrder(){
			UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(Entity, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (sender, e) => {
				Entity.SetUndeliveredStatus(/*e.UndeliveredOrder.GuiltySide*/);
				UpdateButtonState();

				var routeListItem = RouteListItemRepository.GetRouteListItemForOrder(UoW, Entity);
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
			
			Entity.ChangeStatus(OrderStatus.WaitForPayment);
			UpdateButtonState();
		}

		protected void OnButtonCreateDeliveryPointClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrEmpty(Entity.Address1c) || string.IsNullOrEmpty(Entity.Address1cCode))
				return;

			Entity.DeliveryPoint = Entity.Client.DeliveryPoints.FirstOrDefault(x => x.Code1c == Entity.Address1cCode);

			if(Entity.DeliveryPoint != null)
				return;

			DeliveryPointDlg dlg = new DeliveryPointDlg(Entity.Client, Entity.Address1c, Entity.Address1cCode);

			dlg.Entity.HaveResidue = !string.IsNullOrEmpty(Entity.Comment) &&
				(Entity.Comment.ToUpper().Contains("ПЕРВЫЙ ЗАКАЗ") || Entity.Comment.ToUpper().Contains("НОВЫЙ АДРЕС"));
			dlg.EntitySaved += NewDeliveryPointDlg_EntitySaved;
			TabParent.AddSlaveTab(this, dlg);
		}

		void NewDeliveryPointDlg_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			Entity.DeliveryPoint = (e.Entity as DeliveryPoint);
			UpdateButtonState();
		}

		protected void OnEnumDiverCallTypeChanged(object sender, EventArgs e)
		{
			var listDriverCallType = UoW.Session.QueryOver<Order>()
										.Where(x => x.Id == Entity.Id)
										.Select(x => x.DriverCallType).List<DriverCallType>().FirstOrDefault();

			//if(listDriverCallType.Count() == 0)
			//return;

			if(listDriverCallType != (DriverCallType)enumDiverCallType.SelectedItem) {
				var max = UoW.Session.QueryOver<Order>().Select(NHibernate.Criterion.Projections.Max<Order>(x => x.DriverCallId)).SingleOrDefault<int>();
				if(max != 0)
					Entity.DriverCallId = max + 1;
				else
					Entity.DriverCallId = 1;
			}
		}

		protected void OnEntryBottlesReturnChanged(object sender, EventArgs e)
		{
			int result = 0;
			if(Int32.TryParse(entryBottlesToReturn.Text, out result)) {
				Entity.BottlesReturn = result;
			}
		}

		protected void OnEntryTrifleChanged(object sender, EventArgs e)
		{
			int result = 0;
			if(Int32.TryParse(entryTrifle.Text, out result)) {
				Entity.Trifle = result;
			}
		}

		protected void OnShown(object sender, EventArgs e)
		{
			//Скрывает журнал заказов при открытии заказа, чтобы все элементы умещались на экране
			var slider = TabParent as TdiSliderTab;

			if(slider != null)
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

		#endregion

		#region Service functions

		bool HaveAgreementForDeliveryPoint()
		{
			bool a = Entity.HaveActualWaterSaleAgreementByDeliveryPoint();
			if(Entity.ObservableOrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.water) &&
			   !a) {
				//У выбранной точки доставки нет соглашения о доставке воды, предлагаем создать.
				//Если пользователь создаст соглашение, то запишется выбранная точка доставки
				//если не создаст то ничего не произойдет и точка доставки останется прежней
				CounterpartyContract contract;
				if(Entity.Contract != null) {
					contract = Entity.Contract;
				} else {
					contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.Client.PersonType, Entity.PaymentType);
				}
				if(MessageDialogWorks.RunQuestionDialog("В заказе добавлена вода, а для данной точки доставки нет дополнительного соглашения о доставке воды, создать?")) {
					ITdiDialog dlg = new WaterAgreementDlg(contract, Entity.DeliveryPoint, Entity.DeliveryDate);
					(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
					TabParent.AddSlaveTab(this, dlg);
				}
				return false;
			} else {
				return true;
			}
		}

		/// <summary>
		/// Проверка на наличие воды по умолчанию в заказе для выбранной точки доставки и выдача сообщения о возможном штрафе
		/// </summary>
		/// <returns><c>true</c>, если пользователь подтвердил замену воды по умолчанию 
		/// или если для точки доставки не указана вода по умолчанию 
		/// или если среди товаров в заказе имеется вода по умолчанию, <c>false</c> если в заказе среди воды нет воды по умолчанию и 
		/// пользователь не хочет её добавлять в заказ</returns>
		private bool DefaultWaterCheck()
		{
			if(Entity.DeliveryPoint == null)
				return true;
			Nomenclature defaultWater = Entity.DeliveryPoint.DefaultWaterNomenclature;
			var orderWaters = Entity.ObservableOrderItems.Where(w => w.Nomenclature.Category == NomenclatureCategory.water);

			//Если имеется для точки доставки номенклатура по умолчанию, 
			//если имеется вода в заказе и ни одна 19 литровая вода в заказе
			//не совпадает с номенклатурой по умолчанию, то сообщение о штрафе!
			if(defaultWater != null
			   && orderWaters.Any()
			   && !Entity.ObservableOrderItems.Any(i => i.Nomenclature.Category == NomenclatureCategory.water
												   && i.Nomenclature == defaultWater)) {
				string address = Entity.DeliveryPoint.ShortAddress;
				string client = Entity.Client.Name;
				string waterInOrder = "";

				//список вод в заказе за исключением дефолтной для сообщения о штрафе
				foreach(var item in orderWaters) {
					if(item.Nomenclature != defaultWater)
						waterInOrder += String.Format(",\n\t'{0}'", item.Nomenclature.ShortOrFullName);
				}
				//waterInOrder = waterInOrder.Remove(0, 1);//удаление первой запятой
				waterInOrder = waterInOrder.TrimStart(',');
				string title = "Внимание!";
				string header = "Есть риск получить <span foreground=\"Red\" size=\"x-large\">ШТРАФ</span>!\n";
				string text = String.Format("Клиент '{0}' для адреса '{1}' заказывает фиксировано воду \n'{2}'.\nВ заказе же вы указали: {3}. \nДля подтверждения что это не ошибка, нажмите 'Да'.",
											client,
											address,
											defaultWater.ShortOrFullName,
											waterInOrder);
				return MessageDialogWorks.RunWarningDialog(title, header + text);
			}
			return true;
		}

		/// <summary>
		/// Is the payment type barter or cashless?
		/// </summary>
		private bool IsPaymentTypeBarterOrCashless() => Entity.PaymentType == PaymentType.barter || Entity.PaymentType == PaymentType.cashless;


		#endregion

		private bool CanChange {
			get {
				return Entity.OrderStatus == OrderStatus.NewOrder
							 || Entity.OrderStatus == OrderStatus.WaitForPayment;
			}
		}

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
			if(UoW.IsNew && templateOrder == null)
				//открыть окно выбора контрагента
				referenceClient.OpenSelectDialog();
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
			referenceClient.IsEditable = Entity.CanChangeContractor();
		}

		void Entity_ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			treeAnyGoodsFirstColWidth = treeItems.Columns.First(x => x.Title == "Номенклатура").Width;
			treeItems.ExposeEvent += TreeAnyGoods_ExposeEvent;
			//Выполнение в случае если размер не поменяется
			EditGoodsCountCellOnAdd(treeItems);
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
				case LastChosenAction.DailyRentAgreement:
					AddRentAgreement(OrderAgreementType.DailyRent);
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
			if(item.Nomenclature.Category == NomenclatureCategory.water) {
				Entity.RecalcBottlesDeposits(UoWGeneric);
			}
			if((item.Nomenclature.Category == NomenclatureCategory.deposit || item.Nomenclature.Category == NomenclatureCategory.rent)
				 && item.Price != 0)
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
				AgreementType.DailyRent,
				AgreementType.FreeRent,
				AgreementType.NonfreeRent,
				AgreementType.EquipmentSales
			};
			buttonDelete1.Sensitive = items.Length > 0 && ((items[0] as OrderItem).AdditionalAgreement == null
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
		/// </summary>
		void ObservableOrderItems_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(!(aList is GenericObservableList<OrderItem>)) {
				return;
			}
			foreach(var i in aIdx) {
				OrderItem oItem = (aList as GenericObservableList<OrderItem>)[aIdx] as OrderItem;
				if(oItem == null || oItem.PaidRentEquipment == null) {
					return;
				}
				if(oItem.Nomenclature.Category == NomenclatureCategory.rent
				  || oItem.Nomenclature.Category == NomenclatureCategory.equipment) {
					ChangeEquipmentsCount(oItem, oItem.Count);
				}
			}
		}

		/// <summary>
		/// При изменении количества оборудования в списке оборудования меняет его 
		/// также в доп. соглашении и списке товаров заказа
		/// </summary>
		void ObservableOrderEquipments_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(!(aList is GenericObservableList<OrderEquipment>)) {
				return;
			}
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

		/// <summary>
		/// Меняет количество оборудования в списке оборудования заказа, в списке 
		/// товаров заказа, в списке оборудования дополнитульного соглашения и 
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

		private void IsUIEditable(bool val = true)
		{
			if(Entity.Client != null) {
				enumPaymentType.Sensitive = val;
			} else {
				enumPaymentType.Sensitive = false;
			}
			referenceDeliverySchedule.Sensitive = referenceDeliveryPoint.IsEditable =
				referenceClient.IsEditable = val;
			enumAddRentButton.Sensitive = enumSignatureType.Sensitive =// enumStatus.Sensitive = 
				enumDocumentType.Sensitive = val;
			buttonAddDoneService.Sensitive = buttonAddServiceClaim.Sensitive =
				buttonAddForSale.Sensitive = val;
			checkDelivered.Sensitive = checkSelfDelivery.Sensitive = val;
			pickerDeliveryDate.Sensitive = val;
			dataSumDifferenceReason.Sensitive = val;
			treeItems.Sensitive = val;
			enumDiscountUnit.Visible = spinDiscount.Visible = labelDiscont.Visible = vseparatorDiscont.Visible = val;
			tblOnRouteEditReason.Sensitive = val;
			ChangeOrderEditable(val);

			buttonAddForSale.Sensitive = referenceContract.Sensitive = buttonAddMaster.Sensitive = enumAddRentButton.Sensitive = !Entity.IsLoadedFrom1C;
		}

		void ChangeOrderEditable(bool val)
		{
			SetPadInfoSensitive(val);
			vboxGoods.Sensitive = val;
			buttonAddExistingDocument.Sensitive = val;
			btnAddM2ProxyForThisOrder.Sensitive = val;
			btnRemExistingDocument.Sensitive = val;
		}

		void SetPadInfoSensitive(bool value)
		{
			foreach(var widget in table1.Children) {
				if(widget.Name == vboxOrderComment.Name) {
					widget.Sensitive = true;
				}else {
					widget.Sensitive = value;
				}
			}
		}

		void SetSensitivityOfPaymentType()
		{
			if(chkContractCloser.Active) {
				Entity.PaymentType = PaymentType.cashless;
				enumPaymentType.Sensitive = false;
			} else {
				enumPaymentType.Sensitive = CanChange;
			}
		}

		void UpdateButtonState()
		{
			IsUIEditable(Entity.OrderStatus == OrderStatus.NewOrder);
			if(Entity.OrderStatus == OrderStatus.Accepted || Entity.OrderStatus == OrderStatus.Canceled || Entity.OrderStatus == OrderStatus.OnTheWay) {
				var icon = new Image();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}
			if(Entity.OrderStatus == OrderStatus.NewOrder) {
				var icon = new Image();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Подтвердить";
			}

			//если новый заказ и тип платежа бартер или безнал, то вкл кнопку
			buttonWaitForPayment.Sensitive = (Entity.OrderStatus == OrderStatus.NewOrder && IsPaymentTypeBarterOrCashless());

			buttonCancelOrder.Sensitive = OrderRepository.GetStatusesForOrderCancelation().Contains(Entity.OrderStatus);
			buttonAccept.Sensitive = new OrderStatus[] {
				OrderStatus.NewOrder,
				OrderStatus.WaitForPayment,
				OrderStatus.Accepted,
				OrderStatus.Canceled
			}.Contains(Entity.OrderStatus)
			 || (Entity.OrderStatus == OrderStatus.OnTheWay && QSMain.User.Permissions["can_edit_on_the_way_order"]);
			
			if(Counterparty?.DeliveryPoints?.FirstOrDefault(d => d.Address1c == Entity.Address1c) == null
				&& !string.IsNullOrWhiteSpace(Entity.Address1c)
				&& DeliveryPoint == null) {
				buttonCreateDeliveryPoint.Sensitive = true;
			} else
				buttonCreateDeliveryPoint.Sensitive = false;

			ButtonCloseOrderAccessibilityAndAppearance();
		}

		void UpdateProxyInfo()
		{
			bool canShow = Entity.Client != null && Entity.DeliveryDate.HasValue &&
								 (Entity.Client?.PersonType == PersonType.legal || Entity.PaymentType == PaymentType.cashless);

			labelProxyInfo.Visible = canShow;

			DBWorks.SQLHelper text = new DBWorks.SQLHelper("");
			if(canShow) {
				var proxies = Entity.Client.Proxies.Where(p => p.IsActiveProxy(Entity.DeliveryDate.Value) && (p.DeliveryPoints == null || p.DeliveryPoints.Count() == 0 || p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, Entity.DeliveryPoint))));
				foreach(var proxy in proxies) {
					if(!String.IsNullOrWhiteSpace(text.Text))
						text.Add("\n");
					text.Add(String.Format("Доверенность{2} №{0} от {1:d}", proxy.Number, proxy.IssueDate,
						proxy.DeliveryPoints == null ? "(общая)" : ""));
					text.StartNewList(": ");
					foreach(var pers in proxy.Persons) {
						text.AddAsList(pers.NameWithInitials);
					}
				}
			}
			if(String.IsNullOrWhiteSpace(text.Text))
				labelProxyInfo.Markup = "<span foreground=\"red\">Нет активной доверенности</span>";
			else
				labelProxyInfo.LabelProp = text.Text;
		}

		private void CheckSameOrders()
		{
			if(!Entity.DeliveryDate.HasValue || Entity.DeliveryPoint == null) {
				return;
			}

			var sameOrder = OrderRepository.GetOrderOnDateAndDeliveryPoint(UoW, Entity.DeliveryDate.Value, Entity.DeliveryPoint);
			if(sameOrder != null && templateOrder == null) {
				MessageDialogWorks.RunWarningDialog("На выбранную дату и точку доставки уже есть созданный заказ!");
			}
		}

		void SetDiscountEditable(bool? canEdit = null)
		{
			spinDiscount.Sensitive = canEdit.HasValue ? canEdit.Value : enumDiscountUnit.SelectedItem != null;
		}

		void SetDiscountUnitEditable(bool? canEdit = null){
			enumDiscountUnit.Sensitive = canEdit.HasValue ? canEdit.Value : ycomboboxReason.SelectedItem != null;
		}

		/// <summary>
		/// Переключает видимость элементов управления депозитами
		/// </summary>
		/// <param name="visibly"><see langword="true"/>если хотим принудительно сделать видимым;
		/// <see langword="false"/>если хотим принудительно сделать невидимым;
		/// <see langword="null"/>переключает видимость с невидимого на видимый и обратно.</param>
		private void ToggleVisibilityOfDeposits(bool? visibly = null)
		{
			depositrefunditemsview.Visible = visibly.HasValue ? visibly.Value : !depositrefunditemsview.Visible;
			labelDeposit1.Visible = visibly.HasValue ? visibly.Value : !labelDeposit1.Visible;
		}

		private void SetProxyForOrder()
		{
			if(Entity.Client != null
			   && Entity.DeliveryDate.HasValue
			   && (Entity.Client?.PersonType == PersonType.legal || Entity.PaymentType == PaymentType.cashless)) {
				var proxies = Entity.Client.Proxies.Where(p => p.IsActiveProxy(Entity.DeliveryDate.Value) && (p.DeliveryPoints == null || p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, Entity.DeliveryPoint))));
				if(proxies.Count() > 0) {
					enumSignatureType.SelectedItem = OrderSignatureType.ByProxy;
				}
			}
		}

		private void SetDiscount()
		{
			DiscountReason reason = (ycomboboxReason.SelectedItem as DiscountReason);
			DiscountUnits unit = (DiscountUnits)enumDiscountUnit.SelectedItem;
			decimal discount = 0;
			if(Decimal.TryParse(spinDiscount.Text, out discount)) {
				if(reason == null && discount > 0) {
					MessageDialogWorks.RunErrorDialog("Необходимо выбрать основание для скидки");
					return;
				}
				Entity.SetDiscountUnitsForAll(unit);
				Entity.SetDiscount(reason, discount, unit);
			}
		}

		private bool HaveEmailForBill()
		{
			QSContacts.Email clientEmail = Entity.Client.Emails.FirstOrDefault(x => x.EmailType == null || (x.EmailType.Name == "Для счетов"));
			if(clientEmail == null) {
				if(MessageDialogWorks.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?")){
					return true;
				}else {
					return false;
				}
			}
			return true;
		}

		private void SendBillByEmail(QSContacts.Email emailAddressForBill)
		{
			if(!EmailServiceSetting.CanSendEmail || EmailRepository.HaveSendedEmail(Entity.Id, OrderDocumentType.Bill)){
				return;
			}

			var billDocument = Entity.OrderDocuments.FirstOrDefault(x => x.Type == OrderDocumentType.Bill) as BillDocument;
			if(billDocument == null) {
				MessageDialogWorks.RunErrorDialog("Невозможно отправить счет по электронной почте. Счет не найден.");
				return;
			}
			var organization = OrganizationRepository.GetCashlessOrganization(UnitOfWorkFactory.CreateWithoutRoot());
			if(organization == null) {
				MessageDialogWorks.RunErrorDialog("Невозможно отправить счет по электронной почте. В параметрах базы не определена организация для безналичного расчета");
				return;
			}
			var wasHideSignature = billDocument.HideSignature;
			billDocument.HideSignature = false;
			ReportInfo ri = billDocument.GetReportInfo();
			billDocument.HideSignature = wasHideSignature;

			var billTemplate = billDocument.GetEmailTemplate();
			Email email = new Email();
			email.Title = string.Format("{0} {1}", billTemplate.Title, billDocument.Title);
			email.Text = billTemplate.Text;
			email.HtmlText = billTemplate.TextHtml;
			email.Recipient = new EmailContact("", emailAddressForBill.Address);
			email.Sender = new EmailContact("vodovoz-spb.ru", MainSupport.BaseParameters.All["email_for_email_delivery"]);
			email.Order = Entity.Id;
			email.OrderDocumentType = OrderDocumentType.Bill;
			foreach(var item in billTemplate.Attachments) {
				email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
			}
			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
				string billDate = billDocument.DocumentDate.HasValue ? "_" + billDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
				email.AddAttachment($"Bill_{billDocument.Order.Id}{billDate}.pdf", stream);
			}
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var employee = EmployeeRepository.GetEmployeeForCurrentUser(uow);
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
			MessageDialogWorks.RunInfoDialog(resultMessage + result.Item2);
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
				QSContacts.Email clientEmail = Entity.Client.Emails.FirstOrDefault(x => x.EmailType == null || (x.EmailType.Name == "Для счетов"));
				if(clientEmail == null) {
					clientEmail = Entity.Client.Emails.FirstOrDefault();
				}
				email = clientEmail.Address;
			}
			senddocumentbyemailview1.Update(selectedDoc, email);
		}
	}
}