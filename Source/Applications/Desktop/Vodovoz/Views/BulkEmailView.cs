using QS.Utilities;
using System;
using Gdk;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Extensions;

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

			var successTextColor = GdkColors.SuccessText.ToHtmlColor();
			var dangerTextColor = GdkColors.DangerText.ToHtmlColor();

			ylabelSubjectInfo.Binding
				.AddFuncBinding(_bulkEmailViewModel, vm => vm.MailSubjectInfoDanger
						? $"<span foreground='{dangerTextColor}'>{vm.MailSubjectInfo}</span>"
						: $"<span foreground='{successTextColor}'>{vm.MailSubjectInfo}</span>", 
					w => w.LabelProp)
				.InitializeFromSource();

			ytextviewText.Binding
				.AddBinding(_bulkEmailViewModel, vm => vm.MailTextPart, w => w.Buffer.Text)
				.InitializeFromSource();

			ylabelAttachmentsInfo.Binding
				.AddFuncBinding(_bulkEmailViewModel, vm => vm.AttachmentsSizeInfoDanger
						? $"<span foreground='{dangerTextColor}'>{vm.AttachmentsSizeInfo}</span>"
						: $"<span foreground='{successTextColor}'>{vm.AttachmentsSizeInfo}</span>", 
					w => w.LabelProp)
				.InitializeFromSource();

			ylabelRecepientInfo.Binding.
				AddFuncBinding(_bulkEmailViewModel,
					vm => vm.RecepientInfoDanger
					? $"<span foreground='{dangerTextColor}'>{vm.RecepientInfo}</span>"
					: $"<span foreground='{successTextColor}'>{vm.RecepientInfo}</span>", 
					w => w.LabelProp)
				.InitializeFromSource();

			ylabelSendingDuration.Binding
				.AddBinding(_bulkEmailViewModel, vm => vm.SendingDurationInfo, w => w.LabelProp)
				.InitializeFromSource();

			yprogressbarSending.Binding
				.AddSource(_bulkEmailViewModel)
				.AddBinding(vm => vm.IsInSendingProcess, w => w.Visible)
				.InitializeFromSource();

			ycheckbuttonLongUnsubscribed.Binding.AddBinding(_bulkEmailViewModel, vm => vm.IncludeOldUnsubscribed, w => w.Active).InitializeFromSource();

			yspinbuttonMonthsSinceUnsubscribing.Binding.AddSource(_bulkEmailViewModel)
				.AddBinding(vm => vm.IncludeOldUnsubscribed, w => w.Sensitive)
				.AddBinding(vm => vm.MonthsSinceUnsubscribing, w => w.ValueAsInt)
				.InitializeFromSource();

			attachedfileinformationsview.InitializeViewModel(_bulkEmailViewModel.AttachedFileInformationsViewModel);

			buttonSend.Clicked += (sender, args) => _bulkEmailViewModel.StartEmailSendingCommand.Execute();

			_bulkEmailViewModel.PropertyChanged += BulkEmailViewModel_PropertyChanged;
		}

		private void BulkEmailViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(_bulkEmailViewModel.SendingProgressUpper):
				{
					if(yprogressbarSending.Adjustment != null)
					{
						yprogressbarSending.Adjustment.Upper = _bulkEmailViewModel.SendingProgressUpper;
						GtkHelper.WaitRedraw();
					}

					break;
				}
				case nameof(_bulkEmailViewModel.SendingProgressValue):
				{
					if(yprogressbarSending.Adjustment != null)
					{
						yprogressbarSending.Adjustment.Value = _bulkEmailViewModel.SendingProgressValue;
						GtkHelper.WaitRedraw();
					}
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

		protected override bool OnDeleteEvent(Event evnt)
		{
			_bulkEmailViewModel.Stop();
			return base.OnDeleteEvent(evnt);
		}
	}
}
