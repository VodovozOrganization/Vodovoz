using System;
using System.Data.Bindings;
using QSOrmProject;
using QSTDI;
using NHibernate;
using NLog;
using System.Collections.Generic;
using NHibernate.Criterion;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaidRentPackageDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptor = new Adaptor();
		private PaidRentPackage subject;
		private bool NewItem = false;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return NewItem || Session.IsDirty();}
		}

		private string _tabName = "Новый пакет платных услуг";
		public string TabName
		{
			get{return _tabName;}
			set{
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}

		}

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession();
				return session;
			}
			set {
				session = value;
			}
		}

		public object Subject
		{
			get {return subject;}
			set {
				if (value is PaidRentPackage)
					subject = value as PaidRentPackage;
			}
		}

		public PaidRentPackageDlg()
		{
			this.Build();
			NewItem = true;
			subject = new PaidRentPackage();
			ConfigureDlg();
		}

		public PaidRentPackageDlg(int id)
		{
			this.Build();
			subject = Session.Load<PaidRentPackage>(id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public PaidRentPackageDlg(PaidRentPackage sub)
		{
			this.Build();
			subject = Session.Load<PaidRentPackage>(sub.Id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			spinRentPeriod.Sensitive = !checkbuttonDaily.Active;
			referenceDepositService.SubjectType = typeof(Nomenclature);
			referenceDepositService.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq("Category", NomenclatureCategory.service));
			referenceRentService.SubjectType = typeof(Nomenclature);
			referenceRentService.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq("Category", NomenclatureCategory.rent));
		}

		public bool Save()
		{
			logger.Info("Сохраняем пакет платных услуг...");
			if (checkbuttonDaily.Active)
				spinRentPeriod.Value = 0;
			Session.SaveOrUpdate(subject);
			Session.Flush();
			OrmMain.NotifyObjectUpdated(subject);
			return true;
		}

		public override void Destroy()
		{
			Session.Close();
			adaptor.Disconnect ();
			base.Destroy();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab(false);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

		protected void OnCheckbuttonDailyToggled (object sender, EventArgs e)
		{
			spinRentPeriod.Sensitive = !checkbuttonDaily.Active;
		}
	}
}

