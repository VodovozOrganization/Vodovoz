using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics;

namespace Vodovoz.Views.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryAnalyticsReportView : TabViewBase<DeliveryAnalyticsViewModel>, ISingleUoWDialog
	{
		public DeliveryAnalyticsReportView(DeliveryAnalyticsViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureReport();
		}

		public IUnitOfWork UoW { get; set; }

		protected void OnExportBtnClicked(object sender, EventArgs e)
		{
			var parentWindow = GtkHelper.GetParentWindow(this);

			var excelFilter = new FileFilter();
			const string extension = ".xlsx";
			excelFilter.Name = $"Документ Microsoft Excel ({extension})";
			excelFilter.AddMimeType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			excelFilter.AddPattern($"*{extension}");

			var fileChooserDialog = new FileChooserDialog(
				"Сохранение выгрузки",
				parentWindow,
				FileChooserAction.Save,
				Stock.Cancel, ResponseType.Cancel, Stock.Save, ResponseType.Accept)
			{
				DoOverwriteConfirmation = true,
				CurrentName = $"{Tab.TabName} " +
				$"с {ViewModel.StartDeliveryDate.Value.ToShortDateString()} " +
				$"по {ViewModel.EndDeliveryDate.Value.ToShortDateString()}.xlsx"
			};

			fileChooserDialog.AddFilter(excelFilter);
			fileChooserDialog.ShowAll();
			if((ResponseType)fileChooserDialog.Run() == ResponseType.Accept)
			{
				if(String.IsNullOrWhiteSpace(fileChooserDialog.Filename))
				{
					fileChooserDialog.Destroy();
					return;
				}
				var fileName = fileChooserDialog.Filename;
				ViewModel.FileName = fileName.EndsWith(".xlsx") ? fileName : fileName + ".xlsx";
				fileChooserDialog.Destroy();
				ViewModel.ExportCommand.Execute();
			}
			else
			{
				fileChooserDialog.Destroy();
			}
		}

		private void ConfigureReport()
		{
			UoW = ViewModel.Uow;

			treeviewPartCity.ColumnsConfig = FluentColumnsConfig<WageDistrictNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("").AddTextRenderer(x => x.WageDistrict.Name)
				.Finish();
			treeviewPartCity.Binding.AddBinding(ViewModel, x => x.WageDistrictNodes, x => x.ItemsDataSource).InitializeFromSource();
			treeviewPartCity.HeadersVisible = false;

			treeviewGeographic.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();
			treeviewGeographic.Binding.AddBinding(ViewModel, x => x.GeographicGroupNodes, x => x.ItemsDataSource).InitializeFromSource();
			treeviewGeographic.HeadersVisible = false;

			treeviewWave.ColumnsConfig = FluentColumnsConfig<WaveNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("").AddTextRenderer(x => x.WaveNodes.GetEnumTitle())
				.Finish();
			treeviewWave.Binding.AddBinding(ViewModel, x => x.WaveList, x => x.ItemsDataSource).InitializeFromSource();
			treeviewWave.HeadersVisible = false;

			treeviewWeekDay.ColumnsConfig = FluentColumnsConfig<WeekDayNodes>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("").AddTextRenderer(x => x.WeekNameNode.GetEnumTitle())
				.Finish();
			treeviewWeekDay.Binding.AddBinding(ViewModel, s => s.WeekDayName, x => x.ItemsDataSource).InitializeFromSource();
			treeviewWeekDay.HeadersVisible = false;

			districtEntry.SetEntityAutocompleteSelectorFactory(ViewModel.DistrictSelectorFactory);
			districtEntry.Binding.AddBinding(ViewModel, x => x.District, x => x.Subject).InitializeFromSource();

			deliveryDate.Binding.AddBinding(ViewModel, s => s.StartDeliveryDate, w => w.StartDateOrNull);
			deliveryDate.Binding.AddBinding(ViewModel, s => s.EndDeliveryDate, w => w.EndDateOrNull);

			exportBtn.Binding.AddBinding(ViewModel, vm => vm.HasExportReport, w => w.Sensitive).InitializeFromSource();

			btnAllDay.Clicked += OnButtonStatusAllClicked;
			btnUnAllDay.Clicked += OnButtonStatusNoneClicked;
			btnHelp.Visible = false;
			// btnHelp.Clicked += OnButtonHelpShowClicked;
		}

		protected void OnButtonStatusAllClicked(object sender, EventArgs e)
		{
			ViewModel.AllStatusCommand.Execute();
		}

		protected void OnButtonStatusNoneClicked(object sender, EventArgs e)
		{
			ViewModel.NoneStatusCommand.Execute();
		}
		protected void OnButtonHelpShowClicked(object sender, EventArgs e)
		{
			ViewModel.ShowHelpCommand.Execute();
		}
	}
}