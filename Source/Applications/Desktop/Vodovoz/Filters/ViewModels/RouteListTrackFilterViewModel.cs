using QS.Project.Filter;

namespace Vodovoz.Filters.ViewModels
{
    public class RouteListTrackFilterViewModel : FilterViewModelBase<RouteListTrackFilterViewModel> 
    {
        private bool _isFastDeliveryOnly;
        private bool _showFastDeliveryCircle;

		public bool IsFastDeliveryOnly
        {
            get => _isFastDeliveryOnly;
            set
            {
	            UpdateFilterField(ref _isFastDeliveryOnly, value);
	            if(!value)
	            {
		            ShowFastDeliveryCircle = false;
	            }
            }
        }

        public bool ShowFastDeliveryCircle
        {
	        get => _showFastDeliveryCircle;
	        set => UpdateFilterField(ref _showFastDeliveryCircle, value);
        }
    }
}
