using System;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using System.Collections.Generic;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementsView : Gtk.Bin
	{
		private IAdditionalAgreementOwner agreementOwner;
		private GenericObservableList<AdditionalAgreement> additionalAgreements;
		private ISession session;

		public ISession Session
		{
			get
			{
				return session;
			}
			set
			{
				session = value;
			}
		}

		public IAdditionalAgreementOwner AgreementOwner
		{
			get
			{
				return agreementOwner;
			}
			set
			{
				agreementOwner = value;
				if(agreementOwner.AdditionalAgreements == null)
					AgreementOwner.AdditionalAgreements = new List<AdditionalAgreement>();
				additionalAgreements = new GenericObservableList<AdditionalAgreement>(AgreementOwner.AdditionalAgreements);
				treeAdditionalAgreements.ItemsDataSource = additionalAgreements;
			}
		}

		OrmParentReference parentReference;
		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IAdditionalAgreementOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IAdditionalAgreementOwner)));
					}
					AgreementOwner = (IAdditionalAgreementOwner)parentReference.ParentObject;
				}
			}
			get {
				return parentReference;
			}
		}

		public AdditionalAgreementsView()
		{
			this.Build();
			treeAdditionalAgreements.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeAdditionalAgreements.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			//TODO Add switch logic
			ITdiDialog dlg = OrmMain.CreateObjectDialog (typeof(FreeRentAgreement), parentReference);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;
				
			ITdiDialog dlg = OrmMain.CreateObjectDialog (ParentReference, treeAdditionalAgreements.GetSelectedObjects () [0]);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnTreeAdditionalAgreementsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab(this);
			if (mytab == null)
				return;

			additionalAgreements.Remove (treeAdditionalAgreements.GetSelectedObjects () [0] as AdditionalAgreement);
		}
	}
}

