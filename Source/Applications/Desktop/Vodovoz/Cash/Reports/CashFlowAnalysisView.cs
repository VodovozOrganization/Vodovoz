using Vodovoz.Extensions;
using Gamma.Binding;
using Gamma.Binding.Core.RecursiveTreeConfig;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate.Util;
using QS.Views.GtkUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.ViewModels.Cash.Reports;
using static Vodovoz.ViewModels.Cash.Reports.CashFlowAnalysisViewModel;
using static Vodovoz.ViewModels.Cash.Reports.CashFlowAnalysisViewModel.CashFlowDdsReport;

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
						cell.Markup = $"<b>{node.ThirdColumn}</b>";
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

		internal class ReportLine
		{
			private ReportLine()
			{

			}

			public ReportLine ParentLine { get; set; } = null;

			public List<ReportLine> ChildLines { get; } = new List<ReportLine>();

			public string FirstColumn { get; set; }

			public string SecondColumn { get; set; }

			public decimal ThirdColumn { get; set; }

			public bool IsAccented { get; set; } = false;

			public bool Bold { get; set; }

			public bool IsSeparator { get; private set; } = false;

			public static ReportLine Separator => new ReportLine() { IsSeparator = true };

			public static List<ReportLine> Map(CashFlowDdsReport report)
			{
				var result = new List<ReportLine>
				{
					Map(report.IncomesGroupLines.First()),
					Separator,
					Map(report.ExpensesGroupLines.First()),
					Separator,
					new ReportLine()
					{
						FirstColumn = "Прибыль",
						ThirdColumn = report.IncomesGroupLines.Sum(x => x.Money) - report.ExpensesGroupLines.Sum(x => x.Money),
						Bold = true,
						IsAccented = true
					}
				};

				return result;
			}

			private static ReportLine Map(IncomesGroupLine incomesGroupLine)
			{
				var result = new ReportLine
				{
					FirstColumn = "Доходы",
					ThirdColumn = incomesGroupLine.Money,
					Bold = true,
					IsAccented = true
				};

				result.ChildLines.AddRange(Map(
						result,
						incomesGroupLine.Groups,
						incomesGroupLine.IncomeCategories));

				return result;
			}

			private static ReportLine Map(ExpensesGroupLine expensesGroupLine)
			{
				var result = new ReportLine
				{
					FirstColumn = "Расходы",
					ThirdColumn = expensesGroupLine.Money,
					Bold = true,
					IsAccented = true
				};

				result.ChildLines.AddRange(Map(
					result,
					expensesGroupLine.Groups,
					expensesGroupLine.ExpenseCategories));

				return result;
			}

			private static IEnumerable<ReportLine> Map(
				ReportLine parent,
				List<ExpensesGroupLine> expensesGroupLines,
				List<FinancialExpenseCategoryLine> expenseCategoryLines)
			{
				var result = new List<ReportLine>();

				foreach(var expensesGroupLine in expensesGroupLines)
				{
					result.Add(Map(parent, expensesGroupLine));
				}

				foreach(var expenseCategoryLine in expenseCategoryLines)
				{
					result.Add(Map(parent, expenseCategoryLine));
				}

				return result;
			}

			private static ReportLine Map(
				ReportLine parent,
				FinancialExpenseCategoryLine expenseCategoryLine)
			{
				return new ReportLine()
				{
					ParentLine = parent,
					SecondColumn = expenseCategoryLine.Title,
					ThirdColumn = expenseCategoryLine.Money
				};
			}

			private static ReportLine Map(
				ReportLine parent,
				ExpensesGroupLine expensesGroupLine)
			{
				var result = new ReportLine()
				{
					ParentLine = parent,
					SecondColumn = expensesGroupLine.Title,
					ThirdColumn = expensesGroupLine.Money
				};

				result.ChildLines.AddRange(Map(
					result,
					expensesGroupLine.Groups,
					expensesGroupLine.ExpenseCategories));

				return result;
			}

			private static List<ReportLine> Map(
				ReportLine parent,
				IEnumerable<IncomesGroupLine> incomesGroupLines,
				IEnumerable<FinancialIncomeCategoryLine> incomeCategoryLines)
			{
				var result = new List<ReportLine>();

				foreach(var incomesGroupLine in incomesGroupLines)
				{
					result.Add(Map(parent, incomesGroupLine));
				}

				foreach(var incomeCategoryLine in incomeCategoryLines)
				{
					result.Add(Map(parent, incomeCategoryLine));
				}

				return result;
			}

			private static ReportLine Map(
				ReportLine parent,
				FinancialIncomeCategoryLine incomeCategoryLine)
			{
				return new ReportLine()
				{
					ParentLine = parent,
					SecondColumn = incomeCategoryLine.Title,
					ThirdColumn = incomeCategoryLine.Money
				};
			}

			private static ReportLine Map(
				ReportLine parent,
				IncomesGroupLine incomesGroupLine)
			{
				var result = new ReportLine()
				{
					ParentLine = parent,
					SecondColumn = incomesGroupLine.Title,
					ThirdColumn = incomesGroupLine.Money
				};

				result.ChildLines.AddRange(Map(
					result,
					incomesGroupLine.Groups,
					incomesGroupLine.IncomeCategories));

				return result;
			}
		}
	}
}
