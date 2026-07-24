using EdoNotifications.Contracts;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Edo;

namespace Vodovoz.Views.Edo
{
	public partial class EdoNotificationSettingView : TabViewBase<EdoNotificationSettingViewModel>
	{
		public EdoNotificationSettingView(EdoNotificationSettingViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yenumcmbNotificationType.ItemsEnum = typeof(EdoNotificationType);
			yenumcmbNotificationType.Binding
				.AddBinding(ViewModel.Entity, e => e.EdoNotificationType, w => w.SelectedItem)
				.InitializeFromSource();

			yentryEmails.Binding
				.AddBinding(ViewModel.Entity, e => e.Emails, w => w.Text)
				.InitializeFromSource();

			yentryBitrixDialogs.Binding
				.AddBinding(ViewModel.Entity, e => e.BitrixDialogs, w => w.Text)
				.InitializeFromSource();

			ytextviewTemplate.Binding
				.AddBinding(ViewModel.Entity, e => e.Template, w => w.Buffer.Text)
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
