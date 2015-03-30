using System;
using QSOrmProject;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSTDI;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DeliveryScheduleDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private ISession session;
		private Adaptor adaptor = new Adaptor ();
		private DeliverySchedule subject;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public bool HasChanges { 
			get{ return Session.IsDirty (); }
		}

		private string _tabName = "Новый график доставки";

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

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set {
				session = value;
			}
		}

		public object Subject {
			get { return subject; }
			set {
				if (value is DeliverySchedule)
					subject = value as DeliverySchedule;
			}
		}

		public DeliveryScheduleDlg ()
		{
			this.Build ();
			subject = new DeliverySchedule ();
			Session.Persist (subject);
			ConfigureDlg ();
		}

		public DeliveryScheduleDlg (int id)
		{
			this.Build ();
			subject = Session.Load<DeliverySchedule> (id);
			TabName = subject.Name;
			ConfigureDlg ();
		}

		public DeliveryScheduleDlg (DeliverySchedule sub)
		{
			this.Build ();
			subject = Session.Load<DeliverySchedule> (sub.Id);
			TabName = subject.Name;
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
		}

		public bool Save ()
		{
			var valid = new QSValidator<DeliverySchedule> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем график доставки...");
			Session.Flush ();
			OrmMain.NotifyObjectUpdated (subject);
			return true;
		}

		public override void Destroy ()
		{
			Session.Close ();
			adaptor.Disconnect ();
			base.Destroy ();
		}

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
	}
}

