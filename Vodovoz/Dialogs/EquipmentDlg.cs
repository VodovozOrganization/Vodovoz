using System;
using QSOrmProject;
using NHibernate;
using NLog;
using System.Data.Bindings;
using QSTDI;
using QSValidation;
using NHibernate.Criterion;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EquipmentDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private ISession session;
		private Adaptor adaptor = new Adaptor ();
		private Equipment subject;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public bool HasChanges { 
			get{ return Session.IsDirty (); }
		}

		private string _tabName = "Новое оборудование";

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
				if (value is Equipment)
					subject = value as Equipment;
			}
		}

		public EquipmentDlg ()
		{
			this.Build ();
			subject = new Equipment ();
			Session.Persist (subject);
			ConfigureDlg ();
		}

		public EquipmentDlg (int id)
		{
			this.Build ();
			subject = Session.Load<Equipment> (id);
			TabName = subject.Nomenclature.Name;
			ConfigureDlg ();
		}

		public EquipmentDlg (Equipment sub)
		{
			this.Build ();
			subject = Session.Load<Equipment> (sub.Id);
			TabName = subject.Nomenclature.Name;
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			referenceNomenclature.SubjectType = typeof(Nomenclature);
			referenceNomenclature.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.equipment))
				.Add (Restrictions.Eq ("Serial", true));
		}

		public bool Save ()
		{
			var valid = new QSValidator<Equipment> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			
			logger.Info ("Сохраняем оборудование...");
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

