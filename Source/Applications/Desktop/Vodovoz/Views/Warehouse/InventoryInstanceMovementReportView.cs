using System;
using System.Threading;
using System.Threading.Tasks;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class InventoryInstanceMovementReportView : TabViewBase<InventoryInstanceMovementReportViewModel>
	{
		public InventoryInstanceMovementReportView(InventoryInstanceMovementReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			inventoryInstanceEntry.ViewModel = ViewModel.InventoryInstanceEntryViewModel;
			
			lblInventoryNumber.Binding
				.AddBinding(ViewModel, vm => vm.InventoryNumber, w => w.LabelProp)
				.InitializeFromSource();

			buttonLoad.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsGenerating, w => w.Visible)
				.AddFuncBinding(vm => !vm.IsGenerating, w => w.Sensitive)
				.InitializeFromSource();
			buttonLoad.Clicked += OnLoadClicked;
			
			buttonAbort.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsGenerating, w => w.Visible)
				.AddBinding(vm => vm.IsGenerating, w => w.Sensitive)
				.InitializeFromSource();
			buttonAbort.Clicked += OnAbortClicked;

			buttonExport.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsGenerating && vm.MovementHistoryNodes != null, w => w.Sensitive)
				.InitializeFromSource();
			buttonExport.Clicked += OnExportClicked;
			
			eventboxArrow.ButtonPressEvent += (o, args) =>
			{
				vboxSections.Visible = !vboxSections.Visible;
				arrowSlider.ArrowType = vboxSections.Visible ? ArrowType.Left : ArrowType.Right;
			};

			ConfigureTreeData();
		}

		private void ConfigureTreeData()
		{
			treeData.ColumnsConfig = FluentColumnsConfig<InventoryInstanceMovementHistoryNode>.Create()
				.AddColumn("Дата")
					.AddTextRenderer(n => n.Date.ToString("dd.MM.yyyy HH:mm:ss"))
				.AddColumn("№ док-та")
					.AddTextRenderer(n => n.DocumentId.ToString())
				.AddColumn("Документ")
					.AddTextRenderer(n => n.Document)
				.AddColumn("Отправитель")
					.AddTextRenderer(n => n.Sender)
				.AddColumn("Получатель")
					.AddTextRenderer(n => n.Receiver)
				.AddColumn("Автор")
					.AddTextRenderer(n => n.Author)
				.AddColumn("Изменил")
					.AddTextRenderer(n => n.Editor)
				.AddColumn("Комментарий")
					.AddTextRenderer(n => n.Comment)
				.AddColumn("")
				.Finish();
			
			treeData.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNode, w => w.SelectedRow)
				.InitializeFromSource();
			treeData.RowActivated += OnTreeDataRowActivated;
		}

		private void OnTreeDataRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.OpenWarehouseDocumentCommand.Execute();
		}

		#region ButtonsHandlers

		private void OnAbortClicked(object sender, EventArgs e)
		{
			ViewModel.ReportGenerationCancellationTokenSource.Cancel();
		}

		private void OnLoadClicked(object sender, EventArgs e)
		{
			if(ViewModel.SelectedInstance is null)
			{
				ViewModel.ShowWarning("Не выбран экземпляр номенклатуры!");
				return;
			}
			
			ViewModel.ReportGenerationCancellationTokenSource = new CancellationTokenSource();
			ViewModel.IsGenerating = true;

			Task.Run(async () =>
			{
				try
				{
					await ViewModel.ActionGenerateReportAsync(ViewModel.ReportGenerationCancellationTokenSource.Token);

					Application.Invoke((s, eventArgs) =>
					{
						treeData.ItemsDataSource = ViewModel.MovementHistoryNodes;
						treeData.YTreeModel.EmitModelChanged();
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
			}, ViewModel.ReportGenerationCancellationTokenSource.Token);
		}
		
		private void OnExportClicked(object sender, EventArgs e)
		{
			ViewModel.ExportReport();
		}

		#endregion
	}
}
