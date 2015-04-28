using System;
using QSTDI;
using QSOrmProject;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WriteoffDocumentDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		public WriteoffDocumentDlg ()
		{
			this.Build ();
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
			throw new NotImplementedException ();
		}

		public bool HasChanges {
			get { throw new NotImplementedException (); }
		}

		#endregion

		#region IOrmDialog implementation

		public NHibernate.ISession Session {
			get { throw new NotImplementedException (); }
		}

		public object Subject {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion
	}
}

