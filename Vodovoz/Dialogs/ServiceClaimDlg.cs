using System;
using QSOrmProject;
using Vodovoz.Domain.Service;
using NLog;
using Vodovoz.Domain.Orders;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Repository;
using QSTDI;
using QSProjectsLib;
using Gtk.DataBindings;
using Gtk;
using System.Data.Bindings;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ServiceClaimDlg : OrmGtkDialogBase<ServiceClaim>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { get { return isEditable; } set { isEditable = value; } }

		public ServiceClaimDlg (Order order)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ServiceClaim>(new ServiceClaim (order));
			ConfigureDlg ();
		}

		public ServiceClaimDlg (ServiceClaim sub) : this (sub.Id)
		{
		}

		public ServiceClaimDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<ServiceClaim> (id);
			ConfigureDlg ();
		}

		public ServiceClaimDlg (ServiceClaimType type)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<ServiceClaim>(new ServiceClaim (type));
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			enumStatus.Sensitive = enumType.Sensitive = false;
			enumStatusEditable.Sensitive = true;
			notebook1.ShowTabs = false;
			notebook1.CurrentPage = 0;
			subjectAdaptor.Target = UoWGeneric.Root;

			datatable1.DataSource = subjectAdaptor;
			datatable2.DataSource = subjectAdaptor;
			datatable3.DataSource = subjectAdaptor;
			enumPaymentType.DataSource = subjectAdaptor;
			enumStatus.DataSource = subjectAdaptor;
			enumType.DataSource = subjectAdaptor;
			labelTotalPrice.DataSource = subjectAdaptor;

			enumcomboWithSerial.ItemsEnum = typeof(ServiceClaimComboEnum);

			referenceCounterparty.SubjectType = typeof(Counterparty);
			referenceEngineer.SubjectType = typeof(Employee);
			referenceEquipment.SubjectType = typeof(Equipment);
			referenceNomenclature.SubjectType = typeof(Nomenclature);

			referenceDeliveryPoint.Sensitive = (UoWGeneric.Root.Counterparty != null);
			referenceEquipment.Sensitive = (UoWGeneric.Root.Nomenclature != null);

			referenceNomenclature.ItemsQuery = NomenclatureRepository.NomenclatureOfItemsForService ();

			treePartsAndServices.ItemsDataSource = UoWGeneric.Root.ObservableServiceClaimItems;
			treeHistory.ItemsDataSource = UoWGeneric.Root.ObservableServiceClaimHistory;

			treePartsAndServices.ColumnMappingConfig = FluentMappingConfig <ServiceClaimItem>.Create ()
				.AddColumn ("Номенклатура").SetDataProperty (node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn ("Кол-во").AddNumericRenderer (node => node.Count)
				.Adjustment (new Adjustment (0, 0, 1000000, 1, 100, 0))
				.AddSetter ((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddSetter ((c, i) => c.Editable = isEditable)
				.WidthChars (10)
				.AddColumn ("Цена").AddNumericRenderer (node => node.Price).Digits (2)
				.AddTextRenderer (node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn ("Сумма").AddNumericRenderer (node => node.Total).Digits (2)
				.AddTextRenderer (node => CurrencyWorks.CurrencyShortName, false)
				.Finish ();

			treeHistory.ColumnMappingConfig = FluentMappingConfig <ServiceClaimHistory>.Create ()
				.AddColumn ("Дата").SetDataProperty (node => node.Date.ToShortDateString ())
				.AddColumn ("Время").SetDataProperty (node => node.Date.ToString ("HH:mm"))
				.AddColumn ("Статус").SetDataProperty (node => node.Status.GetEnumTitle ())
				.AddColumn ("Сотрудник").SetDataProperty (node => node.Employee == null ? " - " : node.Employee.FullName)
				.AddColumn ("Комментарий").SetDataProperty (node => node.Comment)
				.Finish ();

			UoWGeneric.Root.ObservableServiceClaimItems.ElementChanged += (aList, aIdx) => FixPrice (aIdx [0]);
			configureAvailableNextStatus ();

			if (UoWGeneric.Root.ServiceClaimType == ServiceClaimType.JustService) {
				referenceDeliveryPoint.Visible = false;
				labelDeliveryPoint.Visible = false;
			}
			datePickUpDate.IsEditable = Entity.ServiceClaimType != ServiceClaimType.RegularService;
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save ()
		{
			var valid = new QSValidator<ServiceClaim> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Window)this.Toplevel))
				return false;

			CounterpartyContract contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType 
				(UoWGeneric, UoWGeneric.Root.Counterparty, UoWGeneric.Root.Payment);

			if (contract == null) {
				RunContractCreateDialog ();
				return false;
			}

			UoWGeneric.Session.Refresh (contract);
			if (!contract.RepairAgreementExists ()) {
				RunAgreementCreateDialog (contract);
				return false;
			}

			if (UoWGeneric.Root.InitialOrder != null)
				UoWGeneric.Root.InitialOrder.AddServiceClaimAsInitial (UoWGeneric.Root);

			if (UoWGeneric.Root.FinalOrder != null) {
				UoWGeneric.Root.FinalOrder.AddServiceClaimAsFinal (UoWGeneric.Root);
			}

			if (UoWGeneric.IsNew)
				UoWGeneric.Root.AddHistoryRecord (UoWGeneric.Root.Status, 
					String.IsNullOrWhiteSpace (textComment.Buffer.Text) ? "Заявка зарегистрирована" : textComment.Buffer.Text);

			logger.Info ("Сохраняем заявку на обслуживание...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		#endregion

		protected void OnReferenceNomenclatureChanged (object sender, EventArgs e)
		{
			FixNomenclatureAndEquipmentSensitivity ();

			if (UoWGeneric.Root.Equipment != null &&
			    UoWGeneric.Root.Equipment.Nomenclature.Id != UoWGeneric.Root.Nomenclature.Id) {
			
				UoWGeneric.Root.Equipment = null;
			}
		}

		protected void OnReferenceCounterpartyChanged (object sender, EventArgs e)
		{
			referenceDeliveryPoint.Sensitive = (UoWGeneric.Root.Counterparty != null);
			FixNomenclatureAndEquipmentSensitivity ();
			if (UoWGeneric.Root.DeliveryPoint != null &&
			    UoWGeneric.Root.DeliveryPoint.Counterparty.Id != UoWGeneric.Root.Counterparty.Id) {

				UoWGeneric.Root.DeliveryPoint = null;
			}
			referenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM (UoW, Entity.Counterparty);
		}

		void RunContractCreateDialog ()
		{
			ITdiTab dlg;
			string question = "Отсутствует договор с клиентом для " +
			                  (UoWGeneric.Root.Payment == PaymentType.cash ? "наличной" : "безналичной") +
			                  " формы оплаты. Создать?";
			if (MessageDialogWorks.RunQuestionDialog (question)) {
				dlg = new CounterpartyContractDlg (UoWGeneric.Root.Counterparty, 
					(UoWGeneric.Root.Payment == PaymentType.cash ?
							OrganizationRepository.GetCashOrganization (UoWGeneric) :
							OrganizationRepository.GetCashlessOrganization (UoWGeneric)));
				(dlg as IContractSaved).ContractSaved += (sender, e) => {
					if (UoWGeneric.Root.InitialOrder != null)
						UoWGeneric.Root.InitialOrder.ObservableOrderDocuments.Add (new OrderContract { 
							Order = UoWGeneric.Root.InitialOrder,
							Contract = e.Contract
						});
				};
				TabParent.AddSlaveTab (this, dlg);
			}
		}

		void RunAgreementCreateDialog (CounterpartyContract contract)
		{
			ITdiTab dlg;
			string question = "Отсутствует доп. соглашение сервиса с клиентом в договоре для " +
			                  (UoWGeneric.Root.Payment == PaymentType.cash ? "наличной" : "безналичной") +
			                  " формы оплаты. Создать?";
			if (MessageDialogWorks.RunQuestionDialog (question)) {
				dlg = new AdditionalAgreementRepair (contract);
				(dlg as IAgreementSaved).AgreementSaved += (sender, e) => {
					if (UoWGeneric.Root.InitialOrder != null)
						UoWGeneric.Root.InitialOrder.ObservableOrderDocuments.Add (new OrderAgreement { 
							Order = UoWGeneric.Root.InitialOrder,
							AdditionalAgreement = e.Agreement
						});
				};
				TabParent.AddSlaveTab (this, dlg);
			}
		}

		protected void OnButtonAddServiceClicked (object sender, EventArgs e)
		{
			OpenDialog (NomenclatureRepository.NomenclatureOfServices ());
		}

		protected void OnButtonAddPartClicked (object sender, EventArgs e)
		{
			OpenDialog (NomenclatureRepository.NomenclatureOfPartsForService ());
		}

		void OpenDialog (NHibernate.Criterion.QueryOver<Nomenclature> nomenclatureType)
		{
			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature), UoWGeneric, 
				                            nomenclatureType.GetExecutableQueryOver (UoWGeneric.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += NomenclatureSelected;
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		void NomenclatureSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			UoWGeneric.Root.ObservableServiceClaimItems.Add (new ServiceClaimItem { 
				ServiceClaim = UoWGeneric.Root,
				Nomenclature = e.Subject as Nomenclature,
				Price = (e.Subject as Nomenclature).GetPrice (1),
				Count = 1
			});
		}

		void FixPrice (int id)
		{
			ServiceClaimItem item = UoWGeneric.Root.ObservableServiceClaimItems [id];
			item.Price = item.Nomenclature.GetPrice ((int)item.Count);
		}

		protected void OnToggleInfoToggled (object sender, EventArgs e)
		{
			if (toggleInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnToggleServicesAndWorksToggled (object sender, EventArgs e)
		{
			if (toggleServicesAndWorks.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnToggleHistoryToggled (object sender, EventArgs e)
		{
			if (toggleHistory.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if (!String.IsNullOrWhiteSpace (textComment.Buffer.Text) || MessageDialogWorks.RunQuestionDialog ("Вы не заполнили комментарий. Продолжить?")) {

				var active = enumStatusEditable.Active;
				var status = (active == -1) ? UoWGeneric.Root.Status : (ServiceClaimStatus)active;
				UoWGeneric.Root.AddHistoryRecord (status, textComment.Buffer.Text);
			}
		}

		void configureAvailableNextStatus ()
		{
			var enumList = UoWGeneric.Root.GetAvailableNextStatusList ();
			enumStatusEditable.SetEnumItems<ServiceClaimStatus> (enumList);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
			
		protected void OnReferenceEquipmentChanged (object sender, EventArgs e)
		{
			Equipment selectedEquipment = referenceEquipment.Subject as Equipment;
			referenceNomenclature.Subject = selectedEquipment != null ? selectedEquipment.Nomenclature : null;
		}
			
		protected void OnEnumcomboWithSerialEnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			FixNomenclatureAndEquipmentSensitivity ();
		}
			
		protected void OnReferenceDeliveryPointChanged (object sender, EventArgs e)
		{
			FixNomenclatureAndEquipmentSensitivity ();
			referenceEquipment.ItemsQuery = EquipmentRepository.GetEquipmentAtDeliveryPoint (UoWGeneric.Root.Counterparty, UoWGeneric.Root.DeliveryPoint);
		}

		protected void FixNomenclatureAndEquipmentSensitivity(){
			bool withSerial = ((ServiceClaimComboEnum)enumcomboWithSerial.SelectedItem) == ServiceClaimComboEnum.WithSerial;		 
			referenceEquipment.Sensitive = withSerial && UoWGeneric.Root.Counterparty!=null && UoWGeneric.Root.DeliveryPoint !=null;
			referenceNomenclature.Sensitive = !withSerial && UoWGeneric.Root.Counterparty!=null;
		}
	}

	enum ServiceClaimComboEnum
	{
		[Display (Name="с серийным номером")]
		WithSerial,
		[Display (Name="без серийного номера")]
		WithoutSerial

	}
}

