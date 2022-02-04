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
			yentrySubject.Binding
				.AddBinding(_bulkEmailViewModel, vm => vm.MailSubject, w => w.Text)
				.InitializeFromSource();

			ylabelSubjectInfo.Binding
				.AddFuncBinding(_bulkEmailViewModel, vm => vm.MailSubjectInfoDanger
						? $"<span foreground='red'>{vm.MailSubjectInfo}</span>"
						: $"<span foreground='green'>{vm.MailSubjectInfo}</span>", 
					w => w.LabelProp)
				.InitializeFromSource();

			ytextviewText.Binding
				.AddBinding(_bulkEmailViewModel, vm => vm.MailTextPart, w => w.Buffer.Text)
				.InitializeFromSource();

			ylabelAttachmentsInfo.Binding
				.AddFuncBinding(_bulkEmailViewModel, vm => vm.AttachmentsSizeInfoDanger
						? $"<span foreground='red'>{vm.AttachmentsSizeInfo}</span>"
						: $"<span foreground='green'>{vm.AttachmentsSizeInfo}</span>", 
					w => w.LabelProp)
				.InitializeFromSource();

			ylabelRecepientInfo.Binding.
				AddFuncBinding(_bulkEmailViewModel,
					vm => vm.RecepientInfoDanger
					? $"<span foreground='red'>{vm.RecepientInfo}</span>"
					: $"<span foreground='green'>{vm.RecepientInfo}</span>", 
					w => w.LabelProp)
				.InitializeFromSource();

			ylabelSendingDuration.Binding
				.AddBinding(_bulkEmailViewModel, vm => vm.SendingDurationInfo, w => w.LabelProp)
				.InitializeFromSource();

			yprogressbarSending.Binding
				.AddSource(_bulkEmailViewModel)
				.AddBinding(vm => vm.IsInSendingProcess, w => w.Visible)
				.InitializeFromSource();


			attachmentsEmailView.ViewModel = _bulkEmailViewModel.AttachmentsEmailViewModel;

			buttonSend.Clicked += (sender, args) => _bulkEmailViewModel.StartEmailSendingCommand.Execute();

			_bulkEmailViewModel.PropertyChanged += _bulkEmailViewModel_PropertyChanged;
		}

		private void _bulkEmailViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(_bulkEmailViewModel.SendingProgressUpper):
				{ 
					yprogressbarSending.Adjustment.Upper = _bulkEmailViewModel.SendingProgressUpper;
					GtkHelper.WaitRedraw();
					break;
				}
				case nameof(_bulkEmailViewModel.SendingProgressValue):
				{
					yprogressbarSending.Adjustment.Value = _bulkEmailViewModel.SendingProgressValue;
					GtkHelper.WaitRedraw();
					break;
				}
				case nameof(_bulkEmailViewModel.SendedCountInfo): 
				{
					yprogressbarSending.Text = _bulkEmailViewModel.SendedCountInfo;
					GtkHelper.WaitRedraw();
					break;
				}
				case nameof(_bulkEmailViewModel.CanExecute):
				{
					buttonSend.Sensitive = _bulkEmailViewModel.CanExecute;
					break;
				}
			}
		}
	}
}
