using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Dialogs.Roboats;

namespace Vodovoz.Views.Roboats
{
	public partial class RoboatsWaterTypeView : EntityTabViewBase<RoboatsWaterTypeViewModel, RoboatsWaterType>
	{
		private RoboatsEntityView _roboatsEntityView;

		public RoboatsWaterTypeView(RoboatsWaterTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			labelIdValue.Binding.AddFuncBinding(ViewModel.Entity, e => e.Id.ToString(), w => w.LabelProp).InitializeFromSource();
			//entryNomenclature.SetEntityAutocompleteSelectorFactory(ViewModel.NomenclatureSelectorFactory);
			//entryNomenclature.Binding
			//	.AddBinding(ViewModel.Entity, e => e.Nomenclature, w => w.Subject)
			//	.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive)
			//	.InitializeFromSource();
			entryNomenclature.CanOpenWithoutTabParent = true;

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
