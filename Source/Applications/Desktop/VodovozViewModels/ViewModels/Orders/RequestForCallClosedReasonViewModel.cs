using System;
using System.ComponentModel.DataAnnotations;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Orders;
using Vodovoz.Factories;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class RequestForCallClosedReasonViewModel : EntityDialogViewModelBase<RequestForCallClosedReason>, IAskSaveOnCloseViewModel
	{
		private readonly IPermissionResult _permissionResult;
		private readonly ICommonServices _commonServices;
		private readonly ValidationContext _validationContext;
		
		public RequestForCallClosedReasonViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			ICommonServices commonServices,
			IValidationContextFactory validationContextFactory)
			: base(uowBuilder, uowFactory, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_permissionResult = commonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(RequestForCallClosedReason));

			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);
			Title = Entity.ToString();
			
			CreateCommands();
		}

		public string IdToString => Entity.Id.ToString();
		public bool CanShowId => Entity.Id > 0;
		
		public bool AskSaveOnClose => CanEdit;
		public bool CanChangeName => CanEdit && Entity.Id == 0;
		public bool CanEdit => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;
		public DelegateCommand SaveAndCloseCommand { get; private set; }
		public DelegateCommand CloseCommand { get; private set; }
		
		private void CreateCommands()
		{
			CreateSaveAndCloseCommand();
			CreateCloseCommand();
		}

		private void CreateSaveAndCloseCommand()
		{
			SaveAndCloseCommand = new DelegateCommand(() => SaveAndClose());
			SaveAndCloseCommand.CanExecuteChangedWith(this, x => x.CanEdit);
		}

		private void CreateCloseCommand()
		{
			CloseCommand = new DelegateCommand(
				() => Close(false, CloseSource.Cancel)
			);
		}
		
		protected override bool Validate() => _commonServices.ValidationService.Validate(Entity, _validationContext);
	}
}
