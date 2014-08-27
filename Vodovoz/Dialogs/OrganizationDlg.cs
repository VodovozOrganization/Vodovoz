using System;
using QSTDI;
using QSOrmProject;
using NHibernate;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrganizationDlg : Gtk.Bin, QSTDI.ITdiDialog
	{
		private ISession sesion = OrmMain.Sessions.OpenSession();
		private Organization obj;

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public bool HasChanges { get; private set;}

		private string _tabName = "Новая организация";
		public string TabName
		{
			get{return _tabName;}
			set{
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}

		}

		public OrganizationDlg()
		{
			this.Build();
			obj = new Organization();
		}

		public OrganizationDlg(int id)
		{
			this.Build();
			obj = sesion.Load<Organization>(id);
		}

		public bool Save()
		{
			sesion.SaveOrUpdate(obj);
			return true;
		}

		protected override bool OnDestroyEvent(Gdk.Event evnt)
		{
			sesion.Close;
			return base.OnDestroyEvent(evnt);
		}
	}
}

