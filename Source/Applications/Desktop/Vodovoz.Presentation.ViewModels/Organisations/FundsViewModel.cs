using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class FundsViewModel : EntityDialogViewModelBase<Funds>
	{
		private readonly IPermissionResult _permissionResult;

		public FundsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			IValidator validator) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			_permissionResult =
				(currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService)))
				.ValidateEntityPermission(typeof(Funds));
			CreateCommands();
		}

		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }

		public bool CanEdit => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;
		public string IdString => Entity.Id.ToString();
		public bool CanShowId => Entity.Id > 0;

		private void CreateCommands()
		{
			SaveCommand = new DelegateCommand(() => SaveAndClose());
			SaveCommand.CanExecuteChangedWith(this, x => x.CanEdit);
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}
	}
}
