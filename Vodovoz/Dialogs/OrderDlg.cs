using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gtk;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Repository;
using QSSupportLib;
using Gamma.GtkWidgets;
using Vodovoz.Domain.Orders.Documents;
using Gamma.Utilities;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrderDlg : OrmGtkDialogBase<Order>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

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
			buttonAccept.Visible = (UoWGeneric.Root.OrderStatus == OrderStatus.NewOrder || UoWGeneric.Root.OrderStatus == OrderStatus.Accepted);
			if (UoWGeneric.Root.OrderStatus == OrderStatus.Accepted) {
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
			}

			treeDocuments.ItemsDataSource = UoWGeneric.Root.ObservableOrderDocuments;
			treeItems.ItemsDataSource = UoWGeneric.Root.ObservableOrderItems;
			treeEquipment.ItemsDataSource = UoWGeneric.Root.ObservableOrderEquipments;
			treeDepositRefundItems.ItemsDataSource = UoWGeneric.Root.ObservableOrderDepositItems;
			treeServiceClaim.ItemsDataSource = UoWGeneric.Root.ObservableInitialOrderService;
			//TODO FIXME Добавить в таблицу закрывающие заказы.

			//Подписывемся на изменения листов для засеривания клинета
			Entity.ObservableOrderDocuments.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableFinalOrderService.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableInitialOrderService.ElementAdded += Entity_UpdateClientCanChange;

			enumSignatureType.ItemsEnum = typeof(OrderSignatureType);
			enumSignatureType.Binding.AddBinding (Entity, s => s.SignatureType, w => w.SelectedItem).InitializeFromSource ();

			enumPaymentType.ItemsEnum = typeof(PaymentType);
			enumPaymentType.Binding.AddBinding (Entity, s => s.PaymentType, w => w.SelectedItem).InitializeFromSource ();

			enumStatus.ItemsEnum = typeof(OrderStatus);
			enumStatus.Binding.AddBinding (Entity, s => s.OrderStatus, w => w.SelectedItem).InitializeFromSource ();

			pickerDeliveryDate.Binding.AddBinding (Entity, s => s.DeliveryDate, w => w.Date).InitializeFromSource ();

			textComments.Binding.AddBinding (Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource ();

			checkSelfDelivery.Binding.AddBinding (Entity, s => s.SelfDelivery, w => w.Active).InitializeFromSource ();
			checkDelivered.Binding.AddBinding (Entity, s => s.Shipped, w => w.Active).InitializeFromSource ();

			spinBottlesReturn.Binding.AddBinding (Entity, s => s.BottlesReturn, w => w.ValueAsInt).InitializeFromSource ();

			referenceClient.ItemsQuery = CounterpartyRepository.ActiveClientsQuery ();
			referenceClient.SetObjectDisplayFunc<Counterparty> (e => e.Name);
			referenceClient.Binding.AddBinding (Entity, s => s.Client, w => w.Subject).InitializeFromSource ();

			referenceDeliverySchedule.ItemsQuery = DeliveryScheduleRepository.AllQuery ();
			referenceDeliverySchedule.SetObjectDisplayFunc<DeliverySchedule> (e => e.Name);
			referenceDeliverySchedule.Binding.AddBinding (Entity, s => s.DeliverySchedule, w => w.Subject).InitializeFromSource ();

			referenceAuthor.ItemsQuery = EmployeeRepository.ActiveEmployeeOrderedQuery ();
			referenceAuthor.SetObjectDisplayFunc<Employee> (e => e.ShortName);
			referenceAuthor.Binding.AddBinding (Entity, s => s.Author, w => w.Subject).InitializeFromSource ();
			referenceAuthor.Sensitive = false;

			referenceDeliveryPoint.Binding.AddBinding (Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource ();
			referenceDeliveryPoint.Sensitive = (UoWGeneric.Root.Client != null);

			buttonViewDocument.Sensitive = false;
			buttonDelete.Sensitive = false;
			enumStatus.Sensitive = false;
			notebook1.ShowTabs = false;
			notebook1.Page = 0;

			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);

			#region Events
			treeDocuments.Selection.Changed += (sender, e) => {
				buttonViewDocument.Sensitive = treeDocuments.Selection.CountSelectedRows () > 0;
			};

			treeDocuments.RowActivated += (o, args) => buttonViewDocument.Click ();

			enumAddRentButton.EnumItemClicked += (sender, e) => AddRentAgreement ((OrderAgreementType)e.ItemEnum);
				
			checkSelfDelivery.Toggled += (sender, e) => {
				referenceDeliverySchedule.Sensitive = labelDeliverySchedule.Sensitive = !checkSelfDelivery.Active;
			};

			UoWGeneric.Root.ObservableOrderItems.ElementChanged += (aList, aIdx) => {
				FixPrice (aIdx [0]);
				enumPaymentType.Sensitive = UoWGeneric.Root.CanChangePaymentType ();
			};

			UoWGeneric.Root.ObservableOrderItems.ElementAdded += (aList, aIdx) => { 
				FixPrice (aIdx [0]); 			
				enumPaymentType.Sensitive = UoWGeneric.Root.CanChangePaymentType ();
			};

			UoWGeneric.Root.ObservableOrderItems.ListContentChanged += (sender, e) => {
				UpdateSum ();
				enumPaymentType.Sensitive = UoWGeneric.Root.CanChangePaymentType ();
			};

			UoWGeneric.Root.ObservableOrderDepositItems.ListContentChanged += (sender, e) => {
				UpdateSum ();
				enumPaymentType.Sensitive = UoWGeneric.Root.CanChangePaymentType ();
			};

			UoWGeneric.Root.ObservableFinalOrderService.ElementAdded += (aList, aIdx) => {
				enumPaymentType.Sensitive = UoWGeneric.Root.CanChangePaymentType ();
				UpdateSum ();
			};
			
			UoWGeneric.Root.ObservableOrderDepositItems.ElementAdded += (aList, aIdx) => {
				enumPaymentType.Sensitive = UoWGeneric.Root.CanChangePaymentType ();
				UpdateSum ();
			};

			treeItems.Selection.Changed += TreeItems_Selection_Changed;
			#endregion
			dataSumDifferenceReason.Binding.AddBinding (Entity, s => s.SumDifferenceReason, w => w.Text).InitializeFromSource ();
			dataSumDifferenceReason.Completion = new EntryCompletion ();
			dataSumDifferenceReason.Completion.Model = OrderRepository.GetListStoreSumDifferenceReasons (UoWGeneric);
			dataSumDifferenceReason.Completion.TextColumn = 0;

			spinSumDifference.Value = (double)(UoWGeneric.Root.SumToReceive - UoWGeneric.Root.TotalSum);

			var colorBlack = new Gdk.Color (0, 0, 0);
			var colorBlue = new Gdk.Color (0, 0, 0xff);

			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderItem> ()
				.AddColumn ("Номенклатура").SetDataProperty (node => node.NomenclatureString)
				.AddColumn ("Кол-во").AddNumericRenderer (node => node.Count)
				.Adjustment (new Adjustment (0, 0, 1000000, 1, 100, 0))
				.AddSetter ((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddSetter ((c, node) => c.Editable = node.CanEditAmount)
				.WidthChars (10)
				.AddTextRenderer (node => node.Nomenclature.Unit == null ? String.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn ("Цена").AddNumericRenderer (node => node.Price).Digits (2)
				.Adjustment (new Adjustment (0, 0, 1000000, 1, 100, 0)).Editing (true)
				.AddSetter((c,node)=>c.ForegroundGdk = node.HasUserSpecifiedPrice() && node.Nomenclature.Category==NomenclatureCategory.water ? colorBlue: colorBlack)
				.AddSetter((c,node)=>c.Editable = node.Nomenclature.Category==NomenclatureCategory.water)
				.AddTextRenderer (node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn ("Сумма").AddTextRenderer (node => CurrencyWorks.GetShortCurrencyString (node.Price * node.Count))
				.AddColumn ("Доп. соглашение").SetDataProperty (node => node.AgreementString)
				.Finish ();

			treeEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment> ()
				.AddColumn ("Наименование").SetDataProperty (node => node.NameString)
				.AddColumn ("Направление").SetDataProperty (node => node.DirectionString)
				.AddColumn ("Причина").SetDataProperty (node => node.ReasonString)
				.Finish ();

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<OrderDocument> ()
				.AddColumn ("Документ").SetDataProperty (node => node.Name)
				.AddColumn ("Дата").SetDataProperty (node => node.DocumentDate)
				.Finish ();

			treeDepositRefundItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderDepositItem> ()
				.AddColumn ("Тип").SetDataProperty (node => node.DepositTypeString)
				.AddColumn ("Количество").AddNumericRenderer (node => node.Count)
				.AddColumn ("Цена").AddNumericRenderer (node => node.Deposit)
				.AddColumn ("Сумма").AddNumericRenderer (node => node.Total)
				.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Visible = n.PaymentDirection == PaymentDirection.ToClient)
				.Finish ();

			treeServiceClaim.ColumnsConfig = ColumnsConfigFactory.Create<ServiceClaim> ()
				.AddColumn ("Статус заявки").SetDataProperty (node => node.Status.GetEnumTitle ())
				.AddColumn ("Номенклатура оборудования").SetDataProperty (node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn ("Серийный номер").SetDataProperty (node => node.Equipment != null ? node.Equipment.Serial : "-")
				.AddColumn ("Причина").SetDataProperty (node => node.Reason)
				.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
				.Finish ();
			
			UpdateSum ();

			enumPaymentType.Sensitive = UoWGeneric.Root.CanChangePaymentType ();

			if (UoWGeneric.Root.OrderStatus != OrderStatus.NewOrder)
				IsEditable ();
		}

		void Entity_UpdateClientCanChange (object aList, int[] aIdx)
		{
			referenceClient.Sensitive = Entity.CanChangeContractor ();
		}

		void FixPrice (int id)
		{
			OrderItem item = UoWGeneric.Root.ObservableOrderItems [id];
			if (item.Nomenclature.Category == NomenclatureCategory.water) {
				UoWGeneric.Root.RecalcBottlesDeposits (UoWGeneric);
			}
			if ((item.Nomenclature.Category == NomenclatureCategory.deposit || item.Nomenclature.Category == NomenclatureCategory.rent)
			     && item.Price != 0)
				return;
			if(!item.HasUserSpecifiedPrice())
				item.Price = item.DefaultPrice;
		}

		void TreeItems_Selection_Changed (object sender, EventArgs e)
		{
			object[] items = treeItems.GetSelectedObjects ();

			buttonDelete.Sensitive = items.Length > 0 && ((items [0] as OrderItem).AdditionalAgreement == null || (items [0] as OrderItem).Nomenclature.Category == NomenclatureCategory.water);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Order> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем заказ...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		#region Toggle buttons

		protected void OnToggleInformationToggled (object sender, EventArgs e)
		{
			if (toggleInformation.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnToggleGoodsToggled (object sender, EventArgs e)
		{
			if (toggleGoods.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnToggleEquipmentToggled (object sender, EventArgs e)
		{
			if (toggleEquipment.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnToggleServiceToggled (object sender, EventArgs e)
		{
			if (toggleService.Active)
				notebook1.CurrentPage = 3;
		}

		protected void OnToggleDocumentsToggled (object sender, EventArgs e)
		{
			if (toggleDocuments.Active)
				notebook1.CurrentPage = 4;
		}

		#endregion

		protected void OnReferenceClientChanged (object sender, EventArgs e)
		{
			if (UoWGeneric.Root.Client != null) {
				referenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM (UoW, Entity.Client);
				referenceDeliveryPoint.Sensitive = UoWGeneric.Root.OrderStatus == OrderStatus.NewOrder;
			} else {
				referenceDeliveryPoint.Sensitive = false;
			}
			UpdateProxyInfo ();
		}

		private void IsEditable (bool val = false)
		{
			referenceDeliverySchedule.Sensitive = referenceDeliveryPoint.Sensitive = 
				referenceClient.Sensitive = val;
			enumAddRentButton.Sensitive = enumSignatureType.Sensitive = enumStatus.Sensitive = 
				enumPaymentType.Sensitive = val;
			buttonAddDoneService.Sensitive = buttonAddServiceClaim.Sensitive = 
				buttonAddForSale.Sensitive = buttonFillComment.Sensitive = val;
			spinBottlesReturn.Sensitive = spinSumDifference.Sensitive = val;
			checkDelivered.Sensitive = checkSelfDelivery.Sensitive = val;
			textComments.Sensitive = val;
			pickerDeliveryDate.Sensitive = val;
			dataSumDifferenceReason.Sensitive = val;
			treeItems.Sensitive = val;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			Entity.RemoveItem (treeItems.GetSelectedObject () as OrderItem);
		}

		protected void OnButtonAddForSaleClicked (object sender, EventArgs e)
		{
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation (new ViewModel.NomenclatureForSaleVM (UoWGeneric));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Номенклатура на продажу";
			SelectDialog.ObjectSelected += NomenclatureForSaleSelected;
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		void NomenclatureForSaleSelected(object sender, ReferenceRepresentationSelectedEventArgs e){					
			NomenclatureSelected (this, new OrmReferenceObjectSectedEventArgs (UoWGeneric.Session.Get<Nomenclature> (e.ObjectId)));
		}

		void NomenclatureSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclature = e.Subject as Nomenclature;
			if (nomenclature.Category == NomenclatureCategory.additional) {
				UoWGeneric.Root.AddAdditionalNomenclatureForSale (nomenclature);
			} else if (nomenclature.Category == NomenclatureCategory.equipment) {
				UoWGeneric.Root.AddEquipmentNomenclatureForSale (nomenclature, UoWGeneric);
			} else if (nomenclature.Category == NomenclatureCategory.water) {
				CounterpartyContract contract = CounterpartyContractRepository.
					GetCounterpartyContractByPaymentType (UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType);
				if (contract == null) {
					var result = AskCreateContract ();
					switch (result) {
					case (int)ResponseType.Yes:
						RunContractAndWaterAgreementDialog ();
						break;
					case (int)ResponseType.Accept:
						CreateDefaultContractWithAgreement ();
						break;
					default:
						break;
					}
					return;
				}
				UoWGeneric.Session.Refresh (contract);
				WaterSalesAgreement wsa = contract.GetWaterSalesAgreement (UoWGeneric.Root.DeliveryPoint);
				if (wsa == null) {	
					//Если нет доп. соглашения продажи воды.
					if (MessageDialogWorks.RunQuestionDialog ("Отсутствует доп. соглашение с клиентом для продажи воды. Создать?")) {
						RunAdditionalAgreementWaterDialog ();
					} else
						return;
				} else {
					UoWGeneric.Root.AddWaterForSale (nomenclature, wsa);
					UoWGeneric.Root.RecalcBottlesDeposits (UoWGeneric);
				}
			}
			UpdateSum ();
		}

		private void AddRentAgreement (OrderAgreementType type)
		{
			ITdiDialog dlg;

			if (UoWGeneric.Root.Client == null || UoWGeneric.Root.DeliveryPoint == null) {
				MessageDialogWorks.RunWarningDialog ("Для добавления оборудования должна быть выбрана точка доставки.");
				return;
			}
			CounterpartyContract contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType (UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType);
			if (contract == null) {
				RunContractCreateDialog ();
				return;
			} 
			switch (type) {
			case OrderAgreementType.NonfreeRent:
				dlg = new AdditionalAgreementNonFreeRent (contract, UoWGeneric.Root.DeliveryPoint, UoWGeneric.Root.DeliveryDate);
				break;
			case OrderAgreementType.DailyRent:
				dlg = new AdditionalAgreementDailyRent (contract, UoWGeneric.Root.DeliveryPoint, UoWGeneric.Root.DeliveryDate);
				break;
			default: 
				dlg = new AdditionalAgreementFreeRent (contract, UoWGeneric.Root.DeliveryPoint, UoWGeneric.Root.DeliveryDate);
				break;
			}
			(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
			TabParent.AddSlaveTab (this, dlg);
		}

		void AgreementSaved (object sender, AgreementSavedEventArgs e)
		{
			UoWGeneric.Root.ObservableOrderDocuments.Add (new OrderAgreement { 
				Order = UoWGeneric.Root,
				AdditionalAgreement = e.Agreement
			});
			UoWGeneric.Root.FillItemsFromAgreement (e.Agreement);
			UpdateSum ();
			CounterpartyContractRepository.GetCounterpartyContractByPaymentType (UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType).AdditionalAgreements.Add (e.Agreement);
		}

		void UpdateSum ()
		{
			Decimal sum = UoWGeneric.Root.TotalSum;
			labelSum.Text = CurrencyWorks.GetShortCurrencyString (sum);
			UoWGeneric.Root.SumToReceive = sum + (Decimal)spinSumDifference.Value;
			labelSumTotal.Text = CurrencyWorks.GetShortCurrencyString (UoWGeneric.Root.SumToReceive);
		}

		protected void OnButtonViewDocumentClicked (object sender, EventArgs e)
		{
			if (treeDocuments.GetSelectedObjects ().GetLength (0) > 0) {
				ITdiDialog dlg = null;
				if (treeDocuments.GetSelectedObjects () [0] is OrderAgreement) {
					var agreement = (treeDocuments.GetSelectedObjects () [0] as OrderAgreement).AdditionalAgreement;
					dlg = OrmMain.CreateObjectDialog (agreement);
				} else if (treeDocuments.GetSelectedObjects () [0] is OrderContract) {
					var contract = (treeDocuments.GetSelectedObjects () [0] as OrderContract).Contract;
					dlg = OrmMain.CreateObjectDialog (contract);
				}

				if (dlg != null) {
					(dlg as IEditableDialog).IsEditable = false;
					TabParent.AddSlaveTab (this, dlg);
				}
			}
		}

		protected void OnButtonFillCommentClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference (typeof(CommentTemplate), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += (s, ea) => {
				if (ea.Subject != null) {
					UoWGeneric.Root.Comment = (ea.Subject as CommentTemplate).Comment;
				}
			};
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		protected void OnSpinSumDifferenceValueChanged (object sender, EventArgs e)
		{
			UpdateSum ();
			string text;
			if (spinSumDifference.Value > 0)
				text = "Сумма <b>переплаты</b>/недоплаты:";
			else if (spinSumDifference.Value < 0)
				text = "Сумма переплаты/<b>недоплаты</b>:";
			else
				text = "Сумма переплаты/недоплаты:";
			labelSumDifference.Markup = text;
		}

		void RunContractCreateDialog ()
		{
			ITdiTab dlg;
			var response = AskCreateContract ();
			if (response == (int)ResponseType.Yes) {
				dlg = new CounterpartyContractDlg (UoWGeneric.Root.Client, 
					OrganizationRepository.GetOrganizationByPaymentType (UoWGeneric, UoWGeneric.Root.PaymentType),
					UoWGeneric.Root.DeliveryDate);
				(dlg as IContractSaved).ContractSaved += OnContractSaved;
				TabParent.AddSlaveTab (this, dlg);
			} else if (response == (int)ResponseType.Accept) {
				var contract = CreateDefaultContract ();
				AddContractDocument (contract);
			}
		}

		protected int AskCreateContract(){
			MessageDialog md = new MessageDialog (null,
				DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.YesNo,
				"Отсутствует договор с клиентом для " +
				(UoWGeneric.Root.PaymentType == PaymentType.cash ? "наличной" : "безналичной") +
				" формы оплаты. Создать?");
			md.SetPosition (WindowPosition.Center);
			md.AddButton ("Автоматически", ResponseType.Accept);
			md.ShowAll ();
			var result = md.Run ();
			md.Destroy ();
			return result;
		}

		protected void RunContractAndWaterAgreementDialog(){
			ITdiTab dlg = new CounterpartyContractDlg (UoWGeneric.Root.Client,
				              OrganizationRepository.GetOrganizationByPaymentType (UoWGeneric, UoWGeneric.Root.PaymentType),
				              UoWGeneric.Root.DeliveryDate);
			(dlg as IContractSaved).ContractSaved += OnContractSaved;
			dlg.CloseTab += (sender, e) => {
				CounterpartyContract contract =
					CounterpartyContractRepository.GetCounterpartyContractByPaymentType (
						UoWGeneric,
						UoWGeneric.Root.Client,
						UoWGeneric.Root.PaymentType);
				if(contract!=null){
					bool hasWaterAgreement = contract.GetWaterSalesAgreement (UoWGeneric.Root.DeliveryPoint)!=null;
					if(!hasWaterAgreement)
						RunAdditionalAgreementWaterDialog();
				}
			};
			TabParent.AddSlaveTab (this, dlg);
		}

		protected void OnContractSaved(object sender, ContractSavedEventArgs args){
			UoWGeneric.Root.ObservableOrderDocuments.Add (new OrderContract { 
				Order = UoWGeneric.Root,
				Contract = args.Contract
			});
		}

		protected void RunAdditionalAgreementWaterDialog(){
			ITdiDialog dlg = new AdditionalAgreementWater (CounterpartyContractRepository.GetCounterpartyContractByPaymentType (UoWGeneric, UoWGeneric.Root.Client, UoWGeneric.Root.PaymentType), UoWGeneric.Root.DeliveryDate);
			(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
			TabParent.AddSlaveTab (this, dlg);
		}

		protected void CreateDefaultContractWithAgreement(){
			var contract = CreateDefaultContract ();
			AddContractDocument (contract);
			AdditionalAgreement agreement = contract.GetWaterSalesAgreement (UoWGeneric.Root.DeliveryPoint);
			if(agreement==null){
				agreement = CreateDefaultWaterAgreement (contract);
				contract.AdditionalAgreements.Add (agreement);
				AddAgreementDocument (agreement);
			}
		}			

		protected CounterpartyContract CreateDefaultContract(){
			CounterpartyContract result;
			using (var uow = CounterpartyContract.Create (UoWGeneric.Root.Client)) {
				var contract = uow.Root;
				contract.Organization = OrganizationRepository
					.GetOrganizationByPaymentType (UoWGeneric, UoWGeneric.Root.PaymentType);
				contract.IsArchive = false;
				contract.IssueDate = UoWGeneric.Root.DeliveryDate;
				contract.AdditionalAgreements = new List<AdditionalAgreement> ();
				uow.Save ();
				result = uow.Root;
			}
			return result;
		}

		protected AdditionalAgreement CreateDefaultWaterAgreement(CounterpartyContract contract){
			AdditionalAgreement result;
			using (var uow = WaterSalesAgreement.Create (contract)) {
				AdditionalAgreement agreement = uow.Root;
				agreement.Contract = contract;
				agreement.AgreementNumber = WaterSalesAgreement.GetNumber (contract);
				agreement.IssueDate = UoWGeneric.Root.DeliveryDate;
				agreement.StartDate = UoWGeneric.Root.DeliveryDate;
				result = uow.Root;
				uow.Save ();
			}
			return result;
		}
			
		protected void AddContractDocument(CounterpartyContract contract){
			Order order = UoWGeneric.Root;
			var orderDocuments = UoWGeneric.Root.ObservableOrderDocuments;
			orderDocuments.Add (new OrderContract { 
				Order = order,
				Contract = contract
			});
		}

		protected void AddAgreementDocument(AdditionalAgreement agreement){
			Order order = UoWGeneric.Root;
			var orderDocuments = UoWGeneric.Root.ObservableOrderDocuments;
			orderDocuments.Add (new OrderAgreement { 
				Order = order,
				AdditionalAgreement = agreement
			});
		}
			
		protected void OnSpinBottlesReturnValueChanged (object sender, EventArgs e)
		{
			UoWGeneric.Root.RecalcBottlesDeposits (UoWGeneric);
		}

		protected void OnButtonAcceptClicked (object sender, EventArgs e)
		{
			if (UoWGeneric.Root.OrderStatus == OrderStatus.NewOrder) {
				var valid = new QSValidator<Order> (UoWGeneric.Root, 
					            new Dictionary<object, object> {
						{ "NewStatus", OrderStatus.Accepted }
					});
				if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
					return;

				if (UoWGeneric.Root.BottlesReturn == 0 && Entity.OrderItems.Any (i => i.Nomenclature.Category == NomenclatureCategory.water)) {
					if (!MessageDialogWorks.RunQuestionDialog ("Указано нулевое количество бутылей на возврат. Вы действительно хотите продолжить?"))
						return;
				}
				
				UoWGeneric.Root.OrderStatus = OrderStatus.Accepted;
				IsEditable ();
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Редактировать";
				return;
			}
			if (UoWGeneric.Root.OrderStatus == OrderStatus.Accepted) {
				UoWGeneric.Root.OrderStatus = OrderStatus.NewOrder;
				IsEditable (true);
				var icon = new Image ();
				icon.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-edit", IconSize.Menu);
				buttonAccept.Image = icon;
				buttonAccept.Label = "Подтвердить";
				return;
			}
		}

		protected void OnEnumSignatureTypeChanged (object sender, EventArgs e)
		{
			UpdateProxyInfo ();
		}

		void UpdateProxyInfo ()
		{
			labelProxyInfo.Visible = Entity.SignatureType == OrderSignatureType.ByProxy;
			if (Entity.SignatureType != OrderSignatureType.ByProxy)
				return;
			DBWorks.SQLHelper text = new DBWorks.SQLHelper ("");
			if (Entity.Client != null) {
				var proxies = Entity.Client.Proxies.Where (p => p.IsActiveProxy (Entity.DeliveryDate) && (p.DeliveryPoint == null || p.DeliveryPoint == Entity.DeliveryPoint));
				foreach (var proxy in proxies) {
					if (!String.IsNullOrWhiteSpace (text.Text))
						text.Add ("\n");
					text.Add (String.Format ("Доверенность{2} №{0} от {1:d}", proxy.Number, proxy.IssueDate, 
						proxy.DeliveryPoint == null ? "(общая)" : ""));
					text.StartNewList (": ");
					foreach (var pers in proxy.Persons) {
						text.AddAsList (pers.NameWithInitials);
					}
				}
			}
			if (String.IsNullOrWhiteSpace (text.Text))
				labelProxyInfo.Markup = "<span foreground=\"red\">Нет активной доверенности</span>";
			else
				labelProxyInfo.LabelProp = text.Text;
		}

		protected void OnReferenceDeliveryPointChanged (object sender, EventArgs e)
		{
			UpdateProxyInfo ();
		}

		protected void OnButtonPrintEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			QSReport.ReportViewDlg report = null;
			QSReport.ReportInfo reportInfo = null;
			PrintDocuments selected = (PrintDocuments)e.ItemEnum;

			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(Order), selected.GetEnumTitle ()))
				UoWGeneric.Save ();
			
			switch (selected) {
			case PrintDocuments.Bill:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Счет №{0} от {1:d}", Entity.Id, Entity.DeliveryDate),
					Identifier = "Bill",
					Parameters = new Dictionary<string, object> {
						{ "order_id",  Entity.Id },
						{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization]) },
						{ "hide_signature", false }
					}
				};
				break;
			case PrintDocuments.BillWithoutSignature:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Счет №{0} от {1:d} (без печати и подписи)", Entity.Id, Entity.DeliveryDate),
					Identifier = "Bill",
					Parameters = new Dictionary<string, object> {
						{ "order_id",  Entity.Id },
						{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization])  },
						{ "hide_signature", true }
					}
				};
				break;
			case PrintDocuments.DoneWorkReport:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Акт выполненных работ"),
					Identifier = "DoneWorkReport",
					Parameters = new Dictionary<string,object> {
						{ "order_id",Entity.Id },
						{"service_claim_id",27} //TODO передавать id заявки(возможно печать организовать иначе)
						//TODO телефон указывать точки доставки(если есть и контактное лицо + телефон клиента)
					}
				};
				break;
			case PrintDocuments.EquipmentTransfer:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Акт приема-передачи оборудования"),
					Identifier = "EquipmentTransfer",
					Parameters = new Dictionary<string,object> {
						{ "order_id",Entity.Id },
						{"service_claim_id",27} //TODO передавать id заявки(возможно печать организовать иначе)
						//TODO телефон указывать точки доставки(если есть и контактное лицо + телефон клиента)
					}
				};
				break;
			case PrintDocuments.Invoice:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Накладная №{0} от {1:d}", Entity.Id, Entity.DeliveryDate),
					Identifier = "Invoice",
					Parameters = new Dictionary<string, object> {
						{ "order_id",  Entity.Id }
					}
				};
				break;
			case PrintDocuments.InvoiceBarter:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Накладная №{0} от {1:d} (безденежно)", Entity.Id, Entity.DeliveryDate),
					Identifier = "InvoiceBarter",
					Parameters = new Dictionary<string, object> {
						{ "order_id",  Entity.Id }
					}
				};
				break;
			case PrintDocuments.UPD:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("УПД {0} от {1:d}", Entity.Id, Entity.DeliveryDate),
					Identifier = "UPD",
					Parameters = new Dictionary<string, object> {
						{ "order_id", Entity.Id }
					}
				};
				break;
			case PrintDocuments.CoolerWarranty:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Гарантийный талон на кулера №{0}", Entity.Id),
					Identifier = "CoolerWarranty",
					Parameters = new Dictionary<string, object> {
						{ "order_id", Entity.Id },
						{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization])}
					}
				};
				break;
			case PrintDocuments.PumpWarranty:
				reportInfo = new QSReport.ReportInfo {
					Title = String.Format ("Гарантийный талон на помпы №{0}", Entity.Id),
					Identifier = "PumpWarranty",
					Parameters = new Dictionary<string, object> {
						{ "order_id", Entity.Id },
						{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization])}
					}
				};
				break;
			default:
				throw new InvalidOperationException (String.Format ("Тип документа еще не поддерживается: {0}", selected));
			}

			if (reportInfo != null) {
				report = new QSReport.ReportViewDlg (reportInfo);
				TabParent.AddTab (report, this, false);
			}
		}

		protected void OnButtonPrintSelectedClicked(object c, EventArgs args)
		{
			var selectedPrintableDocuments = treeDocuments.GetSelectedObjects().Cast<OrderDocument>()
				.Where(doc => doc.PrintType != PrinterType.None).ToList();
			if (selectedPrintableDocuments.Count > 0)
			{
				string whatToPrint = selectedPrintableDocuments.Count > 1 
					? "документов" 
					: "документа \""+selectedPrintableDocuments.First().Type.GetEnumTitle()+"\"";
				if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Order), whatToPrint))
					UoWGeneric.Save();
			
				selectedPrintableDocuments.ForEach(
					doc => TabParent.AddTab(DocumentPrinter.PreviewTab(doc), this, false)
				);
			}
		}

		protected void OnTreeServiceClaimRowActivated (object o, RowActivatedArgs args)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ServiceClaimDlg dlg = new ServiceClaimDlg ((treeServiceClaim.GetSelectedObjects () [0] as ServiceClaim).Id);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonAddServiceClaimClicked (object sender, EventArgs e)
		{
			if (!SaveOrderBeforeContinue ())
				return;
			var dlg = new ServiceClaimDlg (UoWGeneric.Root);
			TabParent.AddSlaveTab (this, dlg);
		}

		protected void OnButtonAddDoneServiceClicked (object sender, EventArgs e)
		{
			if (!SaveOrderBeforeContinue ())
				return;
			OrmReference SelectDialog = new OrmReference (typeof(ServiceClaim), UoWGeneric, 
				                            ServiceClaimRepository.GetDoneClaimsForClient (UoWGeneric.Root)
				.GetExecutableQueryOver (UoWGeneric.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += DoneServiceSelected;

			TabParent.AddSlaveTab (this, SelectDialog);
		}

		void DoneServiceSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			ServiceClaim selected = (e.Subject as ServiceClaim);
			var contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType (
				               UoWGeneric, 
				               UoWGeneric.Root.Client, 
				               UoWGeneric.Root.PaymentType);
			if (!contract.RepairAgreementExists ()) {
				RunAgreementCreateDialog (contract);
				return;
			}
			selected.FinalOrder = UoWGeneric.Root;
			UoWGeneric.Root.ObservableFinalOrderService.Add (selected);
			//TODO Add service nomenclature with price.
		}

		bool SaveOrderBeforeContinue ()
		{
			if (UoWGeneric.IsNew) {
				if (CommonDialogs.SaveBeforeCreateSlaveEntity (EntityObject.GetType (), typeof(ServiceClaim))) {
					if (!Save ())
						return false;
				} else
					return false;
			}
			return true;
		}

		void RunAgreementCreateDialog (CounterpartyContract contract)
		{
			ITdiTab dlg;
			string question = "Отсутствует доп. соглашение сервиса с клиентом в договоре для " +
			                  (UoWGeneric.Root.PaymentType == PaymentType.cash ? "наличной" : "безналичной") +
			                  " формы оплаты. Создать?";
			if (MessageDialogWorks.RunQuestionDialog (question)) {
				dlg = new AdditionalAgreementRepair (contract);
				(dlg as IAgreementSaved).AgreementSaved += (sender, e) => UoWGeneric.Root.ObservableOrderDocuments.Add (
					new OrderAgreement {
						Order = UoWGeneric.Root,
						AdditionalAgreement = e.Agreement
					});
				TabParent.AddSlaveTab (this, dlg);
			}
		}

		protected void OnEnumPaymentTypeChanged (object sender, EventArgs e)
		{
			enumSignatureType.Visible = checkDelivered.Visible = labelSignatureType.Visible = 
				(Entity.PaymentType == PaymentType.cashless);
		}

		protected void OnPickerDeliveryDateDateChanged (object sender, EventArgs e)
		{
			UpdateProxyInfo ();
		}
	}

	public enum PrintDocuments
	{
		[Display (Name = "Счет")]
		Bill,
		[Display (Name = "Счет (Без печати и подписи)")]
		BillWithoutSignature,
		[Display (Name = "Акт выполненных работ")]
		DoneWorkReport,
		[Display (Name = "Акт приема-передачи оборудования")]
		EquipmentTransfer,
		[Display (Name = "Накладная (нал.)")]
		Invoice,
		[Display (Name = "Накладная (безденежно)")]
		InvoiceBarter,
		[Display (Name = "УПД")]
		UPD,
		[Display(Name="Гарантийный талон для кулеров")]
		CoolerWarranty,
		[Display(Name="Гарантийный талон для помп")]
		PumpWarranty
	}
}