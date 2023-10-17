using QS.Project.Filter;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public partial class FastDeliveryOrderTransferFilterViewModel
		: FilterViewModelBase<FastDeliveryOrderTransferFilterViewModel>
	{
		private FastDeliveryOrderTransferMode _mode;

		public FastDeliveryOrderTransferMode Mode
		{
			get => _mode;
			set => UpdateFilterField(ref _mode, value);
		}
	}
}
