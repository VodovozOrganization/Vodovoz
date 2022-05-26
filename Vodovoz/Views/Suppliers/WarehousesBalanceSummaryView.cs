using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ViewModels.Suppliers;

namespace Vodovoz.Views.Suppliers
{
	[ToolboxItem(false)]
	public partial class WarehousesBalanceSummaryView : TabViewBase<WarehousesBalanceSummaryViewModel>
	{
		private Task _generationTask;

		public WarehousesBalanceSummaryView(WarehousesBalanceSummaryViewModel viewModel) : base(viewModel)
		{
			this.Build();

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

			var warsWidget = new SelectableParameterReportFilterView(ViewModel.WarsViewModel);
			vboxWarsFilter.Add(warsWidget);
			warsWidget.Show();

			eventboxArrow.ButtonPressEvent += (o, args) =>
			{
				vboxSections.Visible = !vboxSections.Visible;
				arrowSlider.ArrowType = vboxSections.Visible ? ArrowType.Left : ArrowType.Right;
			};

			treeData.EnableGridLines = TreeViewGridLines.Both;
		}

		private void ConfigureTreeView()
		{
			var columnsConfig = FluentColumnsConfig<BalanceSummaryRow>.Create()
				.AddColumn("Код").AddNumericRenderer(row => row.NomId).XAlign(0.5f)
				.AddColumn("Наименование").AddTextRenderer(row => row.NomTitle).XAlign(0.5f)
				.AddColumn("Мин. остаток").AddNumericRenderer(row => row.Min).XAlign(0.5f)
				.AddColumn("Общий остаток").AddNumericRenderer(row => row.Common).XAlign(0.5f)
				.AddColumn("Разница").AddNumericRenderer(row => row.Diff).XAlign(0.5f);

			for(var i = 0; i < ViewModel.Report.WarehousesTitles.Count; i++)
			{
				var index = i;
				columnsConfig.AddColumn($"{ViewModel.Report.WarehousesTitles[i]}").AddNumericRenderer(row => row.Separate[index])
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



		private async void Export()
		{
			var extension = ".xlsx";

			var filechooser = new FileChooserDialog("Сохранить отчет...",
				null,
				FileChooserAction.Save,
				"Отменить", ResponseType.Cancel,
				"Сохранить", ResponseType.Accept)
			{
				DoOverwriteConfirmation = true,
				CurrentName = $"{Tab.TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}{extension}"
			};

			var excelFilter = new FileFilter
			{
				Name = $"Документ Microsoft Excel ({extension})"
			};

			excelFilter.AddMimeType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			excelFilter.AddPattern($"*{extension}");
			filechooser.AddFilter(excelFilter);


			if(filechooser.Run() == (int)ResponseType.Accept)
			{
				var path = filechooser.Filename;

				if(!path.Contains(extension))
				{
					path += extension;
				}

				filechooser.Hide();

				await Task.Run(() =>
				{
					try
					{
						ViewModel.ExportReport(path);
					}
					catch(Exception ex)
					{
						
					}
				});
			}

			filechooser.Destroy();
		}
	}
}
