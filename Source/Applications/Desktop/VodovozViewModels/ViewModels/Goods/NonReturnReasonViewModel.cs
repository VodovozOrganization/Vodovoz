using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NonReturnReasonViewModel : EntityTabViewModelBase<NonReturnReason>
	{
		public NonReturnReasonViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Причина невозврата имущества";

			SaveCommand = new DelegateCommand(SaveAndClose);
			CloseCommand = new DelegateCommand(Close);
		}

		private void Close()
		{
			Close(false, CloseSource.Cancel);
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }
	}
}
