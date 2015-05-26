using System;
using QSTDI;
using System.IO;
using QSProjectsLib;

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
			string ReportPath = System.IO.Path.Combine (Directory.GetCurrentDirectory (), "Reports", "Equipment" + ".rdl");
			string Parameters = "dup=1&equipment_id=1,2,3";
			reportviewer2.LoadReport (new Uri (ReportPath), Parameters, QSMain.ConnectionString);
		}

		protected void OnZoomInActionActivated (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected void OnZoomOutActionActivated (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}


		protected void OnPrintActionActivated (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected void OnPdfActionActivated (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}


		protected void OnRefreshActionActivated (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

