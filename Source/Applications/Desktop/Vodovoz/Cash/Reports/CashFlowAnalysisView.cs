using Gamma.Binding;
using Gamma.Binding.Core.RecursiveTreeConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System.Collections.Generic;
using System.ComponentModel;
using Vodovoz.Extensions;
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

			_defaultAccentColor = Rc.GetStyle(this).IsDark()
				? new Gdk.Color(233, 84, 32)
				: ViewModel.AccentColor.ToGdkColor();

			_defaultTreeViewBackgroundColor = Rc.GetStyle(ytreeReportIndicatorsRows).Background(StateType.Normal);
		}

		private void Initialize()
		{
			UpdateSliderArrow();

			dateStart.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date)
				.InitializeFromSource();

			dateEnd.Binding
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.Date)
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

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				_lines = ReportLine.Map(ViewModel.Report);

				ConfigureReportView();
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
					if(node.IsSeparator)
					{
						cell.Text = string.Empty;
					}
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
						cell.Markup = $"<b>{node.FirstColumn}</b>";
					}
				})
				.AddColumn("")
				.AddTextRenderer(x => x.SecondColumn)
				.AddSetter((cell, node) =>
				{
					if(node.IsSeparator)
					{
						cell.Text = string.Empty;
					}
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
				.AddSetter((cell, node) => {
					var value = node.ThirdColumn == 0 ? "-" : node.ThirdColumn.ToString("# ### ### ##0.00");

					if(node.IsSeparator)
					{
						cell.Text = string.Empty;
					}
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
