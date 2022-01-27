using System;
using Vodovoz.ViewModels.ViewModels;

namespace Vodovoz.Views
{
	public partial class BulkEmailView : Gtk.Dialog
	{
		private readonly BulkEmailViewModel _bulkEmailViewModel;
		public BulkEmailView(BulkEmailViewModel bulkEmailViewModel)
		{
			_bulkEmailViewModel = bulkEmailViewModel ?? throw new ArgumentNullException(nameof(bulkEmailViewModel));
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentrySubject.Binding.AddBinding(_bulkEmailViewModel, vm => vm.SubjectText, w => w.Text).InitializeFromSource();

			ylabelSubjectInfo.Binding.AddSource(_bulkEmailViewModel)
				.AddBinding( vm => vm.SubjectInfo, w => w.LabelProp)
				.AddFuncBinding(vm => vm.SubjectText != null && vm.SubjectText.Length <= 55
						? $"<span foreground='green'>{ vm.SubjectInfo }</span>"
						: $"<span foreground='red'>{ vm.SubjectInfo }</span>",
					w => w.LabelProp)
				.InitializeFromSource();

		}
	}
}
