using System;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EquipmentGenerator : Gtk.Bin, ITdiDialog
	{
		#region ITdiTab implementation

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Регистрация оборудования";

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
			get { return false; }
		}

		#endregion

		public EquipmentGenerator ()
		{
			this.Build ();
		}
	}
}

