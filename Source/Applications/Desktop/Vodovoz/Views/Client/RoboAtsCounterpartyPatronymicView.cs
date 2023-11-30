using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.Views.Roboats;

namespace Vodovoz.Views.Client
{
	public partial class RoboAtsCounterpartyPatronymicView : TabViewBase<RoboAtsCounterpartyPatronymicViewModel>
	{
		private RoboatsEntityView _roboatsEntityView;

		public RoboAtsCounterpartyPatronymicView(RoboAtsCounterpartyPatronymicViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			labelIdValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.Id.ToString(), w => w.LabelProp).InitializeFromSource();
			yentryPatronymic.Binding
				.AddBinding(ViewModel.Entity, e => e.Patronymic, w => w.Text)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yentryAccent.Binding
				.AddBinding(ViewModel.Entity, e => e.Accent, w => w.Text)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.Save(true);
			buttonSave.Sensitive = ViewModel.CanEdit;
			buttonCancel.Clicked += (sender, args) => ViewModel.Cancel();

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
