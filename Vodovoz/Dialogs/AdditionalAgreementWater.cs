using System;
using System.Data.Bindings;
using NHibernate;
using NLog;
using QSOrmProject;
using QSTDI;
using QSValidation;
using NHibernate.Criterion;
using System.Collections.Generic;
using Gtk;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementWater : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();
		protected ISession session;
		protected Adaptor adaptor = new Adaptor ();
		protected IAdditionalAgreementOwner AgreementOwner;

		public bool HasChanges {
			get { return false; }
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Новое доп. соглашение";

		public string TabName {
			get{ return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
			}
		}

		#endregion

		#region IOrmDialog implementation

		public NHibernate.ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set {
				session = value;
			}
		}

		#endregion

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

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (Save ())
				OnCloseTab (false);
		}

		protected void OnCloseTab (bool askSave)
		{
			if (TabParent.CheckClosingSlaveTabs ((ITdiTab)this))
				return;

			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		public override void Destroy ()
		{
			if (checkForAgreementExists ())
				(parentReference.ParentObject as IAdditionalAgreementOwner).AdditionalAgreements.Remove (subject);
			adaptor.Disconnect ();
			base.Destroy ();
		}

		private WaterSalesAgreement subject;

		public object Subject {
			get {
				return subject;
			}
			set {
				if (value is WaterSalesAgreement)
					subject = value as WaterSalesAgreement;
			}
		}

		public AdditionalAgreementWater (OrmParentReference parentReference, WaterSalesAgreement sub)
		{
			this.Build ();
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.AgreementTypeTitle + " " + subject.AgreementNumber;
			ConfigureDlg ();
		}


		private void ConfigureDlg ()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			entryAgreementNumber.IsEditable = true;
			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in (parentReference.ParentObject as CounterpartyContract).Counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.In ("Id", identifiers));
			dataAgreementType.Text = (parentReference.ParentObject as CounterpartyContract).Number + " - В";
			spinFixedPrice.Sensitive = labelCurrency.Sensitive = subject.IsFixedPrice;
		}

		public bool Save ()
		{
			var valid = new QSValidator<WaterSalesAgreement> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			if (checkForAgreementExists ()) {
				String message;
				if (subject.DeliveryPoint != null)
					message = "Дополнительное соглашение по продаже воды для данной точки доставки уже существует.";
				else
					message = "Общее дополнительное соглашение по продаже воды уже существует.";
				MessageDialog md = new MessageDialog (null, 
					                   DialogFlags.DestroyWithParent, 
					                   MessageType.Error,
					                   ButtonsType.Close,
					                   message);
				md.Show ();
				md.Run ();
				md.Destroy ();
				return false;
			}
			OrmMain.DelayedNotifyObjectUpdated (ParentReference.ParentObject, subject);
			return true;
		}

		protected void OnCheckIsFixedPriceToggled (object sender, EventArgs e)
		{
			spinFixedPrice.Sensitive = labelCurrency.Sensitive = subject.IsFixedPrice;
		}

		bool checkForAgreementExists ()
		{
			var agreements = new List<AdditionalAgreement> ();
			foreach (AdditionalAgreement d in (parentReference.ParentObject as IAdditionalAgreementOwner).AdditionalAgreements)
				if (d is WaterSalesAgreement)
					agreements.Add (d);
			if (agreements.FindAll (m => m.DeliveryPoint == subject.DeliveryPoint).Count > 1)
				return true;
			return false;
		}
	}
}

