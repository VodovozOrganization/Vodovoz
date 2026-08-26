using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.StoredEmails;
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
			buttonSendEmail.BindCommand(ViewModel.SendEmailCommand);
			ViewModel.SendEmailCommand.CanExecuteChanged += OnSendEmailCanExecuteChanged;
			buttonRefreshEmailList.BindCommand(ViewModel.RefreshEmailListCommand);
			ViewModel.RefreshEmailListCommand.CanExecuteChanged += OnRefreshEmailListCanExecuteChanged;

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

		private void OnRefreshEmailListCanExecuteChanged(object sender, EventArgs args)
		{
			buttonRefreshEmailList.Sensitive = ViewModel.RefreshEmailListCommand.CanExecute();
		}

		private void OnSendEmailCanExecuteChanged(object sender, EventArgs args)
		{
			buttonSendEmail.Sensitive = ViewModel.SendEmailCommand.CanExecute();
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

		protected override void OnDestroyed()
		{
			yvalidatedentryEmail.Changed -= OnEmailChanged;
			ViewModel.SendEmailCommand.CanExecuteChanged -= OnSendEmailCanExecuteChanged;
			ViewModel.RefreshEmailListCommand.CanExecuteChanged -= OnRefreshEmailListCanExecuteChanged;
			base.OnDestroyed();
		}
	}
}
