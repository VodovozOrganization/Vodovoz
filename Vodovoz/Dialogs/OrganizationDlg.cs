using System;
using QSTDI;
using QSOrmProject;
using NHibernate;
using System.Data.Bindings;
using NLog;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrganizationDlg : Gtk.Bin, QSTDI.ITdiDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession sesion = OrmMain.Sessions.OpenSession();
		private Adaptor adaptorOrg = new Adaptor();
		private Organization obj;

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return sesion.IsDirty();}
		}

		private string _tabName = "Новая организация";
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

		public OrganizationDlg()
		{
			this.Build();
			obj = new Organization();
			adaptorOrg.Target = obj;
			datatableMain.DataSource = adaptorOrg;
		}

		public OrganizationDlg(int id)
		{
			this.Build();
			obj = sesion.Load<Organization>(id);
			adaptorOrg.Target = obj;
			datatableMain.DataSource = adaptorOrg;
		}

		public bool Save()
		{
			logger.Info("Сохраняем организацию...");
			sesion.SaveOrUpdate(obj);
			sesion.Flush();
			return true;
		}

		public override void Destroy()
		{
			sesion.Close();
			//adaptorOrg.Disconnect();
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
	}
}

