using System;
using Gtk;
using QS.ErrorReporting;
using Vodovoz.Infrastructure;
using Vodovoz.Tools;

namespace Vodovoz.Views
{
	public partial class ErrorMessageView : Gtk.Dialog
	{
		public ErrorMessageView(ErrorMessageViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			this.Build();
			this.SetPosition(WindowPosition.CenterOnParent);
			ActionArea.ChildVisible = false;
			ActionArea.SetSizeRequest(0, 0);
			ConfigureDlg();
		}

		ErrorMessageViewModel ViewModel;

		void ConfigureDlg()
		{
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			entryEmail.Binding.AddBinding(ViewModel, vm => vm.Email, w => w.Text).InitializeFromSource();

			textviewError.Binding.AddFuncBinding(ViewModel, vm => (vm as ErrorMessageViewModel).ExceptionText, w => w.Buffer.Text).InitializeFromSource();
			textviewDescription.Binding.AddBinding(ViewModel, vm => vm.Description, w => w.Buffer.Text).InitializeFromSource();

			ybuttonSendReport.Clicked += YbuttonSendReport_Clicked;
			ybuttonSendReport.Binding.AddBinding(ViewModel, vm => vm.CanSendErrorReportManually, w => w.Sensitive).InitializeFromSource();
			ybuttonSendReport.Binding.AddFuncBinding(ViewModel, vm => (vm as ErrorMessageViewModel).CanSendManuallyText, w => w.TooltipText).InitializeFromSource();

			ybuttonCopy.Clicked += YbuttonCopy_Clicked;
			ybuttonOK.Clicked += (sender, e) => this.Destroy();
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.IsEmailValid)) {
				if(ViewModel.IsEmailValid)
					entryEmail.ModifyText(StateType.Normal);
				else
					entryEmail.ModifyText(StateType.Normal, GdkColors.DangerBase);
			}
		}

		void YbuttonSendReport_Clicked(object sender, EventArgs e)
		{
			ViewModel.SendReportCommand.Execute(ReportType.User); 
			this.Destroy();
		}

		void YbuttonCopy_Clicked(object sender, EventArgs e)
		{
			Clipboard clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
			clipboard.Text = ViewModel.ErrorData;
			clipboard.Store();
		}

		protected override void OnDestroyed()
		{
			ViewModel.SendReportAutomatically();
		}
	}
}
