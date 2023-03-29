using QS.Views;
using Vodovoz.ViewModels.ViewModels.Settings;

namespace Vodovoz.Views.Settings
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeneralSettingsView : ViewBase<GeneralSettingsViewModel>
	{
		public GeneralSettingsView(GeneralSettingsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSaveRouteListPrintedPhones.Clicked += (sender, args) => ViewModel.SaveRouteListPrintedFormPhonesCommand.Execute();
			btnSaveRouteListPrintedPhones.Binding.AddBinding(ViewModel, vm => vm.CanEditRouteListPrintedFormPhones, w => w.Sensitive)
				.InitializeFromSource();

			textviewRouteListPrintedFormPhones.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RouteListPrintedFormPhones, w => w.Buffer.Text)
				.AddBinding(vm => vm.CanEditRouteListPrintedFormPhones, w => w.Sensitive)
				.InitializeFromSource();

			btnSaveCanAddForwardersToLargus.Clicked += (sender, args) => ViewModel.SaveCanAddForwardersToLargusCommand.Execute();
			btnSaveCanAddForwardersToLargus.Binding.AddBinding(ViewModel, vm => vm.CanEditCanAddForwardersToLargus, w => w.Sensitive)
				.InitializeFromSource();

			ycheckCanAddForwardersToLargus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanAddForwardersToLargus, w => w.Active)
				.AddBinding(vm => vm.CanEditCanAddForwardersToLargus, w => w.Sensitive)
				.InitializeFromSource();

			roboatssettingsview1.ViewModel = ViewModel.RoboatsSettingsViewModel;

			btnSaveOrderAutoComment.Clicked += (sender, args) => ViewModel.SaveOrderAutoCommentCommand.Execute();
			btnSaveOrderAutoComment.Binding.AddBinding(ViewModel, vm => vm.CanEditOrderAutoComment, w => w.Sensitive).InitializeFromSource();

			btnOrderAutoCommentInfo.Clicked += (sender, args) => ViewModel.ShowAutoCommentInfoCommand.Execute();

			entryOrderAutoComment.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderAutoComment, w => w.Text)
				.AddBinding(vm => vm.CanEditOrderAutoComment, w => w.IsEditable)
				.InitializeFromSource();

			ybtnComplaintWithoutDriverSubdivisionsAdd.Clicked += OnYbtnComplaintWithoutDriverSubdivisionsAddClicked;
			ybtnComplaintWithoutDriverSubdivisionsDelete.Clicked += OnYbtnComplaintWithoutDriverSubdivisionsDeleteClicked;
			ybtnComplaintWithoutDriverSubdivisionsSave.Clicked += OnYbtnComplaintWithoutDriverSubdivisionsSaveClicked;

			ybtnComplaintWithoutDriverSubdivisionsAdd.Sensitive = ViewModel.CanEditComplaintWithoutDriverSubdivisions;
			ybtnComplaintWithoutDriverSubdivisionsSave.Sensitive = ViewModel.CanEditComplaintWithoutDriverSubdivisions;

			ytreeComplaintWithoutDriverSubdivisions.CreateFluentColumnsConfig<Subdivision>()
				.AddColumn("Номер").AddNumericRenderer(x => x.Id)
				.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();

			ytreeComplaintWithoutDriverSubdivisions.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEditComplaintWithoutDriverSubdivisions, w => w.Sensitive)
				.AddBinding(vm => vm.ObservableSubdivisions, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedSubdivision, w => w.SelectedRow)
				.InitializeFromSource();

			ybtnComplaintWithoutDriverSubdivisionsDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveSubdivision, w => w.Sensitive)
				.InitializeFromSource();
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
