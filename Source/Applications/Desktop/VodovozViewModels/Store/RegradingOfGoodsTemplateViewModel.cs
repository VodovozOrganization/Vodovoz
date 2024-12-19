using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Store
{
	public class RegradingOfGoodsTemplateViewModel : EntityTabViewModelBase<RegradingOfGoodsTemplate>, IDisposable
	{
		public RegradingOfGoodsTemplateViewModel(
			IEntityUoWBuilder uowBuilder,
			RegradingOfGoodsTemplateItemsViewModel regradingOfGoodsTemplateItemsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(regradingOfGoodsTemplateItemsViewModel is null)
			{
				throw new ArgumentNullException(nameof(regradingOfGoodsTemplateItemsViewModel));
			}

			ItemsViewModel = regradingOfGoodsTemplateItemsViewModel;
			ItemsViewModel.SetUnitOfWork(UoW);
			ItemsViewModel.Items = Entity.Items;
			ItemsViewModel.ParentViewModel = this;

			SaveCommand = new DelegateCommand(SaveAndClose);
			CancelCommand = new DelegateCommand(() => Close(HasChanges, CloseSource.Cancel));
		}

		public RegradingOfGoodsTemplateItemsViewModel ItemsViewModel { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public override void Dispose()
		{
			ItemsViewModel?.Dispose();
			base.Dispose();
		}
	}
}
