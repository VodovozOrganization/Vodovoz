using QS.Utilities;
using System;
using Gtk;
using Vodovoz.ViewModels.ViewModels;

namespace Vodovoz.Views
{
	public partial class BulkEmailView : Gtk.Dialog
	{
		private readonly BulkEmailViewModel _bulkEmailViewModel;

		public BulkEmailView(BulkEmailViewModel bulkEmailViewModel)
		{
			this.Build();
			_bulkEmailViewModel = bulkEmailViewModel ?? throw new ArgumentNullException(nameof(bulkEmailViewModel));
			Configure();
		}

		private void Configure()
		{
			yentrySubject.Binding.AddBinding(_bulkEmailViewModel, vm => vm.MailSubject, w => w.Text).InitializeFromSource();

			ylabelSubjectInfo.Binding.AddSource(_bulkEmailViewModel)
				//.AddBinding(vm => vm.MailSubjectInfo, w => w.LabelProp)
				.AddFuncBinding(vm => vm.MailSubject != null && vm.MailSubject.Length <= 55 && vm.MailSubject.Length > 0
						? $"<span foreground='green'>{vm.MailSubjectInfo}</span>"
						: $"<span foreground='red'>{vm.MailSubjectInfo}</span>",
					w => w.LabelProp)
				.InitializeFromSource();

			ytextviewText.Binding.AddBinding(_bulkEmailViewModel, vm => vm.MailTextPart, w => w.Buffer.Text).InitializeFromSource();


			attachmentsEmailView.ViewModel = _bulkEmailViewModel.AttachmentsEmailViewModel;

			buttonSend.Clicked += (sender, args) => _bulkEmailViewModel.StartEmailSendingCommand.Execute();

			yprogressbarSending.Binding
				.AddSource(_bulkEmailViewModel)
				.AddBinding(vm => vm.IsInSendingProcess, w => w.Visible)
				//.AddBinding(vm => vm.SendingProgressUpper, w => w.Adjustment.Upper)
				.InitializeFromSource();

			_bulkEmailViewModel.SendingProgressBarUpdated += (sender, args) =>
			{
				yprogressbarSending.Adjustment.Value = _bulkEmailViewModel.SendingProgressValue;
				yprogressbarSending.Adjustment.Upper = _bulkEmailViewModel.SendingProgressUpper;
				yprogressbarSending.Text = _bulkEmailViewModel.SendedCountInfo;
				ytextviewText.Buffer.Text = _bulkEmailViewModel.SendingProgressValue.ToString();
				GtkHelper.WaitRedraw();
			};
		}
	}
}
