using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	public partial class ComplaintKindView : TabViewBase<ComplaintKindViewModel>
	{
		public ComplaintKindView(ComplaintKindViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			yspeccomboboxComplaintObject.ShowSpecialStateNot = true;
			yspeccomboboxComplaintObject.Binding
				.AddBinding(ViewModel, vm => vm.ComplaintObjects, w => w.ItemsList)
				.InitializeFromSource();
			yspeccomboboxComplaintObject.Binding
				.AddBinding(ViewModel.Entity, e => e.ComplaintObject, w => w.SelectedItem)
				.InitializeFromSource();

			chkIsArchive.Binding.AddBinding(ViewModel.Entity, vm => vm.IsArchive, w => w.Active)
				.InitializeFromSource();

			ybuttonAttachSubdivision.Clicked += (sender, e) => ViewModel.AttachSubdivisionCommand.Execute();
			ybuttonRemoveSubdivision.Clicked += (sender, e) => ViewModel.RemoveSubdivisionCommand.Execute(ytreeviewSubdivisions.GetSelectedObject<Subdivision>());

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			ytreeviewSubdivisions.ColumnsConfig = FluentColumnsConfig<Subdivision>.Create()
				.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeviewSubdivisions.ItemsDataSource = ViewModel.Entity.ObservableSubdivisions;
			ytreeviewSubdivisions.HeadersVisible = false;
		}
	}
}
