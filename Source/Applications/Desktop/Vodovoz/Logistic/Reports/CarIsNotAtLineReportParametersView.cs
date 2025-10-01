using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Logistic.Reports;
using static Vodovoz.Presentation.ViewModels.Logistic.Reports.CarIsNotAtLineReport;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Logistic.Reports
{
	public partial class CarIsNotAtLineReportParametersView
		: TabViewBase<CarIsNotAtLineReportParametersViewModel>
	{
		private const int _hpanedDefaultPosition = 530;
		private const int _hpanedMinimalPosition = 16;

		public CarIsNotAtLineReportParametersView(
			CarIsNotAtLineReportParametersViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		private Color _defaultColor => GdkColors.PrimaryBase;
		private Color _notAtLineStartTimeColor => GdkColors.IsLight ? GdkColors.BabyBlue : GdkColors.DarkBlue;
		private Color _carModelTypeColor => GdkColors.IsLight ? GdkColors.LightPurple : GdkColors.DarkViolet;
		private Color _carRegNumberColor => GdkColors.YellowMustard;
		private Color _subtableHeadersColor => GdkColors.IsLight ? GdkColors.BabyBlue : GdkColors.DarkBlue;
		private Color _subtableNameColor => GdkColors.IsLight ? GdkColors.LightPurple : GdkColors.DarkViolet;

		private void Initialize()
		{
			hpanedMain.Position = _hpanedDefaultPosition;

			datepickerDate.Binding
				.AddBinding(ViewModel, vm => vm.Date, w => w.Date)
				.InitializeFromSource();

			datepickerDate.IsEditable = true;

			yspinbuttonDaysCount.Binding
				.AddBinding(ViewModel, vm => vm.CountDays, w => w.ValueAsInt)
				.InitializeFromSource();

			vboxFilter.Remove(includeexcludefiltergroupview1);
			includeexcludefiltergroupview1 = new Presentation.Views.IncludeExcludeFilterGroupView(ViewModel.IncludeExludeFilterGroupViewModel);
			includeexcludefiltergroupview1.Show();
			vboxFilter.Add(includeexcludefiltergroupview1);

			ybuttonGenerate.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanAbortReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonGenerate.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);
			ybuttonInfo.BindCommand(ViewModel.ShowInfoCommand);

			ConfigureDataTreeView();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
		}

		private void ConfigureDataTreeView()
		{
			var columnsConfig = FluentColumnsConfig<UiRow>.Create();

			columnsConfig
				.AddColumn("№ п/п")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.IdString)
				.XAlign(0.5f)
				.RowCells().AddSetter<CellRenderer>(
				(cell, node) =>
				{
					var color = _defaultColor;

					if(node.IsSubtableNameRow)
					{
						color = _subtableNameColor;
					}
					else if(node.IsSubtableHeadereRow)
					{
						color = _subtableHeadersColor;
					}

					cell.CellBackgroundGdk = color;
				});

			columnsConfig
				.AddColumn("Дата начала простоя")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.DowntimeStartedAtString)
				.WrapWidth(100)
				.WrapMode(WrapMode.Word)
				.XAlign(0.5f)
				.RowCells().AddSetter<CellRenderer>(
				(cell, node) =>
				{
					var color = _defaultColor;

					if(node.IsMainRow)
					{
						color = _notAtLineStartTimeColor;
					}
					else if(node.IsSubtableNameRow)
					{
						color = _subtableNameColor;
					}
					else if(node.IsSubtableHeadereRow)
					{
						color = _subtableHeadersColor;
					}

					cell.CellBackgroundGdk = color;
				});

			columnsConfig
				.AddColumn("Тип авто")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.CarTypeWithGeographicalGroup)
				.WrapWidth(200)
				.WrapMode(WrapMode.Word)
				.XAlign(0.05f)
				.RowCells()
					.AddSetter<CellRenderer>(
				(cell, node) =>
				{
					var color = _defaultColor;

					if(node.IsMainRow || node.IsCatTransferRow || node.IsCarReceptionRow)
					{
						color = _carModelTypeColor;
					}
					else if(node.IsSubtableNameRow)
					{
						color = _subtableNameColor;
					}
					else if(node.IsSubtableHeadereRow)
					{
						color = _subtableHeadersColor;
					}

					cell.CellBackgroundGdk = color;
				});
			
			columnsConfig
				.AddColumn("П")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.CarOwnType)
				.WrapWidth(200)
				.WrapMode(WrapMode.Word)
				.XAlign(0.5f)
				.RowCells()
					.AddSetter<CellRenderer>(
					(cell, node) =>
					{
						var color = _defaultColor;

						if(node.IsMainRow || node.IsCatTransferRow || node.IsCarReceptionRow)
						{
							color = _carModelTypeColor;
						}
						else if(node.IsSubtableNameRow)
						{
							color = _subtableNameColor;
						}
						else if(node.IsSubtableHeadereRow)
						{
							color = _subtableHeadersColor;
						}

						cell.CellBackgroundGdk = color;
					});

			columnsConfig
				.AddColumn("Госномер")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.RegistationNumber)
				.WrapWidth(100)
				.WrapMode(WrapMode.Word)
				.XAlign(0.5f)
				.RowCells().AddSetter<CellRenderer>(
				(cell, node) =>
				{
					var color = _defaultColor;

					if(node.IsMainRow)
					{
						color = _carRegNumberColor;
					}
					else if(node.IsSubtableNameRow)
					{
						color = _subtableNameColor;
					}
					else if(node.IsSubtableHeadereRow)
					{
						color = _subtableHeadersColor;
					}

					cell.CellBackgroundGdk = color;
				});

			columnsConfig
				.AddColumn("Время / описание поломки")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.TimeAndBreakdownReason)
				.WrapWidth(200)
				.WrapMode(WrapMode.Word)
				.XAlign(0.05f)
				.RowCells().AddSetter<CellRenderer>(
				(cell, node) =>
				{
					var color = _defaultColor;

					if(node.IsSubtableNameRow)
					{
						color = _subtableNameColor;
					}
					else if(node.IsSubtableHeadereRow)
					{
						color = _subtableHeadersColor;
					}

					cell.CellBackgroundGdk = color;
				});

			columnsConfig
				.AddColumn("Зона ответственности")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.IsMainRow ? x.AreasOfResponsibilityShortNames : "")
				.WrapWidth(200)
				.WrapMode(WrapMode.Word)
				.XAlign(0.5f);

			columnsConfig
				.AddColumn("Планируемая дата\nвыпуска автомобиля\nна линию")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.PlannedReturnToLineDateString)
				.WrapWidth(100)
				.WrapMode(WrapMode.Word)
				.XAlign(0.5f);

			columnsConfig
				.AddColumn("Основания переноса даты")
				.HeaderAlignment(.5f)
				.AddTextRenderer(x => x.PlannedReturnToLineDateAndReschedulingReason)
				.WrapWidth(300)
				.WrapMode(WrapMode.Word)
				.XAlign(0.01f);

			ytreeReportRows.ColumnsConfig = columnsConfig.Finish();

			ytreeReportRows.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.UiRows : Enumerable.Empty<UiRow>(), w => w.ItemsDataSource)
				.InitializeFromSource();

			ytreeReportRows.EnableGridLines = TreeViewGridLines.Both;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxFilter.Visible = !vboxFilter.Visible;
			hpanedMain.Position = vboxFilter.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;
			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxFilter.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
