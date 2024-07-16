using System.Linq;
using System;
using System.Text;
using Gamma.Utilities;
using Gtk;
using QS.ViewModels;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WageParameterView : TabViewBase<EmployeeWageParameterViewModel>
	{
		private Notebook _notebook;

		public WageParameterView(EmployeeWageParameterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			comboWageType.ItemsEnum = typeof(WageParameterItemTypes);
			var itemsToHide = ViewModel.GetWageParameterItemTypesToHide();

			if(itemsToHide.Any())
			{
				foreach(var itemToHide in itemsToHide)
				{
					comboWageType.AddEnumToHideList(itemToHide);
				}
			}

			comboWageType.Binding.AddBinding(ViewModel, vm => vm.WageParameterItemType, w => w.SelectedItem).InitializeFromSource();
			comboWageType.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += (sender, e) =>
			{
				if(e.PropertyName == nameof(ViewModel.WageParameterItemViewModel))
				{
					UpdateWageParameterView();
				}
				if(e.PropertyName == nameof(ViewModel.DriverWithCompanyCarWageParameterItemViewModel))
				{
					UpdateWageParameterView();
				}
				if(e.PropertyName == nameof(ViewModel.RaskatCarWageParameterItemViewModel))
				{
					UpdateWageParameterView();
				}
			};

			UpdateWageParameterView();

			buttonSave.Clicked += (sender, e) => ViewModel.Save();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		private void UpdateWageParameterView()
		{
			_notebook?.Destroy();
			_notebook = new Notebook();

			var defaultRatesWidget = GetWidget(ViewModel.WageParameterItemViewModel);
			if(defaultRatesWidget != null)
			{
				_notebook.InsertPage(defaultRatesWidget, GetNotebookHeaderLabel(ViewModel.WageParameterItemType, CarOwnType.Driver), 0);
			}

			var compayCarWidget = GetWidget(ViewModel.DriverWithCompanyCarWageParameterItemViewModel);
			if(compayCarWidget != null)
			{
				_notebook.InsertPage(compayCarWidget, GetNotebookHeaderLabel(ViewModel.WageParameterItemType, CarOwnType.Company), 1);
			}
			var raskatCarWidget = GetWidget(ViewModel.RaskatCarWageParameterItemViewModel);
			if(raskatCarWidget != null)
			{
				_notebook.InsertPage(raskatCarWidget, GetNotebookHeaderLabel(ViewModel.WageParameterItemType, CarOwnType.Raskat), 2);
			}

			vboxDialog.Add(_notebook);
			_notebook.ShowAll();
		}

		private Widget GetWidget(WidgetViewModelBase viewModel)
		{
			switch(viewModel)
			{
				case FixedWageParameterItemViewModel vm:
					return new FixedWageParameterView(vm);
				case PercentWageParameterItemViewModel vm:
					return new PercentWageParameterView(vm);
				case SalesPlanWageParameterItemViewModel vm:
					return new SalesPlanWageParameterView(vm);
				case RatesLevelWageParameterItemViewModel vm:
					return new RatesLevelWageParameterView(vm);
				case OldRatesWageParameterItemViewModel vm:
					return new OldRatesWageParameterView(vm);
				default:
					return null;
			}
		}

		private Label GetNotebookHeaderLabel(WageParameterItemTypes wageParameterItemType, CarOwnType carOwnType = CarOwnType.Driver)
		{
			string label = string.Empty;

			switch(carOwnType)
			{
				case CarOwnType.Driver:
					label = "для ТС водителей";
					break;
				case CarOwnType.Company:
					label = "для ТС компании";
					break;
				case CarOwnType.Raskat:
					label = "для ТС в раскате";
					break;
			}

			switch(wageParameterItemType)
			{
				case WageParameterItemTypes.RatesLevel:
					return new Label($"Уровень ставок { label }");
				default:
					return new Label($"{ wageParameterItemType.GetEnumTitle() }" );
			}
		}
	}
}
