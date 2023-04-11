using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Settings;

namespace Vodovoz.Views.Settings
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubdivisionsSettingsView : WidgetViewBase<SubdivisionSettingsViewModel>
	{
		public SubdivisionsSettingsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ybtnComplaintWithoutDriverSubdivisionsAdd.Clicked += OnYbtnComplaintWithoutDriverSubdivisionsAddClicked;
			ybtnComplaintWithoutDriverSubdivisionsDelete.Clicked += OnYbtnComplaintWithoutDriverSubdivisionsDeleteClicked;
			ybtnComplaintWithoutDriverSubdivisionsSave.Clicked += OnYbtnComplaintWithoutDriverSubdivisionsSaveClicked;
			ybtnComplaintWithoutDriverSubdivisionsInfo.Clicked += OnYbtnComplaintWithoutDriverSubdivisionsInfoClicked;

			ybtnComplaintWithoutDriverSubdivisionsAdd.Sensitive = ViewModel.CanEdit;
			ybtnComplaintWithoutDriverSubdivisionsSave.Sensitive = ViewModel.CanEdit;

			ytreeComplaintWithoutDriverSubdivisions.CreateFluentColumnsConfig<Subdivision>()
				.AddColumn("Номер").AddNumericRenderer(x => x.Id)
				.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();

			ytreeComplaintWithoutDriverSubdivisions.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.ObservableSubdivisions, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedSubdivision, w => w.SelectedRow)
				.InitializeFromSource();

			ybtnComplaintWithoutDriverSubdivisionsDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanRemove, w => w.Sensitive)
				.InitializeFromSource();

			ylabelTitle.Binding.AddBinding(ViewModel, vm => vm.DetailTitle, w => w.LabelProp).InitializeFromSource();

			((Label)frameConfiguration.LabelWidget).LabelProp = ViewModel.MainTitle;
		}

		private void OnYbtnComplaintWithoutDriverSubdivisionsInfoClicked(object sender, System.EventArgs e)
		{
			ViewModel.ShowSubdivisionsToInformComplaintHasNoDriverInfoCommand?.Execute();
		}

		private void OnYbtnComplaintWithoutDriverSubdivisionsSaveClicked(object sender, System.EventArgs e)
		{
			ViewModel.SaveSubdivisionsCommand?.Execute();
		}

		private void OnYbtnComplaintWithoutDriverSubdivisionsDeleteClicked(object sender, System.EventArgs e)
		{
			ViewModel.RemoveSubdivisionCommand?.Execute();
		}

		private void OnYbtnComplaintWithoutDriverSubdivisionsAddClicked(object sender, System.EventArgs e)
		{
			ViewModel.AddSubdivisionCommand?.Execute();
		}

		public override void Dispose()
		{
			ybtnComplaintWithoutDriverSubdivisionsAdd.Clicked -= OnYbtnComplaintWithoutDriverSubdivisionsAddClicked;
			ybtnComplaintWithoutDriverSubdivisionsDelete.Clicked -= OnYbtnComplaintWithoutDriverSubdivisionsDeleteClicked;
			ybtnComplaintWithoutDriverSubdivisionsSave.Clicked -= OnYbtnComplaintWithoutDriverSubdivisionsSaveClicked;
			base.Dispose();
		}
	}
}
