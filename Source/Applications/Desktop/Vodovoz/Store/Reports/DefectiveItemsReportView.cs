﻿using DynamicData;
using Gtk;
using QS.Dialog;
using QS.Views.Dialog;
using System;
using System.ComponentModel;
using Gamma.Utilities;
using Vodovoz.ViewModels.Store.Reports;
using Gamma.ColumnConfig;
using System.Linq;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Store.Reports
{
	[ToolboxItem(true)]
	public partial class DefectiveItemsReportView : DialogViewBase<DefectiveItemsReportViewModel>
	{
		private int _hpanedDefaultPosition = 480;
		private int _hpanedMinimalPosition = 16;
		private readonly IGuiDispatcher _guiDispatcher;

		public DefectiveItemsReportView(
			DefectiveItemsReportViewModel viewModel,
			IGuiDispatcher guiDispatcher)
			: base(viewModel)
		{
			_guiDispatcher = guiDispatcher
				?? throw new ArgumentNullException(nameof(guiDispatcher));

			Build();

			Initialize();

			UpdateSliderArrow();
		}

		private void Initialize()
		{
			yEnumCmbSource.ItemsEnum = typeof(DefectSource);
			yEnumCmbSource.AddEnumToHideList(DefectSource.None);
			yEnumCmbSource.ShowSpecialStateAll = true;

			yEnumCmbSource.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DefectSource, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			datePeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryDriver.ViewModel = ViewModel.DriverViewModel;

			ybuttonSave.BindCommand(ViewModel.SaveCommand);

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);

			ybuttonCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortCreateCommand);

			ybuttonAbortCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCancelGenerateReport, w => w.Visible)
				.InitializeFromSource();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			hpaned1.Position = _hpanedDefaultPosition;

			UpdateSliderArrow();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					RefreshReportPreview();
					QueueDraw();
				});
			}
		}

		private void RefreshReportPreview()
		{
			ytreeviewMain.CreateFluentColumnsConfig<DefectiveItemsReport.DefectiveItemsReportRow>()
				.AddColumn("№").AddNumericRenderer(n => ViewModel != null ? ViewModel.Report != null ? ViewModel.Report.Rows != null ? ViewModel.Report.Rows.IndexOf(n) + 1 : 0 : 0 : 0)
				.AddColumn("Дата").AddDateRenderer(x => x.Date)
				.AddColumn("Кол-во").AddNumericRenderer(x => x.Amount)
				.AddColumn("Тип").AddTextRenderer(x => x.DefectTypeName)
				.AddColumn("Место обнаружения проблемы").AddTextRenderer(x => x.DefectDetectedAt)
				.AddColumn("Источник").AddTextRenderer(x => x.DefectSource.GetEnumTitle())
				.AddColumn("Водитель и номер МЛ").AddTextRenderer(x => x.RouteListId == null ? "" : $"{x.DriverLastName} МЛ №{x.RouteListId}")
				.AddColumn("Документ и номер").AddTextRenderer(x => $"{x.DocumentTypeName} {x.Id}")
				.AddColumn("Автор").AddTextRenderer(x => x.AuthorLastName)
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment)
				.Finish();

			var summaryConfigPart = new FluentColumnsConfig<DefectiveItemsReport.SummaryDisplayRow>()
			   .AddColumn("Из них")
			   .AddTextRenderer(x => x.Title);

			var dynamicColumnsTitles = ViewModel.Report.WarehouseNames;
			var dynamicColumnsCount = dynamicColumnsTitles.Count();

			ytreeviewSummaryBySource.CreateFluentColumnsConfig<DefectiveItemsReport.SummaryBySourceRow>()
				.AddColumn("").AddTextRenderer(x => $"{x.Value} браков по вине {x.Title}")
				.Finish();

			ytreeviewSummaryBySource.ItemsDataSource = ViewModel.Report.SummaryBySourceRows;

			for(var i = 0; i < dynamicColumnsCount; i++)
			{
				var currentId = i;

				summaryConfigPart.AddColumn(dynamicColumnsTitles.ElementAt(currentId))
					.AddTextRenderer(x => x.DynamicColls.ElementAt(currentId).ToString());
			}

			summaryConfigPart.AddColumn("Итог")
				.AddTextRenderer(x => x.Summary);

			ytreeviewSummary.ColumnsConfig = summaryConfigPart.Finish();

			if(ViewModel.Report != null)
			{
				ytreeviewMain.ItemsDataSource = ViewModel.Report.Rows;
				ytreeviewSummary.ItemsDataSource = ViewModel.Report.SummaryDisplayRows;
			}
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			scrolledwindow1.Visible = !scrolledwindow1.Visible;

			hpaned1.Position = scrolledwindow1.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = scrolledwindow1.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
