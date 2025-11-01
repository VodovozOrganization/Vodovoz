using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderNotificationSettingViewModel : EntityTabViewModelBase<OnlineOrderNotificationSetting>
	{
		public OnlineOrderNotificationSettingViewModel(
			IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = "Настройка уведомления для онлайн заказов";

			SaveCommand = new DelegateCommand(SaveAndClose);
			CloseCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}

		public DelegateCommand SaveCommand { get; set; }
		public DelegateCommand CloseCommand { get; set; }
	}
}
