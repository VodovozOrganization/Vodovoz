using System;
using QSTDI;
using QSOrmProject;
using NHibernate;
using QSValidation;
using NLog;
using System.Data.Bindings;
using NHibernate.Criterion;
using System.Linq;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WriteoffDocumentDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
		ISession session;
		WriteoffDocument subject;
		Adaptor adaptor = new Adaptor ();
		bool isNew, isSaveButton;

		public WriteoffDocumentDlg ()
		{
			this.Build ();
			subject = new WriteoffDocument ();
			Session.Persist (subject);
			subject.TimeStamp = DateTime.Now;
			isNew = true;
			ConfigureDlg ();
		}

		public WriteoffDocumentDlg (int id)
		{
			this.Build ();
			subject = Session.Load<WriteoffDocument> (id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		public WriteoffDocumentDlg (WriteoffDocument sub)
		{
			this.Build ();
			subject = Session.Load<WriteoffDocument> (sub.Id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			adaptor.Target = subject;
			tableWriteoff.DataSource = adaptor;
			referenceCounterparty.SubjectType = typeof(Counterparty);
			referenceCounterparty.ItemsCriteria = session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.customer));
			referenceWarehouse.SubjectType = typeof(Warehouse);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceEmployee.SubjectType = typeof(Employee);
			comboType.Sensitive = true;
			comboType.ItemsEnum = typeof(WriteoffType);
			referenceWarehouse.Sensitive = (subject.WriteoffWarehouse != null);
			referenceDeliveryPoint.Sensitive = referenceCounterparty.Sensitive = (subject.Client != null);
			comboType.EnumItemSelected += (object sender, EnumItemClickedEventArgs e) => {
				referenceDeliveryPoint.Sensitive = (comboType.Active == (int)WriteoffType.counterparty && subject.Client != null);
				referenceCounterparty.Sensitive = (comboType.Active == (int)WriteoffType.counterparty);
				referenceWarehouse.Sensitive = (comboType.Active == (int)WriteoffType.warehouse);
			};
			comboType.Active = subject.Client != null ?
				(int)WriteoffType.counterparty :
				(int)WriteoffType.warehouse;
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Акт списания";

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
			if (comboType.Active == (int)WriteoffType.counterparty) {
				subject.WriteoffWarehouse = null;
			} else {
				subject.Client = null;
				subject.DeliveryPoint = null;
			}
			var valid = new QSValidator<WriteoffDocument> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return (isSaveButton = false);

			logger.Info ("Сохраняем акт списания...");
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
				if (value is WriteoffDocument)
					subject = value as WriteoffDocument;
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

		protected void OnReferenceCounterpartyChanged (object sender, EventArgs e)
		{
			referenceDeliveryPoint.Sensitive = referenceCounterparty.Subject != null;
			if (referenceCounterparty.Subject != null) {
				var points = ((Counterparty)referenceCounterparty.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}
	}
}

