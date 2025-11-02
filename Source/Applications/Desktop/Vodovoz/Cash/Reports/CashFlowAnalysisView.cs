using Gamma.Binding;
using Gamma.Binding.Core.RecursiveTreeConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Cash.Reports;

namespace Vodovoz.Cash.Reports
{
	public partial class CashFlowAnalysisView : TabViewBase<CashFlowAnalysisViewModel>
	{
		private List<ReportLine> _lines;
		private readonly Gdk.Color _defaultTreeViewBackgroundColor;
		private readonly Gdk.Color _defaultAccentColor;

		public CashFlowAnalysisView(CashFlowAnalysisViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();

			_defaultAccentColor = GdkColors.CashFlowTotalColor;
			_defaultTreeViewBackgroundColor = GdkColors.PrimaryBase;
		}

		private void Initialize()
		{
			UpdateSliderArrow();

			yradiobuttonDdr.Toggled += ReportModeChanged;

			yradiobuttonDds.Toggled += ReportModeChanged;

			dateStart.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date)
				.InitializeFromSource();

			dateEnd.Binding
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.Date)
				.InitializeFromSource();

			ycheckbuttonHideCategoriesWithoutDocuments.Binding
				.AddBinding(ViewModel, vm => vm.HideCategoriesWithoutDocuments, w => w.Active)
				.InitializeFromSource();

			ybuttonCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerateDdsReport, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += (s, e) => ViewModel.GenerateDdsReportCommand.Execute();

			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanSaveReport, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSave.Clicked += (s, e) => ViewModel.SaveReportCommand.Execute();

			buttonInfo.Clicked += (s, e) => ViewModel.ShowDdsReportInfoCommand.Execute();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void ReportModeChanged(object sender, EventArgs e)
		{
			if(sender is yRadioButton reportModeRadioButton && reportModeRadioButton.Active)
			{
				if(reportModeRadioButton.Name.Contains(CashFlowAnalysisViewModel.CashFlowDdsReport.ReportMode.Dds.ToString()))
				{
					ViewModel.ReportMode = CashFlowAnalysisViewModel.CashFlowDdsReport.ReportMode.Dds;
				}
				else if(reportModeRadioButton.Name.Contains(CashFlowAnalysisViewModel.CashFlowDdsReport.ReportMode.Ddr.ToString()))
				{
					ViewModel.ReportMode = CashFlowAnalysisViewModel.CashFlowDdsReport.ReportMode.Ddr;
				}
			}
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				_lines = ReportLine.Map(ViewModel.Report);

				ConfigureReportView();
				return;
			}
		}

		private void ConfigureReportView()
		{
			var config = new RecursiveConfig<ReportLine>(
				x => x.ParentLine,
				x => x.ChildLines);

			ytreeReportIndicatorsRows.ItemsDataSource = _lines;

			ytreeReportIndicatorsRows.YTreeModel = new RecursiveTreeModel<ReportLine>(_lines, config);

			ytreeReportIndicatorsRows.ColumnsConfig = ColumnsConfigFactory.Create<ReportLine>()
				.AddColumn("")
				.AddTextRenderer(x => x.FirstColumn)
				.AddSetter((cell, node) =>
				{
					if(node.IsAccented)
					{
						cell.BackgroundGdk = _defaultAccentColor;
					}
					else
					{
						cell.BackgroundGdk = _defaultTreeViewBackgroundColor;
						cell.Xalign = 1;
					}
					if(node.Bold)
					{
						cell.Markup = $"<b>{node.FirstColumn}</b>";
					}
				})
				.AddColumn("")
				.AddTextRenderer(x => x.SecondColumn)
				.AddSetter((cell, node) =>
				{
					if(node.IsAccented)
					{
						cell.BackgroundGdk = _defaultAccentColor;
					}
					else
					{
						cell.BackgroundGdk = _defaultTreeViewBackgroundColor;
					}
					if(node.Bold)
					{
						cell.Markup = $"<b>{node.SecondColumn}</b>";
					}
				})
				.AddColumn("")
				.AddNumericRenderer(x => x.ThirdColumn)
				.AddSetter((cell, node) =>
				{
					var value = node.ThirdColumn == 0 ? "-" : node.ThirdColumn.ToString("# ### ### ##0.00");
					cell.Xalign = 1;

					if(node.IsAccented)
					{
						cell.BackgroundGdk = _defaultAccentColor;
					}
					else
					{
						cell.BackgroundGdk = _defaultTreeViewBackgroundColor;
					}
					if(node.Bold)
					{
						cell.Markup = $"<b>{value}</b>";
					}
					else
					{
						cell.Markup = value;
					}
				})
				.AddColumn("")
				.Finish();

			ytreeReportIndicatorsRows.HeadersVisible = false;

			ytreeReportIndicatorsRows.EnableGridLines = TreeViewGridLines.Both;

			ytreeReportIndicatorsRows.YTreeModel.EmitModelChanged();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxParameters.Visible ? ArrowType.Left : ArrowType.Right;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxParameters.Visible = !vboxParameters.Visible;
			UpdateSliderArrow();
		}
	}
}
