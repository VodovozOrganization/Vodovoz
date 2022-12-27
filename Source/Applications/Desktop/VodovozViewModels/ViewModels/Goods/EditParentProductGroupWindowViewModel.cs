using System;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.Commands;
using Vodovoz.Domain.Goods;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class EditParentProductGroupWindowViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly IInteractiveService _interactiveService;
		private DelegateCommand _moveToParentGroupCommand;
		private ProductGroup _parentProductGroup;

		public EditParentProductGroupWindowViewModel(
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IUnitOfWork uow) : base(navigationManager)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public IUnitOfWork UoW { get; }

		public ProductGroup ParentProductGroup
		{
			get => _parentProductGroup;
			set => SetField(ref _parentProductGroup, value);
		}

		public DelegateCommand MoveToParentGroupCommand => _moveToParentGroupCommand ?? (
			_moveToParentGroupCommand = new DelegateCommand(
				() => { }));
				
		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
