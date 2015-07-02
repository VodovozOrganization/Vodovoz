using System;
using QSOrmProject;
using Vodovoz.Domain;
using NLog;
using QSValidation;
using QSTDI;
using Vodovoz.Repository;
using QSProjectsLib;
using Gtk;
using System.Collections.Generic;

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
			if (UoWGeneric.Root.OrderStatus != OrderStatus.NewOrder)
				BlockAll ();
			if (UoWGeneric.Root.PreviousOrder != null) {
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
//TODO Make it clickable.
			} else
				labelPreviousOrder.Visible = false;
			subjectAdaptor.Target = UoWGeneric.Root;
			treeDocuments.ItemsDataSource = UoWGeneric.Root.ObservableOrderDocuments;
			treeItems.ItemsDataSource = UoWGeneric.Root.ObservableOrderItems;
			treeEquipment.ItemsDataSource = UoWGeneric.Root.ObservableOrderEquipments;
			datatable1.DataSource = subjectAdaptor;
			datatable2.DataSource = subjectAdaptor;
			enumStatus.DataSource = subjectAdaptor;
			enumStatus.Sensitive = false;
			enumSignatureType.DataSource = subjectAdaptor;
			enumPaymentType.DataSource = subjectAdaptor;
			notebook1.Page = 0;
			notebook1.ShowTabs = false;
			referenceClient.SubjectType = typeof(Counterparty);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);
			referenceDeliveryPoint.Sensitive = UoWGeneric.Root.Client != null;
			buttonDelete.Sensitive = false;
			treeDocuments.Selection.Changed += TreeDocuments_Selection_Changed;
			buttonViewDocument.Sensitive = false;
			UpdateSum ();
		}

		void TreeDocuments_Selection_Changed (object sender, EventArgs e)
		{
			buttonViewDocument.Sensitive = treeDocuments.Selection.CountSelectedRows () > 0;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Order> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем заказ...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

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

		protected void OnCheckSelfDeliveryToggled (object sender, EventArgs e)
		{
			pickerDeliveryDate.Sensitive = referenceDeliverySchedule.Sensitive = !checkSelfDelivery.Active;
			labelDeliveryDate.Sensitive = labelDeliveryDate.Sensitive = !checkSelfDelivery.Active;
		}

		protected void OnReferenceClientChanged (object sender, EventArgs e)
		{
			if (UoWGeneric.Root.Client != null) {
				referenceDeliveryPoint.ItemsCriteria = 
					DeliveryPointRepository.DeliveryPointsForCounterpartyQuery (UoWGeneric.Root.Client)
						.GetExecutableQueryOver (UoWGeneric.Session).RootCriteria;
				referenceDeliveryPoint.Sensitive = true;
			} else {
				referenceDeliveryPoint.Sensitive = false;
			}
		}

		private void BlockAll ()
		{
			referenceDeliverySchedule.Sensitive = referenceDeliveryPoint.Sensitive = 
				referenceClient.Sensitive = spinBottlesReturn.Sensitive = 
					spinSumToReceive.Sensitive = textComments.Sensitive = 
						checkDelivered.Sensitive = checkSelfDelivery.Sensitive = 
							enumAddRentButton.Sensitive = buttonAddForSale.Sensitive = 
								enumSignatureType.Sensitive = enumStatus.Sensitive = false;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			//TODO FIXME
		}

		protected void OnButtonAddForSaleClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature), UoWGeneric.Session, 
				                            NomenclatureRepository.NomenclatureForSaleQuery ()
				.GetExecutableQueryOver (UoWGeneric.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += NomenclatureSelected;
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		void NomenclatureSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclature = e.Subject as Nomenclature;
			if (nomenclature.Category == NomenclatureCategory.additional) {
				UoWGeneric.Root.ObservableOrderItems.Add (new OrderItem {
					AdditionalAgreement = null,
					Count = 1,
					Equipment = null,
					Nomenclature = nomenclature,
					Price = nomenclature.NomenclaturePrice [0].Price //FIXME
				});
			} else if (nomenclature.Category == NomenclatureCategory.equipment) {
				Equipment eq = EquipmentRepository.GetEquipmentForSaleByNomenclature (UoWGeneric, nomenclature);
				int ItemId;
				ItemId = UoWGeneric.Root.ObservableOrderItems.AddWithReturn (new OrderItem {
					AdditionalAgreement = null,
					Count = 1,
					Equipment = eq,
					Nomenclature = nomenclature,
					Price = nomenclature.NomenclaturePrice [0].Price //FIXME
				});
				UoWGeneric.Root.ObservableOrderEquipments.Add (new OrderEquipment {
					Direction = Vodovoz.Domain.Direction.Deliver,
					Equipment = eq,
					OrderItem = UoWGeneric.Root.ObservableOrderItems [ItemId],
					Reason = Reason.Rent	//TODO FIXME Добавить причину - продажа.
				});
			}
		}

		protected void OnEnumAddRentButtonEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			AddRentAgreement ((OrderAgreementType)e.ItemEnum);
		}

		private void AddRentAgreement (OrderAgreementType type)
		{
			ITdiDialog dlg;

			if (UoWGeneric.Root.Client == null || UoWGeneric.Root.DeliveryPoint == null) {
				MessageDialog md = new MessageDialog (null,
					                   DialogFlags.Modal,
					                   MessageType.Warning,
					                   ButtonsType.Ok,
					                   "Для добавления оборудования должны быть заполнены следующие поля:" +
					                   "\"Клиент\" и \"Точка доставки\".");
				md.SetPosition (WindowPosition.Center);
				md.ShowAll ();
				md.Run ();
				md.Destroy ();
				return;
			}
			CounterpartyContract contract = CounterpartyContractRepository.GetCounterpartyContract (UoWGeneric);
			if (contract == null) {
				MessageDialog md = new MessageDialog (null,
					                   DialogFlags.Modal,
					                   MessageType.Question,
					                   ButtonsType.YesNo,
					                   "Отсутствует договор с клиентом для выбранного типа оплаты. " +
					                   "Создать?");
				md.SetPosition (WindowPosition.Center);
				md.ShowAll ();
				bool result = md.Run () == (int)ResponseType.Yes;
				md.Destroy ();
				if (result) {
					dlg = new CounterpartyContractDlg (UoWGeneric.Root.Client, 
						(UoWGeneric.Root.PaymentType == Payment.cash ?
						OrganizationRepository.GetCashOrganization (UoWGeneric) :
						OrganizationRepository.GetCashlessOrganization (UoWGeneric)));
					(dlg as IContractSaved).ContractSaved += ContractSaved;
				} else {
					return;
				}
			} else {
				switch (type) {
				case OrderAgreementType.NonfreeRent:
					dlg = new AdditionalAgreementNonFreeRent (contract, UoWGeneric.Root.DeliveryPoint);
					break;
				case OrderAgreementType.DailyRent:
					dlg = new AdditionalAgreementDailyRent (contract, UoWGeneric.Root.DeliveryPoint);
					break;
				default: 
					dlg = new AdditionalAgreementFreeRent (contract, UoWGeneric.Root.DeliveryPoint);
					break;
				}
				(dlg as IAgreementSaved).AgreementSaved += AgreementSaved;
			}
			TabParent.AddSlaveTab (this, dlg);
		}

		void ContractSaved (object sender, ContractSavedEventArgs e)
		{
			UoWGeneric.Root.ObservableOrderDocuments.Add (new OrderContract { 
				Order = UoWGeneric.Root,
				Contract = e.Contract
			});
		}

		void AgreementSaved (object sender, AgreementSavedEventArgs e)
		{
			UoWGeneric.Root.ObservableOrderDocuments.Add (new OrderAgreement { 
				Order = UoWGeneric.Root,
				AdditionalAgreement = e.Agreement
			});
			RefreshItemsAndEquipment (e.Agreement);
		}

		void RefreshItemsAndEquipment (AdditionalAgreement a)
		{
			if (a.Type == AgreementType.DailyRent || a.Type == AgreementType.NonfreeRent) {

				IList<PaidRentEquipment> EquipmentList;
				bool IsDaily = false;

				if (a.Type == AgreementType.DailyRent) {
					EquipmentList = (a as DailyRentAgreement).Equipment;
					IsDaily = true;
				} else
					EquipmentList = (a as NonfreeRentAgreement).Equipment;

				foreach (PaidRentEquipment equipment in EquipmentList) {
					int ItemId;
					//Добавляем номенклатуру залога
					UoWGeneric.Root.ObservableOrderItems.Add (
						new OrderItem {
							AdditionalAgreement = a,
							Count = 1,
							Equipment = null,
							Nomenclature = equipment.PaidRentPackage.DepositService,
							Price = equipment.Deposit
						}
					);
					//Добавляем услугу аренды
					ItemId = UoWGeneric.Root.ObservableOrderItems.AddWithReturn (
						new OrderItem {
							AdditionalAgreement = a,
							Count = 1,
							Equipment = null,
							Nomenclature = IsDaily ? equipment.PaidRentPackage.RentServiceDaily : equipment.PaidRentPackage.RentServiceMonthly,
							Price = equipment.Price * (IsDaily ? (a as DailyRentAgreement).RentDays : 1)
						}
					);
					//Добавляем оборудование
					UoWGeneric.Root.ObservableOrderEquipments.Add (
						new OrderEquipment { 
							Direction = Vodovoz.Domain.Direction.Deliver,
							Equipment = equipment.Equipment,
							Reason = Reason.Rent,
							OrderItem = UoWGeneric.Root.ObservableOrderItems [ItemId]
						}
					);
					UoWGeneric.Root.SumToReceive += equipment.Deposit + equipment.Price * (IsDaily ? (a as DailyRentAgreement).RentDays : 1);
				}
			} else if (a.Type == AgreementType.FreeRent) {
				FreeRentAgreement agreement = a as FreeRentAgreement;
				foreach (FreeRentEquipment equipment in agreement.Equipment) {
					int ItemId;
					//Добавляем номенклатуру залога.
					ItemId = UoWGeneric.Root.ObservableOrderItems.AddWithReturn (
						new OrderItem {
							AdditionalAgreement = agreement,
							Count = 1,
							Equipment = null,
							Nomenclature = equipment.FreeRentPackage.DepositService,
							Price = equipment.Deposit
						}
					);
					//Добавляем оборудование.
					UoWGeneric.Root.ObservableOrderEquipments.Add (
						new OrderEquipment { 
							Direction = Vodovoz.Domain.Direction.Deliver,
							Equipment = equipment.Equipment,
							Reason = Reason.Rent,
							OrderItem = UoWGeneric.Root.ObservableOrderItems [ItemId]
						}
					);
				}
			}
			UpdateSum ();
		}

		void UpdateSum ()
		{
			Decimal sum = 0;
			foreach (OrderItem item in UoWGeneric.Root.ObservableOrderItems) {
				sum += item.Price;
			}
			labelSum.Text = CurrencyWorks.GetShortCurrencyString (sum);
			UoWGeneric.Root.SumToReceive = sum;
		}

		protected void OnTreeDocumentsRowActivated (object o, RowActivatedArgs args)
		{
			buttonViewDocument.Click ();
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
	}
}

