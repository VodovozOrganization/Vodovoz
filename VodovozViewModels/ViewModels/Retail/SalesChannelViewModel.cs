using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Retail;

namespace Vodovoz.ViewModels.ViewModels.Retail
{
    public class SalesChannelViewModel : EntityTabViewModelBase<SalesChannel>
    {
        public SalesChannelViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null)
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {

        }
    }
}
