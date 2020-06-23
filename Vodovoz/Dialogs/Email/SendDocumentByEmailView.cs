using System;
using EmailService;
using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Dialogs.Email
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendDocumentByEmailView : WidgetViewBase<SendDocumentByEmailViewModel>
	{
		public SendDocumentByEmailView(SendDocumentByEmailViewModel viewModel) : base(viewModel)
		{
			this.Build();

			Configure();
		}

		private void Configure()
		{
			buttonSendEmail.Clicked += (sender, e) => ViewModel.SendEmailCommand.Execute();
			buttonRefreshEmailList.Clicked += (sender, e) => ViewModel.RefreshEmailListCommand.Execute();

			yvalidatedentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;
			yvalidatedentryEmail.Binding.AddBinding(ViewModel, vm => vm.EmailString, w => w.Text).InitializeFromSource();
			ylabelDescription.Binding.AddBinding(ViewModel, vm => vm.Description, w => w.Text).InitializeFromSource();

			ytreeviewStoredEmails.ColumnsConfig = ColumnsConfigFactory.Create<StoredEmail>()
				.AddColumn("Дата").AddTextRenderer(x => x.SendDate.ToString("dd.MM.yyyy HH:mm"))
				.AddColumn("Почта").AddTextRenderer(x => x.RecipientAddress)
				.AddColumn("Статус").AddEnumRenderer(x => x.State)
				.RowCells()
				.Finish();

			ytreeviewStoredEmails.ItemsDataSource = ViewModel.StoredEmails;
			ytreeviewStoredEmails.Selection.Changed += OnYtreeviewStoredEmailsSelectionChanged;

			//Sensitive = EmailServiceSetting.SendingAllowed;
		}

		void OnYtreeviewStoredEmailsSelectionChanged(object sender, EventArgs e)
		{
			ViewModel.SelectedObj = ytreeviewStoredEmails.GetSelectedObject();

			if(ViewModel.SelectedObj == null)
				return;

			ViewModel.Description = (ViewModel.SelectedObj as StoredEmail).Description;
		}
	}
}
