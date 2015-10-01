using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	public partial class StockBalanceView : TdiTabBase
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
				stockbalancefilter1.UoW = value;
				var vm = new ViewModel.StockBalanceVM (value);
				vm.Filter = stockbalancefilter1;
				datatreeviewBalance.RepresentationModel = vm;
				datatreeviewBalance.RepresentationModel.UpdateNodes ();
			}
		}

		public StockBalanceView ()
		{
			this.Build ();
			this.TabName = "Складские остатки";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
		}
	}
}

