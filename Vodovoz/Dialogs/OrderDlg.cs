using System;
using QSOrmProject;
using Vodovoz.Domain;
using NLog;
using QSValidation;
using QSTDI;
using NHibernate;

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
				blockAll ();
			if (UoWGeneric.Root.PreviousOrder != null) {
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
//TODO Make it clickable.
			} else
				labelPreviousOrder.Visible = false;
			subjectAdaptor.Target = UoWGeneric.Root;
			datatable1.DataSource = subjectAdaptor;
			datatable2.DataSource = subjectAdaptor;
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

		void blockAll ()
		{
			referenceDeliverySchedule.Sensitive = referenceDeliveryPoint.Sensitive = 
				referenceClient.Sensitive = spinBottlesReturn.Sensitive = 
					spinSumToReceive.Sensitive = textComments.Sensitive = 
						checkDelivered.Sensitive = checkSelfDelivery.Sensitive = 
							buttonAddForRent.Sensitive = buttonAddForSale.Sensitive = 
								enumSignatureType.Sensitive = enumStatus.Sensitive = false;
		}

		protected void OnButtonAddForSaleClicked (object sender, EventArgs e)
		{			
			ICriteria ItemsCriteria = UoWGeneric.Session.CreateCriteria (typeof(Nomenclature))
				.Add (NHibernate.Criterion.Restrictions.In ("Category", 
				                          new[] { NomenclatureCategory.additional, NomenclatureCategory.equipment }));
			OpenNomenclatureSelectDlg (ItemsCriteria);
//TODO FIXME Добавить критерии для того оборудования что имеется в наличии.
		}

		protected void OnButtonAddForRentClicked (object sender, EventArgs e)
		{
			ICriteria ItemsCriteria = UoWGeneric.Session.CreateCriteria (typeof(Nomenclature))
				.Add (NHibernate.Criterion.Restrictions.Eq ("Category", NomenclatureCategory.equipment));
			OpenNomenclatureSelectDlg (ItemsCriteria);
//TODO FIXME Добавить критерии для того оборудования что имеется в наличии.
		}

		//TODO FIXME !IMPORTANT! Нужно придумать механизм, который скажет методу NomenclatureSelected
		//с какой целью мы ее выбирали. Для аренды или для продажи.

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		void OpenNomenclatureSelectDlg (ICriteria criteria)
		{
//TODO Получить количества.
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature), UoWGeneric.Session, criteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if ((e.Subject as Nomenclature).Category == NomenclatureCategory.equipment) {
				ITdiTab mytab = TdiHelper.FindMyTab (this);
				if (mytab == null) {
					logger.Warn ("Родительская вкладка не найдена.");
					return;
				}

				OrmReference SelectDialog = new OrmReference (typeof(Equipment), UoWGeneric.Session, null);
				SelectDialog.Mode = OrmReferenceMode.Select;
				SelectDialog.ButtonMode = ReferenceButtonMode.TreatEditAsOpen | ReferenceButtonMode.CanEdit;

				SelectDialog.ObjectSelected += (s, ev) => {
				};

				mytab.TabParent.AddSlaveTab (mytab, SelectDialog);

			} else {
			}
		}
	}
}

