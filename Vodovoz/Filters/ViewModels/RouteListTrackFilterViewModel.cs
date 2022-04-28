using QS.Project.Filter;

namespace Vodovoz.Filters.ViewModels
{
    public class RouteListTrackFilterViewModel : FilterViewModelBase<RouteListTrackFilterViewModel> 
    {
        private bool _isFastDeliveryOnly;
        public bool IsFastDeliveryOnly
        {
            get => _isFastDeliveryOnly;
            set => UpdateFilterField(ref _isFastDeliveryOnly, value);
        }
    }
}
