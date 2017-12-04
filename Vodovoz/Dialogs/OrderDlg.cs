﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate.Proxy;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSTDI;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Service;
using Vodovoz.Panel;
using Vodovoz.Repository;
using QSDocTemplates;
using Vodovoz.JournalFilters;
using Vodovoz.Domain.Operations;
using System.Data.Bindings.Collections.Generic;
using Gamma.GtkWidgets.Cells;

namespace Vodovoz
{
	public partial class OrderDlg : OrmGtkDialogBase<Order>, ICounterpartyInfoProvider, IDeliveryPointInfoProvider
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		List<Nomenclature> fixedPrices = new List<Nomenclature>();

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		LastChosenAction lastChosenAction = LastChosenAction.None;

		/// <summary>
		/// Ширина колонки "Номенклатура" списка товаров 
		/// (создано для храннения ширины колонки до автосайза ячейки по 
		/// содержимому, чтобы отобразить по правильному положению ввод 
		/// количества при добавлении нового товара)
		/// </summary>
		private int treeItemsNomenclatureColWidth;

		private enum LastChosenAction //
		{
			None,
			NonFreeRentAgreement,
			DailyRentAgreement,
			FreeRentAgreement,
		}

		public PanelViewType[] InfoWidgets
		{
			get
			{
				return new[]{
					PanelViewType.CounterpartyView, 
					PanelViewType.DeliveryPointView, 
					PanelViewType.AdditionalAgreementPanelView 
				};
			}
		}

		public Counterparty Counterparty
		{
			get
			{
				return referenceClient.Subject as Counterparty;
			}
		}

		public DeliveryPoint DeliveryPoint
		{
			get
			{
				return referenceDeliveryPoint.Subject as DeliveryPoint;
			}
		}

