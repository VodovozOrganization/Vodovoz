using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Refunds;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels.ViewModels.Refunds
{
	public class RefundViewModel : EntityTabViewModelBase<RefundEntity>, IAskSaveOnCloseViewModel
	{
		private readonly IGenericRepository<RefundEntity> _refundRepository;
		public RefundViewModel(IEntityUoWBuilder uowBuilder, 
								IUnitOfWorkFactory unitOfWorkFactory, 
								ICommonServices commonServices,
								IGenericRepository<RefundEntity> refundRepository,
								INavigationManager navigation = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_refundRepository = refundRepository ?? throw new ArgumentNullException(nameof(refundRepository));
			Initialize();
		}

		public bool CanEdit { get; private set; }
		public ICommand SaveCommand { get; private set; }
		public ICommand CancelCommand { get; private set; }
		public bool AskSaveOnClose => CanEdit;

		private void Initialize()
		{
			var saveCommand = new DelegateCommand(SaveAndClose);
			saveCommand.CanExecuteChangedWith(this, x => x.CanEdit);
			SaveCommand = saveCommand;

			var cancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			CancelCommand = cancelCommand;

			CanEdit = (Entity.Id == 0 && PermissionResult.CanCreate) || PermissionResult.CanUpdate;
		}
	}
}
