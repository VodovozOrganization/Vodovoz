using QS.Project.Filter;

namespace Vodovoz.Filters.ViewModels
{
    public class ProductGroupFilterViewModel: FilterViewModelBase<ProductGroupFilterViewModel> 
    {
        private bool hideArchive;
        public bool HideArchive {
            get => hideArchive;
            set => UpdateFilterField(ref hideArchive, value);
        }
    }
}