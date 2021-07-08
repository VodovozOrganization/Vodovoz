using System;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	public partial class ComplaintObjectView : TabViewBase<ComplaintObjectViewModel>
	{
		public ComplaintObjectView(ComplaintObjectViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ylabelCreateDate.Binding
				.AddFuncBinding(ViewModel.Entity,e => e.CreateDate.ToString("g"), w => w.LabelProp)
				.InitializeFromSource();

			ylabelArchiveDateName.Binding.AddBinding(ViewModel, vm=> vm.ArchiveDateVisible, w=>w.Visible).InitializeFromSource();

			ylabelArchiveDate.Binding
				.AddFuncBinding(ViewModel.Entity,e => e.ArchiveDate.HasValue ? e.ArchiveDate.Value.ToString("g") : null, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.ArchiveDateVisible, w => w.Visible)
				.InitializeFromSource();

			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			ycheckbuttonArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
