using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Orders.OrderEnums;
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
			yenumcmbNotificationEvent.ItemsEnum = typeof(CustomerNotificationEventType);
			yenumcmbNotificationEvent.Binding
				.AddBinding(ViewModel.Entity, e => e.CustomerNotificationEventType, w => w.SelectedItem)
				.InitializeFromSource();

			yentryNotificationText.Binding
				.AddBinding(ViewModel.Entity, e => e.NotificationText, w => w.Text)
				.InitializeFromSource();
			
			yenumcmbNotificationClassification.ItemsEnum = typeof(CustomerNotificationClassification);
			yenumcmbNotificationClassification.Binding
				.AddBinding(ViewModel.Entity, e => e.NotificationClassification, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcmbCustomerNotificationPushType.ItemsEnum = typeof(CustomerNotificationPushType);
			yenumcmbCustomerNotificationPushType.Binding
				.AddBinding(ViewModel.Entity, e => e.PushType, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcmbCustomerNotificationTargetType.ItemsEnum = typeof(CustomerNotificationTargetType);
			yenumcmbCustomerNotificationTargetType.Binding
				.AddBinding(ViewModel.Entity, e => e.PushTarget, w => w.SelectedItem)
				.InitializeFromSource();

			ycheckNotificationDisabled.Binding
				.AddBinding(ViewModel.Entity, e => e.NotificationDisabled, w => w.Active)
				.InitializeFromSource();

			ycheckAllowDuplicates.Binding
				.AddBinding(ViewModel.Entity, e => e.AllowDuplicateNotifications, w => w.Active)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);
		}
	}
}
