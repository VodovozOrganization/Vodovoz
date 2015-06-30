using System;
using QSOrmProject;
using Vodovoz.Domain;
using NLog;
using QSValidation;
using QSTDI;
using NHibernate;
using Vodovoz.Repository;

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
			throw new NotImplementedException ();
		}

		protected void OnButtonAddForSaleClicked (object sender, EventArgs e)
		{			
		}

		protected void OnEnumAddRentButtonEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			AddRentAgreement ((PaidRentAgreementType)e.ItemEnum);
		}

		private void AddRentAgreement (PaidRentAgreementType type)
		{
			if (referenceClient.Data == null || referenceDeliveryPoint == null) {
				return;
				//TODO FIXME Вывод сообщения, что не все заполнено.
			}
			CounterpartyContract contract = CounterpartyContractRepository.GetCounterpartyContract (UoWGeneric);
			if (contract == null) {
				return;
				//TODO FIXME Вывод сообщения об ошибке.
			}
			IUnitOfWork uow;
			switch (type) {
			case PaidRentAgreementType.NonfreeRent:
				uow = NonfreeRentAgreement.Create (contract);
				break;
			default:
				uow = DailyRentAgreement.Create (contract);
				break;
			}
			uow.Commit ();

		}
	}
}

