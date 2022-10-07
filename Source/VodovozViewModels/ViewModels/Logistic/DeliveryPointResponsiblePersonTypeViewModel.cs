using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
    public class DeliveryPointResponsiblePersonTypeViewModel : EntityTabViewModelBase<DeliveryPointResponsiblePersonType>
    {
        public DeliveryPointResponsiblePersonTypeViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null)
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        { }
    }
}