		public OrderDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Order> ();
			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if (Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать создавать заказы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}
			UoWGeneric.Root.OrderStatus = OrderStatus.NewOrder;
			TabName = "Новый заказ";
			ConfigureDlg ();
		}

		public OrderDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order> (id);
			ConfigureDlg ();
		}

		public OrderDlg (Order sub) : this (sub.Id)
		{
		}

		public void ConfigureDlg ()
		{
			treeDocuments.Selection.Mode=SelectionMode.Multiple;
			if (UoWGeneric.Root.PreviousOrder != null) {
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
//TODO Make it clickable.
			} else
				labelPreviousOrder.Visible = false;
			hboxStatusButtons.Visible = (UoWGeneric.Root.OrderStatus == OrderStatus.NewOrder 
				|| UoWGeneric.Root.OrderStatus == OrderStatus.Accepted || Entity.OrderStatus == OrderStatus.Canceled);

	//		var fixedPrices = GetFixedPriceList(Entity.DeliveryPoint);

			treeDocuments.ItemsDataSource = UoWGeneric.Root.ObservableOrderDocuments;
			treeItems.ItemsDataSource = UoWGeneric.Root.ObservableOrderItems;
			treeEquipment.ItemsDataSource = UoWGeneric.Root.ObservableOrderEquipments;
			//treeEquipmentFromClient.ItemsDataSource = UoWGeneric.Root.ObservableOrderEquipments;
			treeDepositRefundItems.ItemsDataSource = UoWGeneric.Root.ObservableOrderDepositItems;
			treeServiceClaim.ItemsDataSource = UoWGeneric.Root.ObservableInitialOrderService;
			//TODO FIXME Добавить в таблицу закрывающие заказы.

			//Подписывемся на изменения листов для засеривания клиента
			Entity.ObservableOrderDocuments.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableFinalOrderService.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableInitialOrderService.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableOrderItems.ElementAdded += Entity_ObservableOrderItems_ElementAdded;
			Entity.ObservableOrderDocuments.ElementAdded += Entity_ObservableOrderDocuments_ElementAdded;

			//Подписываемся на изменение товара, для обновления количества оборудования в доп. соглашении
			Entity.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged_ChangeCount;
			Entity.ObservableOrderEquipments.ElementChanged += ObservableOrderEquipments_ElementChanged_ChangeCount;

			enumSignatureType.ItemsEnum = typeof(OrderSignatureType);
			enumSignatureType.Binding.AddBinding (Entity, s => s.SignatureType, w => w.SelectedItem).InitializeFromSource ();

			ylabelOrderStatus.Binding.AddFuncBinding(Entity, e => e.OrderStatus.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelNumber.Binding.AddFuncBinding (Entity, e => e.Code1c + (e.DailyNumber.HasValue ? $" ({e.DailyNumber})" : ""), w => w.LabelProp).InitializeFromSource();

			enumDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDocumentType.Binding.AddBinding (Entity, s => s.DocumentType, w => w.SelectedItem).InitializeFromSource ();

			pickerDeliveryDate.Binding.AddBinding (Entity, s => s.DeliveryDate, w => w.DateOrNull).InitializeFromSource ();

			textComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();

			checkSelfDelivery.Binding.AddBinding(Entity, s => s.SelfDelivery, w => w.Active).InitializeFromSource();
			checkDelivered.Binding.AddBinding(Entity, s => s.Shipped, w => w.Active).InitializeFromSource();

			ycheckbuttonCollectBottles.Binding.AddBinding(Entity, s => s.CollectBottles, w => w.Active).InitializeFromSource();

			entryBottlesReturn.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryBottlesReturn.Binding.AddBinding(Entity, e => e.BottlesReturn, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			entryTrifle.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryTrifle.Binding.AddBinding(Entity, e => e.Trifle, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			referenceContract.Binding.AddBinding(Entity, e => e.Contract, w => w.Subject).InitializeFromSource();

			yentryAddress1cDeliveryPoint.Binding.AddBinding(Entity, e => e.Address1c, w => w.Text).InitializeFromSource();
			yentryAddress1cDeliveryPoint.Binding.AddBinding(Entity, e => e.Address1c, w => w.TooltipText).InitializeFromSource();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.RestrictIncludeCustomer = true;
			counterpartyFilter.RestrictIncludeSupplier = false;
			counterpartyFilter.RestrictIncludePartner = true;
			counterpartyFilter.RestrictIncludeArhive = false;
			referenceClient.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceClient.Binding.AddBinding(Entity, s => s.Client, w => w.Subject).InitializeFromSource();

			referenceDeliverySchedule.ItemsQuery = DeliveryScheduleRepository.AllQuery();
			referenceDeliverySchedule.SetObjectDisplayFunc<DeliverySchedule>(e => e.Name);
			referenceDeliverySchedule.Binding.AddBinding(Entity, s => s.DeliverySchedule, w => w.Subject).InitializeFromSource();
			referenceDeliverySchedule.Binding.AddBinding(Entity, s => s.DeliverySchedule1c, w => w.TooltipText).InitializeFromSource();

			referenceAuthor.ItemsQuery = EmployeeRepository.ActiveEmployeeOrderedQuery();
			referenceAuthor.SetObjectDisplayFunc<Employee>(e => e.ShortName);
			referenceAuthor.Binding.AddBinding(Entity, s => s.Author, w => w.Subject).InitializeFromSource();
			referenceAuthor.Sensitive = false;

			referenceDeliveryPoint.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceDeliveryPoint.Sensitive = (UoWGeneric.Root.Client != null);

			buttonViewDocument.Sensitive = false;
			buttonDelete1.Sensitive = false;
			//			enumStatus.Sensitive = false;
			notebook1.ShowTabs = false;
			notebook1.Page = 0;

			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);

			commentsview4.UoW = UoWGeneric;

			#region Events
			treeDocuments.Selection.Changed += (sender, e) => {
				buttonViewDocument.Sensitive = treeDocuments.Selection.CountSelectedRows() > 0;
			};

			treeDocuments.RowActivated += (o, args) => buttonViewDocument.Click();

			enumAddRentButton.ItemsEnum = typeof(OrderAgreementType);
			enumAddRentButton.EnumItemClicked += (sender, e) => AddRentAgreement((OrderAgreementType)e.ItemEnum);

			checkSelfDelivery.Toggled += (sender, e) => {
				referenceDeliverySchedule.Sensitive = labelDeliverySchedule.Sensitive = !checkSelfDelivery.Active;
			};

			UoWGeneric.Root.ObservableOrderItems.ElementChanged += (aList, aIdx) => {
				FixPrice(aIdx[0]);
			};

			UoWGeneric.Root.ObservableOrderItems.ElementAdded += (aList, aIdx) => {
				FixPrice(aIdx[0]);
			};

			UoWGeneric.Root.ObservableOrderDepositItems.ListContentChanged += (sender, e) => {
				UpdateVisibleOfWingets();
			};

			UoWGeneric.Root.ObservableOrderDepositItems.ElementAdded += (aList, aIdx) => {
				UpdateVisibleOfWingets();
			};

			Entity.ObservableOrderDepositItems.ListChanged += delegate (object aList) {
				UpdateVisibleOfWingets();
			};

			treeItems.Selection.Changed += TreeItems_Selection_Changed;
			treeDepositRefundItems.Selection.Changed += TreeDepositRefundItems_Selection_Changed;
			#endregion
			dataSumDifferenceReason.Binding.AddBinding(Entity, s => s.SumDifferenceReason, w => w.Text).InitializeFromSource();
			dataSumDifferenceReason.Completion = new EntryCompletion();
			dataSumDifferenceReason.Completion.Model = OrderRepository.GetListStoreSumDifferenceReasons(UoWGeneric);
			dataSumDifferenceReason.Completion.TextColumn = 0;

			spinSumDifference.Binding.AddBinding(Entity, e => e.ExtraMoney, w => w.ValueAsDecimal).InitializeFromSource();

			labelSum.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.TotalSum), w => w.LabelProp).InitializeFromSource();
			labelCashToReceive.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.SumToReceive), w => w.LabelProp).InitializeFromSource();

			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorBlue = new Gdk.Color(0, 0, 0xff);
			var colorGreen = new Gdk.Color(0, 0xff, 0);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);

			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderItem>()
				.AddColumn("Номенклатура").SetDataProperty(node => node.NomenclatureString)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
				.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddSetter((c, node) => c.Editable = node.CanEditAmount).WidthChars(10)
				.AddTextRenderer(node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Сроки аренды").AddTextRenderer(node => GetRentsCount(node))
				.AddColumn("Цена").AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
				.AddSetter((c, node) => c.Editable = Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category))
				.AddSetter((NodeCellRendererSpin<OrderItem> c, OrderItem node) => {
					AdditionalAgreement aa = node.AdditionalAgreement.Self;
					if(aa is WaterSalesAgreement &&
					  (aa as WaterSalesAgreement).IsFixedPrice) {
						c.ForegroundGdk = colorGreen;
					} else if(node.HasUserSpecifiedPrice() &&
					  Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category)) {
						c.ForegroundGdk = colorBlue;
					} else {
						c.ForegroundGdk = colorBlack;
					}
				})
				.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("В т.ч. НДС").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS))
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("Скидка %").AddNumericRenderer(node => node.Discount)
				.Adjustment(new Adjustment(0, 0, 100, 1, 100, 1)).Editing(true)
				.AddColumn("Доп. соглашение").SetDataProperty(node => node.AgreementString)
				.RowCells()
				.Finish();

			treeEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").SetDataProperty(node => node.NameString)
				.AddColumn("Направление").SetDataProperty(node => node.DirectionString)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
				.AddColumn("")
				.Finish();

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<OrderDocument>()
				.AddColumn("Документ").SetDataProperty(node => node.Name)
				.AddColumn("Дата").SetDataProperty(node => node.DocumentDate)
				.Finish();

			treeDepositRefundItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderDepositItem>()
				.AddColumn("Тип").SetDataProperty(node => node.DepositTypeString)
				.AddColumn("Количество").AddNumericRenderer(node => node.Count).Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1)).Editing(true)
				.AddColumn("Цена").AddNumericRenderer(node => node.Deposit).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("Сумма").AddNumericRenderer(node => node.Total)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Visible = n.PaymentDirection == PaymentDirection.ToClient)
				.Finish();

			treeServiceClaim.ColumnsConfig = ColumnsConfigFactory.Create<ServiceClaim>()
				.AddColumn("Статус заявки").SetDataProperty(node => node.Status.GetEnumTitle())
				.AddColumn("Номенклатура оборудования").SetDataProperty(node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn("Серийный номер").SetDataProperty(node => node.Equipment != null && node.Equipment.Nomenclature.IsSerial ? node.Equipment.Serial : "-")
				.AddColumn("Причина").SetDataProperty(node => node.Reason)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();
			treeServiceClaim.Selection.Changed += TreeServiceClaim_Selection_Changed;

			enumPaymentType.ItemsEnum = typeof(PaymentType);
			enumPaymentType.Binding.AddBinding(Entity, s => s.PaymentType, w => w.SelectedItem).InitializeFromSource();

			textManagerComments.Binding.AddBinding(Entity, s => s.CommentManager, w => w.Buffer.Text).InitializeFromSource();
			enumDiverCallType.ItemsEnum = typeof(DriverCallType);
			enumDiverCallType.Binding.AddBinding(Entity, s => s.DriverCallType, w => w.SelectedItem).InitializeFromSource();

			referenceDriverCallId.Binding.AddBinding(Entity, e => e.DriverCallId, w => w.Subject).InitializeFromSource();
			textTaraComments.Binding.AddBinding(Entity, e => e.InformationOnTara, w => w.Buffer.Text).InitializeFromSource();
			enumareRasonType.ItemsEnum = typeof(ReasonType);
			enumareRasonType.Binding.AddBinding(Entity, s => s.ReasonType, w => w.SelectedItem).InitializeFromSource();


			UpdateVisibleOfWingets();
			UpdateButtonState();

			if(Entity.DeliveryPoint == null && !string.IsNullOrWhiteSpace(Entity.Address1c)) {
				var deliveryPoint = Counterparty.DeliveryPoints.FirstOrDefault(d => d.Address1c == Entity.Address1c);
				if(deliveryPoint != null)
					Entity.DeliveryPoint = deliveryPoint;
			}

			if(UoWGeneric.Root.OrderStatus != OrderStatus.NewOrder)
				IsUIEditable(true);
			
			OrderItemEquipmentCountHasChanges = false;

			ButtonCloseOrderSensitivity();
		}

		string GetRentsCount(OrderItem orderItem)
		{
			switch(orderItem.AdditionalAgreement.Type) {
				case AgreementType.NonfreeRent:
					if(orderItem.NonFreeRentAgreement.RentMonths.HasValue){
						return orderItem.NonFreeRentAgreement.RentMonths.Value.ToString();
					}
					return "";
				case AgreementType.DailyRent:
					return orderItem.DailyRentAgreement.RentDays.ToString();
			}
			return "";
		}

		void Entity_UpdateClientCanChange(object aList, int[] aIdx)
		{
			referenceClient.Sensitive = Entity.CanChangeContractor();
		}

		void Entity_ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			treeItemsNomenclatureColWidth = treeItems.Columns.First(x => x.Title == "Номенклатура").Width;
			treeItems.ExposeEvent += TreeItems_ExposeEvent;
			//Выполнение в случае если размер не поменяется
			EditItemCountCellOnAdd();
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

		void TreeServiceClaim_Selection_Changed(object sender, EventArgs e)
		{
			buttonOpenServiceClaim.Sensitive = treeServiceClaim.Selection.CountSelectedRows() > 0;
		}

		void FixPrice(int id)
		{
			OrderItem item = UoWGeneric.Root.ObservableOrderItems[id];
			if(item.Nomenclature.Category == NomenclatureCategory.water) {
				UoWGeneric.Root.RecalcBottlesDeposits(UoWGeneric);
			}
			if((item.Nomenclature.Category == NomenclatureCategory.deposit || item.Nomenclature.Category == NomenclatureCategory.rent)
				 && item.Price != 0)
				return;
			if(!item.HasUserSpecifiedPrice())
				item.Price = item.DefaultPrice;
		}

		void TreeItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeItems.GetSelectedObjects();

			if(items.Length == 0) {
				return;
			}

			if(treeDepositRefundItems.GetSelectedObjects().Length > 0) {
				treeDepositRefundItems.Selection.UnselectAll();
			}

			buttonDelete1.Sensitive = items.Length > 0 && ((items[0] as OrderItem).AdditionalAgreement == null || (items[0] as OrderItem).Nomenclature.Category == NomenclatureCategory.water
														  || (items[0] as OrderItem).AdditionalAgreement.Type == AgreementType.DailyRent
														  || (items[0] as OrderItem).AdditionalAgreement.Type == AgreementType.FreeRent
														  || (items[0] as OrderItem).AdditionalAgreement.Type == AgreementType.NonfreeRent);
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



		void TreeDepositRefundItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeDepositRefundItems.GetSelectedObjects();

			if(items.Length == 0) {
				return;
			}

			if(treeItems.GetSelectedObjects().Length > 0) {
				treeItems.Selection.UnselectAll();
			}

			buttonDelete1.Sensitive = items.Length > 0;
		}

		public override bool Save()
		{

			var valid = new QSValidator<Order>(UoWGeneric.Root);
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
			SaveChanges();
			UoWGeneric.Save();

			if (Entity.OrderDocuments.Count() > 0)
				{
					string whatToPrint = "Распечатать " +
											Entity.OrderDocuments.Count() +
												  (Entity.OrderDocuments.Count() > 1 ? " документов?" : " документ?");
					if (MessageDialogWorks.RunQuestionDialog(whatToPrint))
					{
						PrintDocuments(Entity.OrderDocuments);
					}
				} 

			logger.Info("Ok.");
			ButtonCloseOrderSensitivity();
			return true;
		}

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
		}

		#endregion

		protected void OnReferenceClientChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(referenceClient.Subject));
			if(UoWGeneric.Root.Client != null) {
				referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, Entity.Client);
				referenceDeliveryPoint.Sensitive = referenceContract.Sensitive = UoWGeneric.Root.OrderStatus == OrderStatus.NewOrder;
				referenceContract.RepresentationModel = new ViewModel.ContractsVM(UoW, Entity.Client);
			} else {
				referenceDeliveryPoint.Sensitive = referenceContract.Sensitive = false;
			}
			SetProxyForOrder();
			//	UpdateProxyInfo();
		}

		private void IsUIEditable(bool val = true)
		{
			referenceDeliverySchedule.Sensitive = referenceDeliveryPoint.IsEditable =
				referenceClient.IsEditable = val;
			enumAddRentButton.Sensitive = enumSignatureType.Sensitive =// enumStatus.Sensitive = 
				enumPaymentType.Sensitive = enumDocumentType.Sensitive = val;
			buttonAddDoneService.Sensitive = buttonAddServiceClaim.Sensitive =
				buttonAddForSale.Sensitive = val;
			//spinBottlesReturn.Sensitive = spinSumDifference.Sensitive = val;
			checkDelivered.Sensitive = checkSelfDelivery.Sensitive = val;
			textComments.Sensitive = val;
			pickerDeliveryDate.Sensitive = val;
			dataSumDifferenceReason.Sensitive = val;
			treeItems.Sensitive = val;

			yspinDiscountOrder.Visible = buttonSetDiscount.Visible = labelDiscont.Visible = vseparatorDiscont.Visible = val;
		}

		protected void OnButtonDelete1Clicked(object sender, EventArgs e)
		{
			Entity.RemoveItem(UoWGeneric, treeItems.GetSelectedObject() as OrderItem);
		}

		protected void OnButtonAddForSaleClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.Root.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			if(UoWGeneric.Root.DeliveryDate == null) {
				MessageDialogWorks.RunErrorDialog("Введите дату доставки");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.NomenCategory = NomenclatureCategory.water;
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Номенклатура на продажу";
			SelectDialog.ObjectSelected += NomenclatureForSaleSelected;
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

		void AddNomenclature(Nomenclature nomenclature)
		{
			if(UoWGeneric.Root.OrderItems.Any(x => x.Nomenclature.NoDelivey == true) && nomenclature.NoDelivey == false) {
				MessageDialogWorks.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}

			if(UoWGeneric.Root.OrderItems.Any(x => x.Nomenclature.NoDelivey == false) && nomenclature.NoDelivey == true) {
				MessageDialogWorks.RunInfoDialog("Услуга без доставки должна добавляться в новый заказ");
				return;
			}

			if(nomenclature.Category == NomenclatureCategory.equipment) {
				UoWGeneric.Root.AddEquipmentNomenclatureForSale(nomenclature, UoWGeneric);
			} else if(nomenclature.Category == NomenclatureCategory.water) {
				CounterpartyContract contract = CounterpartyContractRepository.
					GetCounterpartyContractByPaymentType(UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType);
				if(contract == null) {
					var result = AskCreateContract();
					switch(result) {
						case (int)ResponseType.Yes:
							RunContractAndWaterAgreementDialog(nomenclature);
							break;
						case (int)ResponseType.Accept:
							CreateDefaultContractWithAgreement(nomenclature);
							break;
						default:
							break;
					}
					return;
				}
				UoWGeneric.Session.Refresh(contract);
				WaterSalesAgreement wsa = contract.GetWaterSalesAgreement(UoWGeneric.Root.DeliveryPoint, nomenclature);
				if(wsa == null) {
					//Если нет доп. соглашения продажи воды.
					if(MessageDialogWorks.RunQuestionDialog("Отсутствует доп. соглашение с клиентом для продажи воды. Создать?")) {
						RunAdditionalAgreementWaterDialog();
					} else
						return;
				} else {
					UoWGeneric.Root.AddWaterForSale(nomenclature, wsa);
					UoWGeneric.Root.RecalcBottlesDeposits(UoWGeneric);
				}
			} else
				UoWGeneric.Root.AddAnyGoodsNomenclatureForSale(nomenclature);

			if(nomenclature.NoDelivey == true)
				UoWGeneric.Root.IsService = true;
			else
				UoWGeneric.Root.IsService = false;


		}

		private void AddRentAgreement(OrderAgreementType type)
		{
			ITdiDialog dlg;

			if(UoWGeneric.Root.Client == null || UoWGeneric.Root.DeliveryPoint == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления оборудования должна быть выбрана точка доставки.");
				return;
			}
			CounterpartyContract contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType);
			if(contract == null) {
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
				RunContractCreateDialog();
				return;
			}
			switch(type) {
				case OrderAgreementType.NonfreeRent:
					dlg = new NonFreeRentAgreementDlg(contract, UoWGeneric.Root.DeliveryPoint, UoWGeneric.Root.DeliveryDate);
					break;
				case OrderAgreementType.DailyRent:
					dlg = new DailyRentAgreementDlg(contract, UoWGeneric.Root.DeliveryPoint, UoWGeneric.Root.DeliveryDate);
					break;
				default:
					dlg = new FreeRentAgreementDlg(contract, UoWGeneric.Root.DeliveryPoint, UoWGeneric.Root.DeliveryDate);
					break;
			}
			(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
			TabParent.AddSlaveTab(this, dlg);
		}

		void AgreementSaved(object sender, AgreementSavedEventArgs e)
		{
			var agreement = UoWGeneric.Session.Merge(e.Agreement);
			var orderAgreement = UoWGeneric.Root.ObservableOrderDocuments.OfType<OrderAgreement>()
			          .FirstOrDefault(x => x.AdditionalAgreement == agreement);
			if(orderAgreement == null) {
				UoWGeneric.Root.ObservableOrderDocuments.Add(new OrderAgreement {
					Order = UoWGeneric.Root,
					AdditionalAgreement = agreement
				});
			}
			UoWGeneric.Root.FillItemsFromAgreement(agreement);
			CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType).AdditionalAgreements.Add(agreement);
		}

		void UpdateVisibleOfWingets()
		{
			bool visibleDeposit = Entity.OrderDepositItems.Count > 0;
			treeDepositRefundItems.Visible = labelDeposit.Visible = visibleDeposit;
		}

		protected void OnButtonViewDocumentClicked(object sender, EventArgs e)
		{
			if(treeDocuments.GetSelectedObjects().GetLength(0) > 0) {
				ITdiDialog dlg = null;
				if(treeDocuments.GetSelectedObjects()[0] is OrderAgreement) {
					var agreement = (treeDocuments.GetSelectedObjects()[0] as OrderAgreement).AdditionalAgreement;
					var type = NHibernateProxyHelper.GuessClass(agreement);
					var dialog = OrmMain.CreateObjectDialog(type, agreement.Id);
					if(dialog is IAgreementSaved) {
						(dialog as IAgreementSaved).AgreementSaved += AgreementSaved;
					}
					TabParent.OpenTab(
						OrmMain.GenerateDialogHashName(type, agreement.Id),
						() => dialog
					);
				} else if(treeDocuments.GetSelectedObjects()[0] is OrderContract) {
					var contract = (treeDocuments.GetSelectedObjects()[0] as OrderContract).Contract;
					dlg = OrmMain.CreateObjectDialog(contract);
				}

				if(dlg != null) {
					(dlg as IEditableDialog).IsEditable = false;
					TabParent.AddSlaveTab(this, dlg);
				}
			}

			var selectedPrintableDocuments = treeDocuments.GetSelectedObjects().Cast<OrderDocument>()
				.Where(doc => doc.PrintType != PrinterType.None).ToList();
			if(selectedPrintableDocuments.Count > 0) {
				string whatToPrint = selectedPrintableDocuments.Count > 1
					? "документов"
					: "документа \"" + selectedPrintableDocuments.First().Type.GetEnumTitle() + "\"";
				if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Order), whatToPrint))
					UoWGeneric.Save();
				selectedPrintableDocuments.ForEach(
					doc => TabParent.AddTab(DocumentPrinter.GetPreviewTab(doc), this, false)
				);
			}
		}

		protected void OnButtonFillCommentClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(CommentTemplate), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += (s, ea) => {
				if(ea.Subject != null) {
					UoWGeneric.Root.Comment = (ea.Subject as CommentTemplate).Comment;
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

		void RunContractCreateDialog()
		{
			ITdiTab dlg;
			var response = AskCreateContract();
			if(response == (int)ResponseType.Yes) {
				dlg = new CounterpartyContractDlg(UoWGeneric.Root.Client,
					OrganizationRepository.GetOrganizationByPaymentType(UoWGeneric, UoWGeneric.Root.PaymentType),
					UoWGeneric.Root.DeliveryDate);
				(dlg as IContractSaved).ContractSaved += OnContractSaved;
				TabParent.AddSlaveTab(this, dlg);
			} else if(response == (int)ResponseType.Accept) {
				var contract = CreateDefaultContract();
				AddContractDocument(contract);
				Entity.Contract = contract;
			}
		}

		protected int AskCreateContract()
		{
			MessageDialog md = new MessageDialog(null,
				DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.YesNo,
				"Отсутствует договор с клиентом для " +
				(UoWGeneric.Root.PaymentType == PaymentType.cash ? "наличной" : "безналичной") +
				" формы оплаты. Создать?");
			md.SetPosition(WindowPosition.Center);
			md.AddButton("Автоматически", ResponseType.Accept);
			md.ShowAll();
			var result = md.Run();
			md.Destroy();
			return result;
		}

		protected void RunContractAndWaterAgreementDialog(Nomenclature nomenclature)
		{
			ITdiTab dlg = new CounterpartyContractDlg(UoWGeneric.Root.Client,
							  OrganizationRepository.GetOrganizationByPaymentType(UoWGeneric, UoWGeneric.Root.PaymentType),
							  UoWGeneric.Root.DeliveryDate);
			(dlg as IContractSaved).ContractSaved += OnContractSaved;
			dlg.CloseTab += (sender, e) => {
				CounterpartyContract contract =
					CounterpartyContractRepository.GetCounterpartyContractByPaymentType(
						UoWGeneric,
						UoWGeneric.Root.Client,
						UoWGeneric.Root.PaymentType);
				if(contract != null) {
					bool hasWaterAgreement = contract.GetWaterSalesAgreement(UoWGeneric.Root.DeliveryPoint, nomenclature) != null;
					if(!hasWaterAgreement)
						RunAdditionalAgreementWaterDialog();
				}
			};
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnContractSaved(object sender, ContractSavedEventArgs args)
		{
			UoWGeneric.Root.ObservableOrderDocuments.Add(new OrderContract {
				Order = UoWGeneric.Root,
				Contract = args.Contract
			});
			Entity.Contract = args.Contract;
		}

		protected void RunAdditionalAgreementWaterDialog()
		{
			ITdiDialog dlg = new WaterAgreementDlg(CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType), UoWGeneric.Root.DeliveryPoint, UoWGeneric.Root.DeliveryDate);
			(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void CreateDefaultContractWithAgreement(Nomenclature nomenclature)
		{
			var contract = CreateDefaultContract();
			Entity.Contract = contract;
			AddContractDocument(contract);
			AdditionalAgreement agreement = contract.GetWaterSalesAgreement(UoWGeneric.Root.DeliveryPoint, nomenclature);
			if(agreement == null) {
				agreement = CreateDefaultWaterAgreement(contract);
				contract.AdditionalAgreements.Add(agreement);
				AddAgreementDocument(agreement);
			}
		}

		protected CounterpartyContract CreateDefaultContract()
		{
			CounterpartyContract result;
			using(var uow = CounterpartyContract.Create(UoWGeneric.Root.Client)) {
				var contract = uow.Root;
				contract.Organization = OrganizationRepository
					.GetOrganizationByPaymentType(UoWGeneric, UoWGeneric.Root.PaymentType);
				contract.IsArchive = false;
				if(UoWGeneric.Root.DeliveryDate.HasValue)
					contract.IssueDate = UoWGeneric.Root.DeliveryDate.Value;
				contract.AdditionalAgreements = new List<AdditionalAgreement>();
				uow.Save();
				result = uow.Root;
			}
			return result;
		}

		protected AdditionalAgreement CreateDefaultWaterAgreement(CounterpartyContract contract)
		{
			AdditionalAgreement result;
			using(var uow = WaterSalesAgreement.Create(contract)) {
				AdditionalAgreement agreement = uow.Root;
				agreement.Contract = contract;
				agreement.AgreementNumber = WaterSalesAgreement.GetNumberWithType(contract, AgreementType.WaterSales);
				if(UoWGeneric.Root.DeliveryDate.HasValue) {
					agreement.IssueDate = UoWGeneric.Root.DeliveryDate.Value;
					agreement.StartDate = UoWGeneric.Root.DeliveryDate.Value;
				}
				result = uow.Root;
				uow.Save();
			}
			return result;
		}

		protected void AddContractDocument(CounterpartyContract contract)
		{
			Order order = UoWGeneric.Root;
			var orderDocuments = UoWGeneric.Root.ObservableOrderDocuments;
			orderDocuments.Add(new OrderContract {
				Order = order,
				Contract = contract
			});
		}

		protected void AddAgreementDocument(AdditionalAgreement agreement)
		{
			Order order = UoWGeneric.Root;
			var orderDocuments = UoWGeneric.Root.ObservableOrderDocuments;
			orderDocuments.Add(new OrderAgreement {
				Order = order,
				AdditionalAgreement = agreement
			});
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{



			if(UoWGeneric.Root.OrderStatus == OrderStatus.NewOrder) {
				var valid = new QSValidator<Order>(UoWGeneric.Root,
								new Dictionary<object, object> {
						{ "NewStatus", OrderStatus.Accepted }
					});
				if(valid.RunDlgIfNotValid((Window)this.Toplevel))
					return;

#if !SHORT

				if (UoWGeneric.Root.BottlesReturn == 0 && Entity.OrderItems.Any (i => i.Nomenclature.Category == NomenclatureCategory.water)) {
					if (!MessageDialogWorks.RunQuestionDialog ("Указано нулевое количество бутылей на возврат. Вы действительно хотите продолжить?"))
						return;
				}

#endif
				 
				foreach(OrderItem item in UoWGeneric.Root.ObservableOrderItems) {
					if(item.Nomenclature.Category == NomenclatureCategory.equipment && item.Nomenclature.IsSerial) {
						int[] alreadyAdded = UoWGeneric.Root.OrderEquipments
							.Where(orderEquipment => orderEquipment.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
							.Where(orderEquipment => orderEquipment.Equipment != null)
							.Select(orderEquipment => orderEquipment.Equipment.Id).ToArray();
						int equipmentCount = UoWGeneric.Root.OrderEquipments.Count(orderEquipment => orderEquipment?.Equipment?.Nomenclature?.Id == item.Nomenclature.Id);
						int equipmentToAddCount = item.Count - equipmentCount;
						var equipmentToAdd = EquipmentRepository.GetEquipmentForSaleByNomenclature(UoW, item.Nomenclature, equipmentToAddCount, alreadyAdded);
						for(int i = 0; i < equipmentToAddCount; i++) {
							UoWGeneric.Root.ObservableOrderEquipments.Add(new OrderEquipment {
								Order = UoWGeneric.Root,
								Direction = Vodovoz.Domain.Orders.Direction.Deliver,
								Equipment = equipmentToAdd[i],
								OrderItem = item,
								Reason = Reason.Sale
							});
						}
						for(; equipmentCount > item.Count; equipmentCount--) {
							UoWGeneric.Root.ObservableOrderEquipments.Remove(
								UoWGeneric.Root.ObservableOrderEquipments.Where(orderEquipment => orderEquipment.Reason == Reason.Sale && orderEquipment.Equipment.Nomenclature.Id == item.Nomenclature.Id).First()
							);
						}
					}
				}

				DailyNumberIncrement();

				UoWGeneric.Root.OrderStatus = OrderStatus.Accepted;
				UoWGeneric.Root.UpdateDocuments();

				treeItems.Selection.UnselectAll();
				treeEquipment.Selection.UnselectAll();
				treeDepositRefundItems.Selection.UnselectAll();
				UpdateButtonState();
				buttonSave.Click();
				return;
			}
			if(Entity.OrderStatus == OrderStatus.Accepted || Entity.OrderStatus == OrderStatus.Canceled) {
				Entity.ChangeStatus(OrderStatus.NewOrder);
				UpdateButtonState();
				return;
			}
		}

		private void DailyNumberIncrement()
		{
			var todayLastNumber = UoW.Session.QueryOver<Order>()
													 .Select(NHibernate.Criterion.Projections.Max<Order>(x => x.DailyNumber))
													 .Where(d => d.DeliveryDate == DateTime.Now.Date)
													 .SingleOrDefault<int>();


			if(Entity.DailyNumber == null) {
				if(todayLastNumber != 0)
					Entity.DailyNumber = todayLastNumber + 1;
				else
					Entity.DailyNumber = 1;
			}
		}

		void SaveChanges()
		{
			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
		}

		void UpdateButtonState()
		{
			IsUIEditable(Entity.OrderStatus == OrderStatus.NewOrder);
			if(Entity.OrderStatus == OrderStatus.Accepted || Entity.OrderStatus == OrderStatus.Canceled) {
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

			buttonWaitForPayment.Sensitive = Entity.OrderStatus == OrderStatus.NewOrder;
			buttonCancelOrder.Sensitive = Entity.OrderStatus == OrderStatus.Accepted
										  || Entity.OrderStatus == OrderStatus.NewOrder
										  || Entity.OrderStatus == OrderStatus.WaitForPayment;

			if(Counterparty?.DeliveryPoints?.FirstOrDefault(d => d.Address1c == Entity.Address1c) == null
				&& !string.IsNullOrWhiteSpace(Entity.Address1c)
				&& DeliveryPoint == null) {
				buttonCreateDeliveryPoint.Sensitive = true;
			} else
				buttonCreateDeliveryPoint.Sensitive = false;
		}

		protected void OnEnumSignatureTypeChanged(object sender, EventArgs e)
		{
			UpdateProxyInfo();
		}

		void UpdateProxyInfo()
		{
			labelProxyInfo.Visible = Entity.SignatureType == OrderSignatureType.ByProxy;
			if(Entity.SignatureType != OrderSignatureType.ByProxy)
				return;
			DBWorks.SQLHelper text = new DBWorks.SQLHelper("");
			if(Entity.Client != null && Entity.DeliveryDate.HasValue) {
				var proxies = Entity.Client.Proxies.Where(p => p.IsActiveProxy(Entity.DeliveryDate.Value) && (p.DeliveryPoints == null || p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, Entity.DeliveryPoint))));
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

		protected void OnReferenceDeliveryPointChanged(object sender, EventArgs e)
		{
			if(CurrentObjectChanged != null)
				CurrentObjectChanged(this, new CurrentObjectChangedArgs(referenceDeliveryPoint.Subject));
			if(Entity.DeliveryPoint != null) {
				UpdateProxyInfo();
				fixedPrices = GetFixedPriceList(Entity.DeliveryPoint);
				SetProxyForOrder();
			}
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
			if(selectedPrintableRDLDocuments.Count > 0) {
				DocumentPrinter.PrintAll(selectedPrintableRDLDocuments);
			}

			var selectedPrintableODTDocuments = treeDocuments.GetSelectedObjects()
				.OfType<ITemplatePrntDoc>().ToList();
			if(selectedPrintableODTDocuments.Count > 0) {
				TemplatePrinter.PrintAll(selectedPrintableODTDocuments);
			}

		}

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
			if(!SaveOrderBeforeContinue())
				return;
			var dlg = new ServiceClaimDlg(UoWGeneric.Root);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonAddDoneServiceClicked(object sender, EventArgs e)
		{
			if(!SaveOrderBeforeContinue())
				return;
			OrmReference SelectDialog = new OrmReference(typeof(ServiceClaim), UoWGeneric,
											ServiceClaimRepository.GetDoneClaimsForClient(UoWGeneric.Root)
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
							   UoWGeneric.Root.Client,
							   UoWGeneric.Root.PaymentType);
			if(!contract.RepairAgreementExists()) {
				RunAgreementCreateDialog(contract);
				return;
			}
			selected.FinalOrder = UoWGeneric.Root;
			UoWGeneric.Root.ObservableFinalOrderService.Add(selected);
			//TODO Add service nomenclature with price.
		}

		bool SaveOrderBeforeContinue()
		{
			if(UoWGeneric.IsNew) {
				if(CommonDialogs.SaveBeforeCreateSlaveEntity(EntityObject.GetType(), typeof(ServiceClaim))) {
					if(!Save())
						return false;
				} else
					return false;
			}
			return true;
		}

		void RunAgreementCreateDialog(CounterpartyContract contract)
		{
			ITdiTab dlg;
			string question = "Отсутствует доп. соглашение сервиса с клиентом в договоре для " +
							  (UoWGeneric.Root.PaymentType == PaymentType.cash ? "наличной" : "безналичной") +
							  " формы оплаты. Создать?";
			if(MessageDialogWorks.RunQuestionDialog(question)) {
				dlg = new RepairAgreementDlg(contract);
				(dlg as IAgreementSaved).AgreementSaved += (sender, e) => UoWGeneric.Root.ObservableOrderDocuments.Add(
					new OrderAgreement {
						Order = UoWGeneric.Root,
						AdditionalAgreement = e.Agreement
					});
				TabParent.AddSlaveTab(this, dlg);
			}
		}

		protected void OnEnumPaymentTypeChanged(object sender, EventArgs e)
		{
			enumSignatureType.Visible = checkDelivered.Visible = labelSignatureType.Visible =
				enumDocumentType.Visible = labelDocumentType.Visible =
				(Entity.PaymentType == PaymentType.cashless);

			treeItems.Columns.First(x => x.Title == "В т.ч. НДС").Visible = Entity.PaymentType != PaymentType.cash;
			spinSumDifference.Visible = labelSumDifference.Visible = labelSumDifferenceReason.Visible =
				dataSumDifferenceReason.Visible = Entity.PaymentType == PaymentType.cash;
		}

		protected void OnPickerDeliveryDateDateChanged(object sender, EventArgs e)
		{
			SetProxyForOrder();
			//	UpdateProxyInfo();
		}

		protected void OnReferenceClientChangedByUser(object sender, EventArgs e)
		{
			//Заполняем точку доставки если она одна.
			if(Entity.Client != null && Entity.Client.DeliveryPoints != null
				&& Entity.OrderStatus == OrderStatus.NewOrder && !Entity.SelfDelivery
				&& Entity.Client.DeliveryPoints.Count == 1) {
				Entity.DeliveryPoint = Entity.Client.DeliveryPoints[0];
			}else {
				Entity.DeliveryPoint = null;
			}
			//Устанавливаем тип документа
			if(Entity.Client != null && Entity.Client.DefaultDocumentType != null) {
				Entity.DocumentType = Entity.Client.DefaultDocumentType;
			} else if(Entity.Client != null) {
				Entity.DocumentType = DefaultDocumentType.upd;
			}

			//Выбираем конракт, если он один у контрагента
			if(Entity.Client.CounterpartyContracts.Count == 1) {
				Entity.Contract = Entity.Client.CounterpartyContracts.FirstOrDefault();
			}else {
				Entity.Contract = null;
			}

			//Очищаем время доставки
			Entity.DeliverySchedule = null;

			//Устанавливаем тип оплаты
			if(Entity.Client != null) {
				Entity.PaymentType = Entity.Client.PaymentMethod;
				OnEnumPaymentTypeChangedByUser(null, EventArgs.Empty);
			} else {
				Entity.Contract = null;
			}
		}

		protected void OnButtonOpenServiceClaimClicked(object sender, EventArgs e)
		{
			var claim = treeServiceClaim.GetSelectedObject<ServiceClaim>();
			OpenTab(
				OrmGtkDialogBase<ServiceClaim>.GenerateHashName(claim.Id),
				() => new ServiceClaimDlg(claim)
			);
		}

		protected void OnButtonCancelOrderClicked(object sender, EventArgs e)
		{
			var valid = new QSValidator<Order>(UoWGeneric.Root,
				new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.Canceled }
			});
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;

			Entity.ChangeStatus(OrderStatus.Canceled);
			UpdateButtonState();
		}

		protected void OnEnumPaymentTypeChangedByUser(object sender, EventArgs e)
		{
			var org = OrganizationRepository.GetOrganizationByPaymentType(UoW, Entity.PaymentType);
			if((Entity.Contract == null || Entity.Contract.Organization.Id != org.Id) && Entity.Client != null)
				Entity.Contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoWGeneric, Entity.Client, Entity.PaymentType);

			//Изменяем организацию в документах.
			foreach(var doc in Entity.OrderDocuments.OfType<OrderContract>()) {
				if(doc.Contract.Organization != org) {
					doc.Contract.Organization = org;
					UoW.Save(doc.Contract.Organization);
					Entity.Contract = doc.Contract;
				}
			}
		}

		protected void OnButtonSetDiscountClicked(object sender, EventArgs e)
		{
			int discount = (int)yspinDiscountOrder.Value;
			foreach(OrderItem item in UoWGeneric.Root.ObservableOrderItems) {
				item.Discount = discount;
			}
		}

		protected void OnButtonWaitForPaymentClicked(object sender, EventArgs e)
		{
			var valid = new QSValidator<Order>(UoWGeneric.Root,
				new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.WaitForPayment }
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
			dlg.EntitySaved += Dlg_EntitySaved;
			TabParent.AddSlaveTab(this, dlg);
		}

		void Dlg_EntitySaved(object sender, EntitySavedEventArgs e)
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

		/// <summary>
		/// Распечатать документы.
		/// </summary>
		/// <param name="docList">Лист документов.</param>
		private void PrintDocuments(IList<OrderDocument> docList)
		{
			if(docList.Count > 0) {
				DocumentPrinter.PrintAll(docList);
			}
		}

		/// <summary>
		/// Получить список номенклатур с фиксированной ценой для данной точки доставки.
		/// </summary>
		/// <returns>Лист номенклатур с фиксированной ценой.</returns>
		/// <param name="delPoint">Точка доставки.</param>
		private List<Nomenclature> GetFixedPriceList(DeliveryPoint delPoint)
		{
			var agreements = AdditionalAgreementRepository.GetActiveAgreementsForDeliveryPoint(UoW, delPoint);

			List<WaterSalesAgreementFixedPrice> fixedPricesList = new List<WaterSalesAgreementFixedPrice>();
			List<Nomenclature> nomenclature = new List<Nomenclature>();

			foreach(AdditionalAgreement agreement in agreements) {
				var fixedPrices = WaterSalesAgreementFixedPriceRepository.GetFixedPricesForAgreement(UoW, agreement);
				fixedPricesList.AddRange(fixedPrices);
			}

			foreach(WaterSalesAgreementFixedPrice fixedPrice in fixedPricesList) {
				nomenclature.Add(fixedPrice.Nomenclature);
			}

			return nomenclature;
		}

		private void SetProxyForOrder()
		{
			if(Entity.Client != null
			   && Entity.DeliveryDate.HasValue) {
				var proxies = Entity.Client.Proxies.Where(p => p.IsActiveProxy(Entity.DeliveryDate.Value) && (p.DeliveryPoints == null || p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, Entity.DeliveryPoint))));
				if(proxies.Count() > 0) {
					enumSignatureType.SelectedItem = OrderSignatureType.ByProxy;
				}
				UpdateProxyInfo();
			}
		}

		protected void OnButtonCloseOrderClicked(object sender, EventArgs e)
		{
			if(!MessageDialogWorks.RunQuestionDialog("Вы уверены, что хотите закрыть заказ?")) {
				return;
			}

			foreach(OrderItem item in Entity.OrderItems) {
				item.ActualCount = item.Count;
			}

			int amountDelivered = Entity.OrderItems
					.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
					.Sum(item => item.ActualCount);

			if(Entity.BottlesMovementOperation == null) {
				if(amountDelivered != 0 || (Entity.ReturnedTare != 0 && Entity.ReturnedTare != null)) {
					var bottlesMovementOperation = new BottlesMovementOperation {
						OperationTime = Entity.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
						Order = Entity,
						Delivered = amountDelivered,
						Returned = Entity.ReturnedTare.GetValueOrDefault(),
						Counterparty = Entity.Client,
						DeliveryPoint = Entity.DeliveryPoint
					};
					UoW.Save(bottlesMovementOperation);
					Entity.BottlesMovementOperation = bottlesMovementOperation;
				}
			}

			Entity.ChangeStatus(OrderStatus.Closed);
			ButtonCloseOrderSensitivity();
		}

		void ButtonCloseOrderSensitivity()
		{
			buttonCloseOrder.Sensitive = QSMain.User.Permissions["can_close_orders"]
											&& Entity.OrderStatus >= OrderStatus.Accepted
											&& Entity.OrderStatus != OrderStatus.Closed;
		}

		/// <summary>
		/// Активирует редактирование ячейки количества
		/// </summary>
		private void EditItemCountCellOnAdd()
		{
			int index = treeItems.Model.IterNChildren() - 1;
			Gtk.TreeIter iter;
			Gtk.TreePath path;

			treeItems.Model.IterNthChild(out iter, index);
			path = treeItems.Model.GetPath(iter);

			var column = treeItems.Columns.First(x => x.Title == "Кол-во");
			var renderer = column.CellRenderers.First();
			Application.Invoke(delegate {
				treeItems.SetCursorOnCell(path, column, renderer, true);
			});
			treeItems.GrabFocus();
		}

		void TreeItems_ExposeEvent(object o, ExposeEventArgs args)
		{
			var newColWidth = treeItems.Columns.First(x => x.Title == "Номенклатура").Width;
			if(treeItemsNomenclatureColWidth != newColWidth) {
				EditItemCountCellOnAdd();
				treeItems.ExposeEvent -= TreeItems_ExposeEvent;
			}
		}

		public void FillOrderItems(Order order)
		{
			if(Entity.ObservableOrderItems.Count > 0 && !MessageDialogWorks.RunQuestionDialog("Вы уверены, что хотите удалить все позиции текущего из заказа и заполнить его позициями из выбранного?")) {
				return;
			}

			Entity.ClearOrderItemsList();
			foreach(OrderItem orderItem in order.OrderItems) {
				switch(orderItem.Nomenclature.Category) {
					case NomenclatureCategory.equipment:
						Entity.AddEquipmentNomenclatureForSaleFromPreviousOrder(orderItem, UoWGeneric);
						continue;
					case NomenclatureCategory.water:
						CounterpartyContract contract = CounterpartyContractRepository.
						GetCounterpartyContractByPaymentType(UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType);
						if(contract == null) {
							/*	var result = AskCreateContract();
								switch (result)
								{
									case (int)ResponseType.Yes:
										RunContractAndWaterAgreementDialog(orderItem.Nomenclature);
										break;
									case (int)ResponseType.Accept:
										CreateDefaultContractWithAgreement(orderItem.Nomenclature);
										break;
									default:
										break;
								} */
							continue;
						}
						UoWGeneric.Session.Refresh(contract);
						WaterSalesAgreement wsa = contract.GetWaterSalesAgreement(UoWGeneric.Root.DeliveryPoint, orderItem.Nomenclature);
						if(wsa == null) {
							//Если нет доп. соглашения продажи воды.
							if(MessageDialogWorks.RunQuestionDialog("Отсутствует доп. соглашение с клиентом для продажи воды. Создать?")) {
								RunAdditionalAgreementWaterDialog();
							} else
								continue;
						} else {
							Entity.AddWaterForSaleFromPreviousOrder(orderItem, wsa);
							UoWGeneric.Root.RecalcBottlesDeposits(UoWGeneric);
						}
						continue;
					default:
						Entity.AddAnyGoodsNomenclatureForSaleFromPreviousOrder(orderItem);
						continue;
				}
			}
		}

		protected void OnButtonbuttonAddEquipmentToClientClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.Root.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.NomenCategory = NomenclatureCategory.equipment;
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование к клиенту";
			SelectDialog.ObjectSelected += NomenclatureToClient;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureToClient(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			AddNomenclatureToClient(UoWGeneric.Session.Get<Nomenclature>(e.ObjectId));
		}

		void AddNomenclatureToClient(Nomenclature nomenclature)
		{
			UoWGeneric.Root.AddEquipmentNomenclatureToClient(nomenclature, UoWGeneric);
		}

		protected void OnButtonAddEquipmentFromClientClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.Root.Client == null) {
				MessageDialogWorks.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.NomenCategory = NomenclatureCategory.equipment;
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование от клиенту";
			SelectDialog.ObjectSelected += NomenclatureFromClient;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureFromClient(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			AddNomenclatureFromClient(UoWGeneric.Session.Get<Nomenclature>(e.ObjectId));
		}

		void AddNomenclatureFromClient(Nomenclature nomenclature)
		{
			UoWGeneric.Root.AddEquipmentNomenclatureFromClient(nomenclature, UoWGeneric);
		}

		protected void OnButtonDeleteEquipmentClicked(object sender, EventArgs e)
		{
			UoWGeneric.Root.DeleteEquipment(treeEquipment.GetSelectedObject() as OrderEquipment);
		}

		protected void OnEntryBottlesReturnChanged(object sender, EventArgs e)
		{
			int result = 0;
			if(Int32.TryParse(entryBottlesReturn.Text,out result)) {
				Entity.BottlesReturn = result;
				UoWGeneric.Root.RecalcBottlesDeposits(UoWGeneric);
			}
		}

		protected void OnEntryTrifleChanged(object sender, EventArgs e)
		{
			int result = 0;
			if(Int32.TryParse(entryTrifle.Text, out result)) {
				Entity.Trifle = result;
			}
		}
	}
}