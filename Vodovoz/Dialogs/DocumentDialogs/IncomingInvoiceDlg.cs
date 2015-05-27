using System;
using System.Data.Bindings;
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
	public partial class IncomingInvoiceDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
		ISession session;
		IncomingInvoice subject;
		Adaptor adaptor = new Adaptor ();
		bool isNew, isSaveButton;

		public IncomingInvoiceDlg ()
		{
			this.Build ();
			subject = new IncomingInvoice ();
			Session.Persist (subject);
			subject.TimeStamp = DateTime.Now;
			isNew = true;
			ConfigureDlg ();
		}

		public IncomingInvoiceDlg (int id)
		{
			this.Build ();
			subject = Session.Load<IncomingInvoice> (id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		public IncomingInvoiceDlg (IncomingInvoice sub)
		{
			this.Build ();
			subject = Session.Load<IncomingInvoice> (sub.Id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			adaptor.Target = subject;
			tableInvoice.DataSource = adaptor;
			referenceContractor.SubjectType = typeof(Counterparty);
			referenceWarehouse.SubjectType = typeof(Warehouse);
			referenceContractor.ItemsCriteria = Session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.supplier));
			incominginvoiceitemsview1.ParentReference = new OrmParentReference (Session, Subject, "Items");
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Входящая накладная";

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
			var valid = new QSValidator<IncomingInvoice> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return (isSaveButton = false);

			logger.Info ("Сохраняем входящую накладную...");
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
					session = OrmMain.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		public object Subject {
			get { return subject; }
			set {
				if (value is IncomingInvoice)
					subject = value as IncomingInvoice;
			}
		}

		#endregion

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!HasChanges || Save ())
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
	}
}

