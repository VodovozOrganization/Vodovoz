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
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Journals;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;

namespace Vodovoz.ViewModels.ViewModels.Security
{
	public class RegisteredRMViewModel : EntityTabViewModelBase<RegisteredRM>
	{
		public RegisteredRMViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IPermissionRepository permissionRepository,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_permissionRepository = permissionRepository ?? throw new System.ArgumentNullException(nameof(permissionRepository));
		}

		private DelegateCommand _addUserCommand;

		public DelegateCommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new DelegateCommand(
			() =>
			{
				var userFilterViewModel = new UsersJournalFilterViewModel();
				var userJournalViewModel = new SelectUserJournalViewModel(
					userFilterViewModel,
					UnitOfWorkFactory,
					ServicesConfig.CommonServices)
				{
					SelectionMode = JournalSelectionMode.Single,
				};

				userJournalViewModel.OnEntitySelectedResult += (s, ea) =>
				{
					var selectedNode = ea.SelectedNodes.FirstOrDefault();
					if(selectedNode == null)
					{
						return;
					}

					var user = UoWGeneric.Session.Get<User>(selectedNode.Id);

					Entity.ObservableUsers.Add(user);
				};

				TabParent.AddSlaveTab(this, userJournalViewModel);
			}, () => true
		));

		private DelegateCommand<User> _removeUserCommand;
		private readonly IPermissionRepository _permissionRepository;

		public DelegateCommand<User> RemoveUserCommand => _removeUserCommand ?? (_removeUserCommand = new DelegateCommand<User>(
			(user) =>
			{
				if(user != null)
				{
					Entity.ObservableUsers.Remove(user);
				}
			}, (user) => true
		));
	}
}
