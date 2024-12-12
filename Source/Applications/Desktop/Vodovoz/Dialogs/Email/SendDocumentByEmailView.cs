using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.ViewModels.Dialogs.Email;

namespace Vodovoz.Dialogs.Email
{
	[ToolboxItem(true)]
	public partial class SendDocumentByEmailView : WidgetViewBase<SendDocumentByEmailViewModel>
	{
		public SendDocumentByEmailView(SendDocumentByEmailViewModel viewModel) : base(viewModel)
		{
			Build();
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
			yvalidatedentryEmail.Changed += OnEmailChanged;
			
			ylabelDescription.Binding.AddBinding(ViewModel, vm => vm.Description, w => w.Text).InitializeFromSource();

			ytreeviewStoredEmails.ColumnsConfig = ColumnsConfigFactory.Create<StoredEmail>()
				.AddColumn("Дата").AddTextRenderer(x => x.SendDate.ToString("dd.MM.yyyy HH:mm"))
				.AddColumn("Почта").AddTextRenderer(x => x.RecipientAddress)
				.AddColumn("Статус").AddEnumRenderer(x => x.State)
				.RowCells()
				.Finish();

			ytreeviewStoredEmails.ItemsDataSource = ViewModel.StoredEmails;
			ytreeviewStoredEmails.Binding
				.AddBinding(ViewModel, vm => vm.SelectedStoredEmail, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void OnEmailChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(yvalidatedentryEmail.Text))
			{
				var regex = new Regex(Vodovoz.Domain.Contacts.Email.EmailRegEx);

				yvalidatedentryEmail.Text = yvalidatedentryEmail.Text.Replace(" ", "").Replace("\n", "");
				
				if(regex.IsMatch(yvalidatedentryEmail.Text))
				{
					ViewModel.UpdateEmails();
				}
				else
				{
					ViewModel.BtnSendEmailSensitive = false;
				}
			}
			else
			{
				ViewModel.BtnSendEmailSensitive = false;
			}
		}

		public override void Destroy()
		{
			yvalidatedentryEmail.Changed -= OnEmailChanged;
			base.Destroy();
		}
	}
}
