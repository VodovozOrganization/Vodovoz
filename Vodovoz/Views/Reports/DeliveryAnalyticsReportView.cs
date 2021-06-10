using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Utilities;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics;

namespace Vodovoz.Views.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryAnalyticsReportView : DialogViewBase<DeliveryAnalyticsViewModel>
	{
		public DeliveryAnalyticsReportView(DeliveryAnalyticsViewModel viewModel) : base (viewModel)
		{
			Build();
			ConfigureReport();
		}

		protected void OnExportBtnClicked(object sender, EventArgs e)
		{
		}
		private void ConfigureReport()
		{
			treeviewPartCity.ColumnsConfig = FluentColumnsConfig<WageDistrictNode>.Create()
            .AddColumn("").AddToggleRenderer(x => x.Selected)
            .AddColumn("").AddTextRenderer(x => x.WageDistrict.Title).Finish(); 
			treeviewPartCity.Binding.AddBinding(ViewModel, x=>x.WageDistrictNodes, x=>x.ItemsDataSource).InitializeFromSource();
			
			treeviewGeographic.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>.Create()
            .AddColumn("").AddToggleRenderer(x=>x.Selected)
            .AddColumn("").AddTextRenderer(x=>x.GeographicGroup.Name)
            .Finish(); 
			treeviewGeographic.Binding.AddBinding(ViewModel, x=>x.GeographicGroupNodes, x=>x.ItemsDataSource).InitializeFromSource();
			
			treeviewWave.ColumnsConfig = FluentColumnsConfig<WaveNode>.Create()
            .AddColumn("").AddToggleRenderer(x=>x.Selected)
            .AddColumn("").AddTextRenderer(x=>x.WaveNodes.GetEnumTitle())
            .Finish(); 
			treeviewWave.Binding.AddBinding(ViewModel, x => x.WaveList, x => x.ItemsDataSource).InitializeFromSource();
      }
      
      private void OnLoadReportBtnCLicked(object sender, EventArgs e)
      {
         var parentW = GtkHelper.GetParentWindow(this);
         var csvFilter = new FileFilter();
         csvFilter.AddPattern("*.csv");
         csvFilter.Name = "Comma Separated Values File (*.csv)";
           
         var param = new object[4];
         param[0] = "Cancel";
         param[1] = ResponseType.Cancel;
         param[2] = "Save";
         param[3] = ResponseType.Accept;

         var fc = new FileChooserDialog("Save File As", parentW, FileChooserAction.Save, param)
         {
            CurrentName = "Аналитика объемов доставки.csv"
         };
           
         fc.AddFilter(csvFilter);

         if (fc.Run() == (int)ResponseType.Accept)
         {
            ViewModel.FileName = fc.Filename;
            ViewModel.ExportCommand.Execute();
         }
           
         fc.Destroy();
      }
   }
}