using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gamma.ColumnConfig;
using Gamma.Widgets;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using WrapMode = Pango.WrapMode;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Views.Suppliers
{
	[ToolboxItem(false)]
	public partial class WarehousesBalanceSummaryView : TabViewBase<WarehousesBalanceSummaryViewModel>
	{
		private Task _generationTask;
		private int _maxWarehousesInReportExport = 10;

		public WarehousesBalanceSummaryView(WarehousesBalanceSummaryViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
			buttonLoad.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsGenerating, w => w.Visible)
				.AddFuncBinding(vm => !vm.IsGenerating, w => w.Sensitive)
				.InitializeFromSource();
			buttonLoad.Clicked += ButtonLoadOnClicked;
			buttonAbort.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsGenerating, w => w.Visible)
				.AddBinding(vm => vm.IsGenerating, w => w.Sensitive)
				.InitializeFromSource();
			buttonAbort.Clicked += (sender, args) => { ViewModel.ReportGenerationCancelationTokenSource.Cancel(); };

			buttonExport.Clicked += (sender, args) => Export();

			datePicker.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.DateOrNull).InitializeFromSource();
			ycheckbuttonShowReserv.Binding.AddBinding(ViewModel, vm => vm.ShowReserve, w => w.Active).InitializeFromSource();

			ycheckbuttonShowReserv.Clicked += (object sender, EventArgs e) =>
			{
				datePicker.Sensitive = !ycheckbuttonShowReserv.Active;
				datePicker.Date = DateTime.Now;
			};

			ycheckbuttonShowPrices.Binding.AddBinding(ViewModel, vm => vm.ShowPrices, w => w.Active).InitializeFromSource();

			radioAllNoms.Binding.AddBinding(ViewModel, vm => vm.AllNomenclatures, w => w.Active).InitializeFromSource();
			radioGtZNoms.Binding.AddBinding(ViewModel, vm => vm.IsGreaterThanZeroByNomenclature, w => w.Active).InitializeFromSource();
			radioLeZNoms.Binding.AddBinding(ViewModel, vm => vm.IsLessOrEqualZeroByNomenclature, w => w.Active).InitializeFromSource();
			radioLtMinNoms.Binding.AddBinding(ViewModel, vm => vm.IsLessThanMinByNomenclature, w => w.Active).InitializeFromSource();
			radioGeMinNoms.Binding.AddBinding(ViewModel, vm => vm.IsGreaterOrEqualThanMinByNomenclature, w => w.Active).InitializeFromSource();

			radioAllWars.Binding.AddBinding(ViewModel, vm => vm.AllWarehouses, w => w.Active).InitializeFromSource();
			radioGtZWars.Binding.AddBinding(ViewModel, vm => vm.IsGreaterThanZeroByWarehouse, w => w.Active).InitializeFromSource();
			radioLeZWars.Binding.AddBinding(ViewModel, vm => vm.IsLessOrEqualZeroByWarehouse, w => w.Active).InitializeFromSource();
			radioLtMinWars.Binding.AddBinding(ViewModel, vm => vm.IsLessThanMinByWarehouse, w => w.Active).InitializeFromSource();
			radioGeMinWars.Binding.AddBinding(ViewModel, vm => vm.IsGreaterOrEqualThanMinByWarehouse, w => w.Active).InitializeFromSource();

			var nomsWidget = new SelectableParameterReportFilterView(ViewModel.NomsViewModel);
			vboxNomsFilter.Add(nomsWidget);
			nomsWidget.Show();

			var warsWidget = new SelectableParameterReportFilterView(ViewModel.StoragesViewModel);
			vboxWarsFilter.Add(warsWidget);
			warsWidget.Show();
			
			enumChkListStorages.EnumType = typeof(StorageType);
			enumChkListStorages.SelectAll();
			enumChkListStorages.CheckStateChanged += EnumChkListStoragesOnCheckStateChanged;
			//enumChkListStorages.OnlySelectAt(0);

			eventboxArrow.ButtonPressEvent += (o, args) =>
			{
				vboxSections.Visible = !vboxSections.Visible;
				arrowSlider.ArrowType = vboxSections.Visible ? ArrowType.Left : ArrowType.Right;
			};

			treeData.EnableGridLines = TreeViewGridLines.Both;
		}

		private void EnumChkListStoragesOnCheckStateChanged(object sender, CheckStateChangedEventArgs e)
		{
			SelectableParameterSet parameterSet = null;
			switch(e.Item)
			{
				case StorageType.Warehouse:
					parameterSet = ViewModel.StoragesParametersSets.SingleOrDefault(x => x.ParameterName == nameof(Warehouse));
					break;
				case StorageType.Employee:
					parameterSet = ViewModel.StoragesParametersSets.SingleOrDefault(x => x.ParameterName == nameof(Employee));
					break;
				case StorageType.Car:
					parameterSet = ViewModel.StoragesParametersSets.SingleOrDefault(x => x.ParameterName == nameof(Car));
					break;
			}

			if(parameterSet is null)
			{
				return;
			}

			parameterSet.IsVisible = e.IsChecked;
		}

		private void ConfigureTreeView()
		{
			var columnsConfig = FluentColumnsConfig<BalanceSummaryRow>.Create()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => ViewModel.Report.SummaryRows.IndexOf(row) + 1)
					.XAlign(0.5f)
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.NomId)
					.XAlign(0.5f)
				.AddColumn("Наименование")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(row => row.NomTitle)
					.XAlign(0.5f)
					.WrapWidth(500).WrapMode(WrapMode.Word)
				.AddColumn("Инвентарный\nномер")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(row => row.InventoryNumber)
					.XAlign(0.5f)
				.AddColumn("Мин. остаток")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.Min)
					.XAlign(0.5f);

			if(ViewModel.ShowReserve)
			{
				columnsConfig
				.AddColumn("В резерве").AddNumericRenderer(row => row.ReservedItemsAmount).XAlign(0.5f)
				.AddColumn("Доступно для заказа").AddNumericRenderer(row => row.AvailableItemsAmount).XAlign(0.5f);
			}

			columnsConfig
				.AddColumn("Общий остаток")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.Common)
					.XAlign(0.5f)
				.AddColumn("Разница")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.Diff)
					.XAlign(0.5f);

			if(ViewModel.ShowPrices)
			{
				columnsConfig
					.AddColumn("Цена закупки").AddNumericRenderer(row => row.PurchasePrice).XAlign(0.5f).Digits(2)
					.AddColumn("Цена").AddNumericRenderer(row => row.Price).XAlign(0.5f).Digits(2)
					.AddColumn("Цена KulerSale").AddNumericRenderer(row => row.AlternativePrice).XAlign(0.5f).Digits(2);
			}

			for(var i = 0; i < ViewModel.Report.WarehouseStoragesTitles?.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn($"{ViewModel.Report.WarehouseStoragesTitles[i]}")
				.AddNumericRenderer(row => row.WarehousesBalances[index])
				.XAlign(0.5f);
			}
			
			for(var i = 0; i < ViewModel.Report.EmployeeStoragesTitles?.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn($"{ViewModel.Report.EmployeeStoragesTitles[i]}")
				.AddNumericRenderer(row => row.EmployeesBalances[index])
				.XAlign(0.5f);
			}
			
			for(var i = 0; i < ViewModel.Report.CarStoragesTitles?.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn($"{ViewModel.Report.CarStoragesTitles[i]}")
				.AddNumericRenderer(row => row.CarsBalances[index])
				.XAlign(0.5f);
			}

			treeData.ColumnsConfig = columnsConfig.AddColumn("").Finish();
		}

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report) && ViewModel.Report?.SummaryRows != null)
			{
				ConfigureTreeView();
				treeData.ItemsDataSource = ViewModel.Report.SummaryRows;
			}
		}

		private async void ButtonLoadOnClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancelationTokenSource = new CancellationTokenSource();
			ViewModel.IsGenerating = true;

			_generationTask = Task.Run(async () =>
			{
				try
				{
					var report = await ViewModel.ActionGenerateReportAsync(ViewModel.ReportGenerationCancelationTokenSource.Token);

					Application.Invoke((s, eventArgs) =>
					{
						ViewModel.Report = report;
					});
				}
				catch(OperationCanceledException)
				{
					Application.Invoke((s, eventArgs) =>
					{
						ViewModel.ShowWarning("Формирование отчета было прервано");
					});
				}
				catch(Exception ex)
				{
					Application.Invoke((s, eventArgs) => throw ex);
				}
				finally
				{
					Application.Invoke((s, eventArgs) =>
					{
						ViewModel.IsGenerating = false;
					});
				}
			}, ViewModel.ReportGenerationCancelationTokenSource.Token);

			await _generationTask;
		}

		private void Export()
		{
			if(ViewModel.Report == null)
			{
				ViewModel.ShowWarning("Отчет пустой.");
				return;
			}

			try
			{
				ViewModel.ExportReport();
			}
			catch(OutOfMemoryException)
			{
				ViewModel.ShowWarning("Слишком большой обьём данных.\n Пожалуйста, уменьшите выборку.");
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}
	}
}
