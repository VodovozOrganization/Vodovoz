using System;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ModelCarView : TabViewBase<ModelCarViewModel>
	{
		public ModelCarView(ModelCarViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		private void ConfigureView()
		{
			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			
			//entitymanufacturedCars.SetEntityAutocompleteSelectorFactory(
				//ViewModel.EmployeePostsJournalFactory.CreateEmployeePostsAutocompleteSelectorFactory());
			//entitymanufacturedCars.Binding.AddBinding(ViewModel.Entity, e => e.ManufacturerCars, w => w.Subject).InitializeFromSource();
		}
	}
}
