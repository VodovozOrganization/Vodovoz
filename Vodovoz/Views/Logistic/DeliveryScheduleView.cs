using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.Views.Organization;

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
			entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryFrom.Binding.AddBinding(ViewModel.Entity, e => e.From, w => w.Time).InitializeFromSource();
			entryTo.Binding.AddBinding(ViewModel.Entity, e => e.To, w => w.Time).InitializeFromSource();
            ycheckIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

			buttonSave.Clicked += (s, e) => ViewModel.Save(true);
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
				_roboatsEntityView.Show();
			}
		}
	}
}
