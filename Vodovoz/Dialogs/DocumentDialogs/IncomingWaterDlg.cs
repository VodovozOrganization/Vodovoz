using System;
using QSTDI;
using QSOrmProject;
using NHibernate;
using QSValidation;
using NLog;
using System.Data.Bindings;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingWaterDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
		ISession session;
		IncomingWater subject;
		Adaptor adaptor = new Adaptor ();

		public IncomingWaterDlg ()
		{
			this.Build ();
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
			var valid = new QSValidator<IncomingWater> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

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
					Session = OrmMain.Sessions.OpenSession ();
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

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		public override void Destroy ()
		{
			Session.Close ();
			adaptor.Disconnect ();
			base.Destroy ();
		}
	}
}

