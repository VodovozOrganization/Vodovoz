using System;
using System.Collections.Generic;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Dialogs.Client;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementsView : WidgetOnDialogBase, IEditableDialog
	{
		IUnitOfWorkGeneric<CounterpartyContract> agreementUoW;

		public IUnitOfWorkGeneric<CounterpartyContract> AgreementUoW {
			get { return agreementUoW; }
			set {
				if (agreementUoW == value)
					return;
				agreementUoW = value;
				if (AgreementUoW.Root.AdditionalAgreements == null)
					AgreementUoW.Root.AdditionalAgreements = new List<AdditionalAgreement> ();
				treeAdditionalAgreements.RepresentationModel = new ViewModel.AdditionalAgreementsVM (value);
				treeAdditionalAgreements.RepresentationModel.UpdateNodes ();
			}
		}

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value; 
				buttonAdd.Sensitive = buttonDelete.Sensitive = 
					treeAdditionalAgreements.Sensitive = buttonEdit.Sensitive = value;
			}
		}

		public AdditionalAgreementsView ()
		{
			this.Build ();
			buttonAdd.ItemsEnum = typeof(AgreementType);
			buttonAdd.SetSensitive (AgreementType.FreeRent, false);
			buttonAdd.SetSensitive (AgreementType.DailyRent, false);
			buttonAdd.SetSensitive (AgreementType.NonfreeRent, false);
			treeAdditionalAgreements.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeAdditionalAgreements.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		void OnButtonAddClicked (AgreementType type)
		{
			if (MyOrmDialog.UoW.IsNew) {
				if (CommonDialogs.SaveBeforeCreateSlaveEntity (MyOrmDialog.EntityObject.GetType (), typeof(AdditionalAgreement))) {
					if (!MyTdiDialog.Save ())
						return;
				} else
					return;
			}

			ITdiDialog dlg;
			switch (type) {
			case AgreementType.FreeRent:
				dlg = new FreeRentAgreementDlg (AgreementUoW.Root);
				break;
			case AgreementType.NonfreeRent:
				dlg = new NonFreeRentAgreementDlg (AgreementUoW.Root);
				break;
			case AgreementType.WaterSales:
				dlg = new WaterAgreementDlg (AgreementUoW.Root);
				break;
			case AgreementType.DailyRent:
				dlg = new DailyRentAgreementDlg (AgreementUoW.Root);
				break;
			case AgreementType.EquipmentSales:
				dlg = new EquipSalesAgreementDlg(AgreementUoW.Root);
				break;
			case AgreementType.Repair:
				if (AgreementUoW.Root.RepairAgreementExists ()) {
					MessageDialogWorks.RunWarningDialog ("Доп. соглашение на ремонт оборудования уже существует. " +
					"Нельзя создать более одного доп. соглашения данного типа.");
					return;
				}
				dlg = new RepairAgreementDlg (AgreementUoW.Root);
				break;
			default:
				throw new NotSupportedException (String.Format ("Тип {0} пока не поддерживается.", type));
			}
			MyTab.TabParent.AddSlaveTab (MyTab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			if (treeAdditionalAgreements.GetSelectedObjects ().GetLength (0) > 0) {
				int id = (treeAdditionalAgreements.GetSelectedObjects () [0] as ViewModel.AdditionalAgreementVMNode).Id;
				var agreement = AgreementUoW.GetById<AdditionalAgreement> (id);
				ITdiDialog dlg = OrmMain.CreateObjectDialog (agreement);
				mytab.TabParent.AddSlaveTab (mytab, dlg);
			}
		}

		protected void OnTreeAdditionalAgreementsRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			OrmMain.DeleteObject (typeof(AdditionalAgreement), (treeAdditionalAgreements.GetSelectedObjects () [0] as ViewModel.AdditionalAgreementVMNode).Id);
			treeAdditionalAgreements.RepresentationModel.ItemsList.Remove (treeAdditionalAgreements.GetSelectedObjects () [0]);
		}

		protected void OnButtonAddEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			OnButtonAddClicked ((AgreementType)e.ItemEnum);
		}

		protected void OnButtonDeactivateClicked (object sender, EventArgs e)
		{
			if (MessageDialogWorks.RunQuestionDialog ("Вы действительно хотите закрыть данное доп. соглашение?"))
				(treeAdditionalAgreements.GetSelectedObjects () [0] as AdditionalAgreement).IsCancelled = true;
			//TODO Скрыть из выборки
		}
	}
}