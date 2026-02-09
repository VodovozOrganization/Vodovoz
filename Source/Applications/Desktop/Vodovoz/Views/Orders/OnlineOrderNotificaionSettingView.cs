using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class OnlineOrderNotificationSettingView : TabViewBase<OnlineOrderNotificationSettingViewModel>
	{
		public OnlineOrderNotificationSettingView(OnlineOrderNotificationSettingViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yenumcmbNotificationEvent.ItemsEnum = typeof(ExternalOrderStatus);
			yenumcmbNotificationEvent.Binding
				.AddBinding(ViewModel.Entity, e => e.ExternalOrderStatus, w => w.SelectedItem)
				.InitializeFromSource();

			yentryNotificationText.Binding
				.AddBinding(ViewModel.Entity, e => e.NotificationText, w => w.Text)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);
		}
	}
}
