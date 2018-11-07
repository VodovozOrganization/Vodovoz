using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Tdi;

namespace Vodovoz
{
	public partial class StockBalanceView : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
				stockbalancefilter1.SetAndRefilterAtOnce(
					x => x.UoW = value,
					x => x.ShowArchive = true
				);
				var vm = new ViewModel.StockBalanceVM (value);
				vm.Filter = stockbalancefilter1;
				datatreeviewBalance.RepresentationModel = vm;
			}
		}

		public StockBalanceView ()
		{
			this.Build ();
			this.TabName = "Складские остатки";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
		}

		protected void OnSearchentity1TextChanged(object sender, System.EventArgs e)
		{
			datatreeviewBalance.SearchHighlightText = searchentity1.Text;
			datatreeviewBalance.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonRefreshClicked(object sender, System.EventArgs e)
		{
			datatreeviewBalance.RepresentationModel?.UpdateNodes();
		}
	}
}

