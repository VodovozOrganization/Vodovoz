using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
namespace Vodovoz.ViewModels.ViewModels.Warehouses
{
	public class InventoryInstanceMovementReportViewModel : DialogTabViewModelBase
	{
		public InventoryInstanceMovementReportViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService) :base(uowFactory, interactiveService, navigation)
		{
		}
	}
}
