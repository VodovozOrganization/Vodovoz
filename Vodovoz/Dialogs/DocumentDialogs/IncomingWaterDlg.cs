using System;
using System.Data.Bindings;
using NHibernate;
using NLog;
using QSOrmProject;
using QSTDI;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingWaterDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
		ISession session;
		IncomingWater subject;
		Adaptor adaptor = new Adaptor ();
		bool isNew, isSaveButton;

		public IncomingWaterDlg ()
		{
			this.Build ();
			subject = new IncomingWater ();
			Session.Persist (subject);
			subject.TimeStamp = DateTime.Now;
			isNew = true;
			ConfigureDlg ();
		}

		public IncomingWaterDlg (int id)
		{
			this.Build ();
			subject = Session.Load<IncomingWater> (id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		public IncomingWaterDlg (IncomingWater sub)
		{
			this.Build ();
			subject = Session.Load<IncomingWater> (sub.Id);
			TabName = subject.Number;
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			adaptor.Target = subject;
			tableWater.DataSource = adaptor;
			referenceWarehouse.SubjectType = typeof(Warehouse);
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Документ производства";

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
			var valid = new QSValidator<IncomingWater> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return (isSaveButton = false);

			logger.Info ("Сохраняем документ производства...");
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
				if (value is IncomingWater)
					subject = value as IncomingWater;
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
	}
}

