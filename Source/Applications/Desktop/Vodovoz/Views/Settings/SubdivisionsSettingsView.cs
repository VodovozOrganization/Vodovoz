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

			ybtnSubdivisionsAdd.Clicked += OnYbtnSubdivisionsAddClicked;
			ybtnSubdivisionsDelete.Clicked += OnYbtnSubdivisionsDeleteClicked;
			ybtnSubdivisionsSave.Clicked += OnYbtnSubdivisionsSaveClicked;
			ybtnSubdivisionsInfo.Clicked += OnYbtnSubdivisionsInfoClicked;

			ybtnSubdivisionsAdd.Sensitive = ViewModel.CanEdit;
			ybtnSubdivisionsSave.Sensitive = ViewModel.CanEdit;

			ytreeSubdivisions.CreateFluentColumnsConfig<Subdivision>()
				.AddColumn("Номер").AddNumericRenderer(x => x.Id)
				.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();

			ytreeSubdivisions.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.ObservableSubdivisions, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedSubdivision, w => w.SelectedRow)
				.InitializeFromSource();

			ybtnSubdivisionsDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanRemove, w => w.Sensitive)
				.InitializeFromSource();

			ylabelTitle.Binding.AddBinding(ViewModel, vm => vm.DetailTitle, w => w.LabelProp).InitializeFromSource();

			((Label)frameConfiguration.LabelWidget).LabelProp = ViewModel.MainTitle;
		}

		private void OnYbtnSubdivisionsInfoClicked(object sender, System.EventArgs e)
		{
			ViewModel.ShowSubdivisionsInfoCommand?.Execute();
		}

		private void OnYbtnSubdivisionsSaveClicked(object sender, System.EventArgs e)
		{
			ViewModel.SaveSubdivisionsCommand?.Execute();
		}

		private void OnYbtnSubdivisionsDeleteClicked(object sender, System.EventArgs e)
		{
			ViewModel.RemoveSubdivisionCommand?.Execute();
		}

		private void OnYbtnSubdivisionsAddClicked(object sender, System.EventArgs e)
		{
			ViewModel.AddSubdivisionCommand?.Execute();
		}

		public override void Dispose()
		{
			ybtnSubdivisionsAdd.Clicked -= OnYbtnSubdivisionsAddClicked;
			ybtnSubdivisionsDelete.Clicked -= OnYbtnSubdivisionsDeleteClicked;
			ybtnSubdivisionsSave.Clicked -= OnYbtnSubdivisionsSaveClicked;
			base.Dispose();
		}
	}
}
