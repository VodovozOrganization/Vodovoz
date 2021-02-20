using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Security;
using Vodovoz.Journals;
using Vodovoz.Journals.FilterViewModels.Employees;

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

        public void RemoveUser(User user)
        {
            Entity.ObservableUsers.Remove(user);
        }

        public void OpenUserSelectionTab()
        {
            var userFilterViewModel = new UserJournalFilterViewModel();
            var userJournalViewModel = new UserJournalViewModel(
                    userFilterViewModel,
                    UnitOfWorkFactory,
                    ServicesConfig.CommonServices)
            {
                SelectionMode = JournalSelectionMode.Single,
            };

            userJournalViewModel.OnEntitySelectedResult += (s, ea) => {
                var selectedNode = ea.SelectedNodes.FirstOrDefault();
                if (selectedNode == null)
                    return;

                AddUser(UoWGeneric.Session.Get<User>(selectedNode.Id));
            };
            TabParent.AddSlaveTab(this, userJournalViewModel);
        }
    }
}
