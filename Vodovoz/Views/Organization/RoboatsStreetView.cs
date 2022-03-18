using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Dialogs.Organizations;

namespace Vodovoz.Views.Organization
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RoboatsStreetView : EntityTabViewBase<RoboatsStreetViewModel, RoboatsStreet>
	{
		private RoboatsEntityView _roboatsEntityView;

		public RoboatsStreetView(RoboatsStreetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			labelIdValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.Id.ToString(), w => w.LabelProp).InitializeFromSource();

			yentryStreetType.Binding
				.AddBinding(ViewModel.Entity, e => e.Type, w => w.Text)
				.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yentryStreet.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
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
