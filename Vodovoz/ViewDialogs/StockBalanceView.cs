using System;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class StockBalanceView : Gtk.Bin, ITdiJournal
	{
		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {if (uow == value)
				return;
				uow = value;
				stockbalancefilter1.UoW = value;
				var vm = new ViewModel.StockBalanceVM(value);
				vm.Filter = stockbalancefilter1;
				datatreeviewBalance.RepresentationModel = vm;
				datatreeviewBalance.RepresentationModel.UpdateNodes ();
			}
		}

		public StockBalanceView ()
		{
			this.Build ();

			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			datatreeviewBalance.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewBalance.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}


		protected void OndatatreeviewBalanceRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Складские остатки";

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
	}
}

