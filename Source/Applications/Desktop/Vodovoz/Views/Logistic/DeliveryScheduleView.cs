using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.Views.Roboats;

namespace Vodovoz.Views.Logistic
{
	public partial class DeliveryScheduleView : TabViewBase<DeliveryScheduleViewModel>
	{
		private RoboatsEntityView _roboatsEntityView;

		public DeliveryScheduleView(DeliveryScheduleViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg ()
		{
			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryFrom.Binding
				.AddBinding(ViewModel.Entity, e => e.From, w => w.Time)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryTo.Binding
				.AddBinding(ViewModel.Entity, e => e.To, w => w.Time)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ycheckIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += (s, e) => ViewModel.Save(true);
			buttonSave.Sensitive = ViewModel.CanEdit;
			buttonCancel.Clicked += (s, e) => ViewModel.Cancel();

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			UpdateRoboatsEntityView();
		}

		private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.RoboatsEntityViewModel):
					UpdateRoboatsEntityView();
					break;
				default:
					break;
			}
		}

		private void UpdateRoboatsEntityView()
		{
			_roboatsEntityView?.Destroy();
			if(ViewModel.RoboatsEntityViewModel != null)
			{
				_roboatsEntityView = new RoboatsEntityView(ViewModel.RoboatsEntityViewModel);
				boxRoboatsHolder.Add(_roboatsEntityView);
				_roboatsEntityView.Sensitive = ViewModel.CanEdit;
				_roboatsEntityView.Show();
			}
		}
	}
}
