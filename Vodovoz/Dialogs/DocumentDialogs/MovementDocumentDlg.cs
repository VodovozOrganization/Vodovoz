using System;
using System.Data.Bindings;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSTDI;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MovementDocumentDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
		ISession session;
		MovementDocument subject;
		Adaptor adaptor = new Adaptor ();
		bool isNew, isSaveButton;

		public MovementDocumentDlg ()
		{
			this.Build ();
			subject = new MovementDocument ();
			Session.Persist (subject);
			subject.TimeStamp = DateTime.Now;
			isNew = true;
			ConfigureDlg ();
		}

		public MovementDocumentDlg (int id)
		{
			this.Build ();
			subject = Session.Load<MovementDocument> (id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		public MovementDocumentDlg (MovementDocument sub)
		{
			this.Build ();
			subject = Session.Load<MovementDocument> (sub.Id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			adaptor.Target = subject;
			tableSender.DataSource = adaptor;
			tableCommon.DataSource = adaptor;
			tableReceiver.DataSource = adaptor;
			enumMovementType.DataSource = adaptor;
			referenceCounterpartyTo.SubjectType = typeof(Counterparty);
			referenceCounterpartyFrom.SubjectType = typeof(Counterparty);
			referenceCounterpartyTo.ItemsCriteria = session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.customer));
			referenceCounterpartyFrom.ItemsCriteria = session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.customer));
			referenceWarehouseTo.SubjectType = typeof(Warehouse);
			referenceWarehouseFrom.SubjectType = typeof(Warehouse);
			referenceDeliveryPointTo.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointFrom.SubjectType = typeof(DeliveryPoint);
			referenceEmployee.SubjectType = typeof(Employee);

		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Документ перемещения";

		public string TabName {
			get { return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
			}
		}

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			isSaveButton = true;
			var valid = new QSValidator<MovementDocument> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return (isSaveButton = false);

			logger.Info ("Сохраняем документ перемещения...");
			Session.Flush ();
			logger.Info ("Ok.");
			OrmMain.NotifyObjectUpdated (subject);

			return true;
		}

		public bool HasChanges {
			get { return Session.IsDirty (); }
		}

		#endregion

		#region IOrmDialog implementation

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		public object Subject {
			get { return subject; }
			set {
				if (value is MovementDocument)
					subject = value as MovementDocument;
			}
		}

		#endregion

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save ())
				OnCloseTab (false);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		public override void Destroy ()
		{
			if (isNew && !isSaveButton) {
				Session.Delete (subject);
				Session.Flush ();
			}
			Session.Close ();
			adaptor.Disconnect ();
			base.Destroy ();
		}

		protected void OnEnumMovementTypeChanged (object sender, EventArgs e)
		{
			var selected = (MovementDocumentCategory)enumMovementType.Active;
			referenceWarehouseTo.Sensitive = referenceWarehouseFrom.Sensitive = 
				(selected == MovementDocumentCategory.warehouse);
			referenceCounterpartyTo.Sensitive = referenceCounterpartyFrom.Sensitive =
				(selected == MovementDocumentCategory.counterparty);
			referenceDeliveryPointFrom.Sensitive = (referenceCounterpartyFrom.Subject != null && selected == MovementDocumentCategory.counterparty);
			referenceDeliveryPointTo.Sensitive = (referenceCounterpartyTo.Subject != null && selected == MovementDocumentCategory.counterparty);
		}

		protected void OnReferenceCounterpartyFromChanged (object sender, EventArgs e)
		{
			referenceDeliveryPointFrom.Sensitive = referenceCounterpartyFrom.Subject != null;
			if (referenceCounterpartyFrom.Subject != null) {
				var points = ((Counterparty)referenceCounterpartyFrom.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				referenceDeliveryPointFrom.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}

		protected void OnReferenceCounterpartyToChanged (object sender, EventArgs e)
		{
			referenceDeliveryPointTo.Sensitive = referenceCounterpartyTo.Subject != null;
			if (referenceCounterpartyTo.Subject != null) {
				var points = ((Counterparty)referenceCounterpartyTo.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				referenceDeliveryPointTo.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}
	}
}

