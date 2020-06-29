using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Logistic
{
    public class DistrictViewModel : EntityTabViewModelBase<District>
    {
        public DistrictViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null) 
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {
            
        }
    }
}