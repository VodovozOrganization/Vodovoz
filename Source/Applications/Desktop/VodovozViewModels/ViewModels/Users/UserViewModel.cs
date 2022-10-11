using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels
{
    public class UserViewModel : EntityTabViewModelBase<User>
    {
        public UserViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null) 
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {
        }
    }
}
