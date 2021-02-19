using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Security;

namespace Vodovoz.ViewModels.ViewModels.Security
{
    public class RegisteredRMViewModel : EntityTabViewModelBase<RegisteredRM>
    {
        public RegisteredRMViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null)
            : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
        {
        }

        public void AddUser(User user)
        {
            Entity.ObservableUsers.Add(user);
        }
    }
}
