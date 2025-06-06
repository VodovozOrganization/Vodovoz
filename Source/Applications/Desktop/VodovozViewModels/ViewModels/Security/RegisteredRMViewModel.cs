using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System.Linq;
using Vodovoz.Core.Domain.Users;
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
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
		}

		private DelegateCommand _addUserCommand;

		public DelegateCommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new DelegateCommand(
			() =>
			{
				var userFilterViewModel = new UsersJournalFilterViewModel();
				var userJournalViewModel = new SelectUserJournalViewModel(
					userFilterViewModel,
					UnitOfWorkFactory,
					CommonServices)
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

					Entity.Users.Add(user);
				};

				TabParent.AddSlaveTab(this, userJournalViewModel);
			}, () => true
		));

		private DelegateCommand<User> _removeUserCommand;

		public DelegateCommand<User> RemoveUserCommand => _removeUserCommand ?? (_removeUserCommand = new DelegateCommand<User>(
			(user) =>
			{
				if(user != null)
				{
					Entity.Users.Remove(user);
				}
			}, (user) => true
		));
	}
}
