using QS.Commands;
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

        private DelegateCommand addUserCommand;
        public DelegateCommand AddUserCommand => addUserCommand ?? (addUserCommand = new DelegateCommand(
            () => {
                var userFilterViewModel = new UserJournalFilterViewModel();
                var userJournalViewModel = new UserJournalViewModel(
                        userFilterViewModel,
                        UnitOfWorkFactory,
                        ServicesConfig.CommonServices)
                {
                    SelectionMode = JournalSelectionMode.Single,
                };

                userJournalViewModel.OnEntitySelectedResult += (s, ea) =>
                {
                    var selectedNode = ea.SelectedNodes.FirstOrDefault();
                    if (selectedNode == null)
                        return;

                    var user = UoWGeneric.Session.Get<User>(selectedNode.Id);

                    Entity.ObservableUsers.Add(user);
                };

                TabParent.AddSlaveTab(this, userJournalViewModel);
            }, () => true
        ));

        private DelegateCommand<User> removeUserCommand;
        public DelegateCommand<User> RemoveUserCommand => removeUserCommand ?? (removeUserCommand = new DelegateCommand<User>(
            (user) => {
                if (user != null)
                {
                    Entity.ObservableUsers.Remove(user);
                }
            }, (user) => true
        ));
    }
}
