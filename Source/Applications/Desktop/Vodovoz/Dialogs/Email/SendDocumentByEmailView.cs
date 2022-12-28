using System;
using System.Text.RegularExpressions;
using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.ViewModels.Dialogs.Email;

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
			ViewModel.SendEmailCommand.CanExecuteChanged += (sender, args) =>
				buttonSendEmail.Sensitive = ViewModel.SendEmailCommand.CanExecute();
			buttonRefreshEmailList.Clicked += (sender, e) => ViewModel.RefreshEmailListCommand.Execute();
			ViewModel.RefreshEmailListCommand.CanExecuteChanged += (sender, args) =>
				buttonRefreshEmailList.Sensitive = ViewModel.RefreshEmailListCommand.CanExecute();

			buttonSendEmail.Binding.AddBinding(ViewModel, vm => vm.BtnSendEmailSensitive, w => w.Sensitive).InitializeFromSource();
			
			yvalidatedentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;
			yvalidatedentryEmail.Binding.AddBinding(ViewModel, vm => vm.EmailString, w => w.Text).InitializeFromSource();
			yvalidatedentryEmail.Changed += YvalidatedentryEmailOnChanged;
			
			ylabelDescription.Binding.AddBinding(ViewModel, vm => vm.Description, w => w.Text).InitializeFromSource();

			ytreeviewStoredEmails.ColumnsConfig = ColumnsConfigFactory.Create<StoredEmail>()
				.AddColumn("Дата").AddTextRenderer(x => x.SendDate.ToString("dd.MM.yyyy HH:mm"))
				.AddColumn("Почта").AddTextRenderer(x => x.RecipientAddress)
				.AddColumn("Статус").AddEnumRenderer(x => x.State)
				.RowCells()
				.Finish();

			ytreeviewStoredEmails.ItemsDataSource = ViewModel.StoredEmails;
			ytreeviewStoredEmails.Selection.Changed += OnYtreeviewStoredEmailsSelectionChanged;
		}

		private void YvalidatedentryEmailOnChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(yvalidatedentryEmail.Text))
			{
				var regex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@" +
				                      @"[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]\.[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]$");

				yvalidatedentryEmail.Text = yvalidatedentryEmail.Text.Replace(" ", "").Replace("\n", "");
				
				if(regex.IsMatch(yvalidatedentryEmail.Text))
					ViewModel.UpdateEmails();
				else
					ViewModel.BtnSendEmailSensitive = false;
			}
			else
				ViewModel.BtnSendEmailSensitive = false;
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
