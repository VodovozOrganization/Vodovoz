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
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Suppliers
{
	[ToolboxItem(false)]
	public partial class WarehousesBalanceSummaryView : TabViewBase<WarehousesBalanceSummaryViewModel>
	{
		private Task _generationTask;
		private int _maxWarehousesInReportExport = 10;
		private SelectableParameterReportFilterView _nomenclaturesFilter;
		private SelectableParameterReportFilterView _storagesFilter;

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
			
			buttonAbort.Clicked += (sender, args) => ViewModel.ReportGenerationCancellationTokenSource.Cancel();
			buttonExport.Clicked += (sender, args) => Export();

			datePicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EndDate, w => w.DateOrNull)
				.AddFuncBinding(vm => vm.Sensitivity && !vm.ShowReserve, w => w.Sensitive)
				.InitializeFromSource();
			
			ycheckbuttonShowReserv.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowReserve, w => w.Active)
				.AddBinding(vm => vm.Sensitivity, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonShowPrices.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowPrices, w => w.Active)
				.AddBinding(vm => vm.Sensitivity, w => w.Sensitive)
				.InitializeFromSource();

			yhbox1.Binding
				.AddBinding(ViewModel, vm => vm.Sensitivity, w => w.Sensitive)
				.InitializeFromSource();
			
			radioAllNoms.Binding
				.AddBinding(ViewModel, vm => vm.AllNomenclatures, w => w.Active)
				.InitializeFromSource();
			
			radioGtZNoms.Binding
				.AddBinding(ViewModel, vm => vm.IsGreaterThanZeroByNomenclature, w => w.Active)
				.InitializeFromSource();
			
			yhbox2.Binding
				.AddBinding(ViewModel, vm => vm.Sensitivity, w => w.Sensitive)
				.InitializeFromSource();
			
			radioLeZNoms.Binding
				.AddBinding(ViewModel, vm => vm.IsLessOrEqualZeroByNomenclature, w => w.Active)
				.InitializeFromSource();
			
			radioLtMinNoms.Binding
				.AddBinding(ViewModel, vm => vm.IsLessThanMinByNomenclature, w => w.Active)
				.InitializeFromSource();
			
			radioGeMinNoms.Binding
				.AddBinding(ViewModel, vm => vm.IsGreaterOrEqualThanMinByNomenclature, w => w.Active)
				.InitializeFromSource();

			yhbox5.Binding
				.AddBinding(ViewModel, vm => vm.Sensitivity, w => w.Sensitive)
				.InitializeFromSource();
			
			radioAllWars.Binding
				.AddBinding(ViewModel, vm => vm.AllWarehouses, w => w.Active)
				.InitializeFromSource();
			
			radioGtZWars.Binding
				.AddBinding(ViewModel, vm => vm.IsGreaterThanZeroByWarehouse, w => w.Active)
				.InitializeFromSource();
			
			yhbox3.Binding
				.AddBinding(ViewModel, vm => vm.Sensitivity, w => w.Sensitive)
				.InitializeFromSource();
			
			radioLeZWars.Binding
				.AddBinding(ViewModel, vm => vm.IsLessOrEqualZeroByWarehouse, w => w.Active)
				.InitializeFromSource();
			
			radioLtMinWars.Binding
				.AddBinding(ViewModel, vm => vm.IsLessThanMinByWarehouse, w => w.Active)
				.InitializeFromSource();
			
			radioGeMinWars.Binding
				.AddBinding(ViewModel, vm => vm.IsGreaterOrEqualThanMinByWarehouse, w => w.Active)
				.InitializeFromSource();

			_nomenclaturesFilter = new SelectableParameterReportFilterView(ViewModel.NomsViewModel);
			vboxNomsFilter.Add(_nomenclaturesFilter);
			_nomenclaturesFilter.Show();

			_storagesFilter = new SelectableParameterReportFilterView(ViewModel.StoragesViewModel);
			vboxWarsFilter.Add(_storagesFilter);
			_storagesFilter.Show();
			
			enumChkListStorages.EnumType = typeof(StorageType);
			enumChkListStorages.SelectAll();
			enumChkListStorages.CheckStateChanged += EnumChkListStoragesOnCheckStateChanged;
			enumChkListStorages.OnlySelectAt(0);
			enumChkListStorages.Binding
				.AddBinding(ViewModel, vm => vm.Sensitivity, w => w.Sensitive)
				.InitializeFromSource();
			
			chkGroupByActiveStorages.Binding
				.AddBinding(ViewModel, vm => vm.GroupingActiveStorage, w => w.Active)
				.InitializeFromSource();

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
					parameterSet = ViewModel.StoragesParametersSets.SingleOrDefault(
						x => x.ParameterName == WarehousesBalanceSummaryViewModel.ParameterWarehouseStorages);
					break;
				case StorageType.Employee:
					parameterSet = ViewModel.StoragesParametersSets.SingleOrDefault(
						x => x.ParameterName == WarehousesBalanceSummaryViewModel.ParameterEmployeeStorages);
					break;
				case StorageType.Car:
					parameterSet = ViewModel.StoragesParametersSets.SingleOrDefault(
						x => x.ParameterName == WarehousesBalanceSummaryViewModel.ParameterCarStorages);
					break;
			}

			if(parameterSet is null)
			{
				return;
			}

			parameterSet.IsVisible = e.IsChecked;
			UpdateChkGroupByActiveStorages();
		}

		private void UpdateChkGroupByActiveStorages()
		{
			var activeGroupableParameter =
				enumChkListStorages.SelectedValues.FirstOrDefault(
					x => (StorageType)x == StorageType.Employee || (StorageType)x == StorageType.Car);
			
			chkGroupByActiveStorages.Visible = activeGroupableParameter != null && enumChkListStorages.SelectedValues.Count() == 1;
			
			if(activeGroupableParameter is null)
			{
				return;
			}
			
			switch(activeGroupableParameter)
			{
				case StorageType.Employee:
					chkGroupByActiveStorages.Label = "Группировать по работающим сотрудникам";
					break;
				case StorageType.Car:
					chkGroupByActiveStorages.Label = "Группировать по активным автомобилям";
					break;
			}
		}

		private void ConfigureBalanceSummaryTreeView()
		{
			var columnsConfig = FluentColumnsConfig<BalanceSummaryRow>.Create()
				.AddColumn(WarehousesBalanceSummaryViewModel.RowNumberTitle)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.Num)
					.XAlign(0.5f)
				.AddColumn(WarehousesBalanceSummaryViewModel.IdTitle)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.EntityId)
					.XAlign(0.5f)
				.AddColumn(WarehousesBalanceSummaryViewModel.EntityTitle)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(row => row.NomTitle)
					.XAlign(0.5f)
					.WrapWidth(500).WrapMode(WrapMode.Word)
				.AddColumn(WarehousesBalanceSummaryViewModel.InventoryNumberTitle)
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

			for(var i = 0; i < ViewModel.BalanceSummaryReport.WarehouseStoragesTitles?.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn($"{ViewModel.BalanceSummaryReport.WarehouseStoragesTitles[i]}")
				.AddNumericRenderer(row => row.WarehousesBalances[index])
				.XAlign(0.5f);
			}
			
			for(var i = 0; i < ViewModel.BalanceSummaryReport.EmployeeStoragesTitles?.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn($"{ViewModel.BalanceSummaryReport.EmployeeStoragesTitles[i]}")
				.AddNumericRenderer(row => row.EmployeesBalances[index])
				.XAlign(0.5f);
			}
			
			for(var i = 0; i < ViewModel.BalanceSummaryReport.CarStoragesTitles?.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn($"{ViewModel.BalanceSummaryReport.CarStoragesTitles[i]}")
				.AddNumericRenderer(row => row.CarsBalances[index])
				.XAlign(0.5f);
			}

			treeData.ColumnsConfig = columnsConfig.AddColumn("").Finish();
		}
		
		private void ConfigureActiveStoragesBalanceSummaryTreeView()
		{
			var storageTitle = ViewModel.GetActiveSelectedStorageTypeTitle();
			
			treeData.ColumnsConfig = FluentColumnsConfig<ActiveStoragesBalanceSummaryRow>.Create()
				.AddColumn(WarehousesBalanceSummaryViewModel.RowNumberTitle)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.RowNumberStorage)
					.XAlign(0.5f)
				.AddColumn(storageTitle)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(row => row.Storage)
					.XAlign(0.5f)
					.WrapWidth(500).WrapMode(WrapMode.Word)
				.AddColumn(WarehousesBalanceSummaryViewModel.RowNumberTitle)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.RowNumberFromStorage)
					.XAlign(0.5f)
				.AddColumn(WarehousesBalanceSummaryViewModel.IdTitle)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.EntityId)
					.XAlign(0.5f)
				.AddColumn(WarehousesBalanceSummaryViewModel.EntityTitle)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(row => row.Entity)
					.XAlign(0.5f)
					.WrapWidth(500).WrapMode(WrapMode.Word)
				.AddColumn(WarehousesBalanceSummaryViewModel.InventoryNumberTitle)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(row => row.InventoryNumber)
					.XAlign(0.5f)
				.AddColumn(WarehousesBalanceSummaryViewModel.BalanceTitle)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(row => row.Balance)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
		}

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.BalanceSummaryReport) && ViewModel.BalanceSummaryReport?.SummaryRows != null)
			{
				ConfigureBalanceSummaryTreeView();
				treeData.ItemsDataSource = ViewModel.BalanceSummaryReport.SummaryRows;
			}
			if(e.PropertyName == nameof(ViewModel.ActiveStoragesBalanceSummaryReport)
				&& ViewModel.ActiveStoragesBalanceSummaryReport?.ActiveStoragesBalanceRows != null)
			{
				ConfigureActiveStoragesBalanceSummaryTreeView();
				treeData.ItemsDataSource = ViewModel.ActiveStoragesBalanceSummaryReport.ActiveStoragesBalanceRows;
			}
			if(e.PropertyName == nameof(ViewModel.Sensitivity))
			{
				_nomenclaturesFilter.Sensitive = ViewModel.Sensitivity;
				_storagesFilter.Sensitive = ViewModel.Sensitivity;
			}
		}

		private async void ButtonLoadOnClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancellationTokenSource = new CancellationTokenSource();
			ViewModel.IsGenerating = true;

			_generationTask = Task.Run(async () =>
			{
				try
				{
					if(ViewModel.GroupingActiveStorage)
					{
						var report = await ViewModel.GenerateActiveStoragesBalanceSummaryReportAsync(ViewModel.ReportGenerationCancellationTokenSource.Token);

						Gtk.Application.Invoke((s, eventArgs) =>
						{
							ViewModel.ActiveStoragesBalanceSummaryReport = report;
						});
					}
					else
					{
						var defaultReport = await ViewModel.ActionGenerateReportAsync(ViewModel.ReportGenerationCancellationTokenSource.Token);

						Gtk.Application.Invoke((s, eventArgs) =>
						{
							ViewModel.BalanceSummaryReport = defaultReport;
						});
					}
				}
				catch(OperationCanceledException)
				{
					Gtk.Application.Invoke((s, eventArgs) =>
					{
						ViewModel.ShowWarning("Формирование отчета было прервано");
					});
				}
				catch(Exception ex)
				{
					Gtk.Application.Invoke((s, eventArgs) => throw ex);
				}
				finally
				{
					Gtk.Application.Invoke((s, eventArgs) =>
					{
						ViewModel.IsGenerating = false;
					});
				}
			}, ViewModel.ReportGenerationCancellationTokenSource.Token);

			await _generationTask;
		}

		private void Export()
		{
			if(!ViewModel.GroupingActiveStorage && ViewModel.BalanceSummaryReport is null
				|| (ViewModel.GroupingActiveStorage && ViewModel.ActiveStoragesBalanceSummaryReport is null))
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
