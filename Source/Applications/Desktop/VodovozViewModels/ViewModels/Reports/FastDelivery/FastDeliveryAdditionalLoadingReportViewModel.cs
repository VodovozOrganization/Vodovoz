using ClosedXML.Report;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools;
using static Vodovoz.ViewModels.ViewModels.Reports.FastDelivery.FastDeliveryAdditionalLoadingReportViewModel.FastDeliveryAdditionalLoadingReport;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryAdditionalLoadingReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\Orders\FastDeliveryAdditionalLoadingReport.xlsx";
		private const string _fastDeliveryRemainingBottlesReportPath = @".\Reports\Logistic\FastDeliveryAdditionalLoadingReport.xlsx";

		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private FastDeliveryAdditionalLoadingReport _report;
		private bool _isRunning;

		public FastDeliveryAdditionalLoadingReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService;
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			Title = "Отчёт по дозагрузке МЛ";

			CreateDateFrom = DateTime.Now.Date;
			CreateDateTo = DateTime.Now.LatestDayTime();

			GenerateCommand = new DelegateCommand(GenerateReport);
			ExportCommand = new DelegateCommand(ExportReport);
			GenerateFastDeliveryRemainingBottlesReportCommand = new DelegateCommand(GenerateFastDeliveryRemainingBottlesReport);
		}

		private IList<FastDeliveryAdditionalLoadingReportRow> GenerateReportRows()
		{
			if(!IsHasDates)
			{
				return new List<FastDeliveryAdditionalLoadingReportRow>();
			}

			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			FastDeliveryAdditionalLoadingReportRow resultAlias = null;
			AdditionalLoadingDocumentItem additionalLoadingDocumentItemAlias = null;
			AdditionalLoadingDocument additionalLoadingDocumentAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => additionalLoadingDocumentItemAlias)
				.JoinAlias(() => additionalLoadingDocumentItemAlias.AdditionalLoadingDocument, () => additionalLoadingDocumentAlias)
				.JoinEntityAlias(() => routeListAlias, () => routeListAlias.AdditionalLoadingDocument.Id == additionalLoadingDocumentAlias.Id)
				.JoinAlias(() => additionalLoadingDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => routeListAlias.Date >= CreateDateFrom.Value.Date
					&& routeListAlias.Date <= CreateDateTo.Value.Date.LatestDayTime());

			var ownOrdersAmountSubquery = QueryOver.Of(() => routeListItemAlias)
				.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.WhereRestrictionOn(() => routeListItemAlias.Status).Not.IsIn(new RouteListItemStatus[]
				{
					RouteListItemStatus.Canceled,
					RouteListItemStatus.Overdue,
					RouteListItemStatus.Transfered
				})
				.And(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
				.And(() => !orderAlias.IsFastDelivery)
				.Select(Projections.Count(Projections.Id()));

			return itemsQuery
				.SelectList(list => list
					.Select(() => routeListAlias.Date).WithAlias(() => resultAlias.RouteListDate)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.SelectSubQuery(ownOrdersAmountSubquery).WithAlias(() => resultAlias.OwnOrdersCount)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.AdditionaLoadingNomenclature)
					.Select(() => additionalLoadingDocumentItemAlias.Amount).WithAlias(() => resultAlias.AdditionaLoadingAmount))
				.OrderBy(() => routeListAlias.Date).Desc
				.ThenBy(() => routeListAlias.Id).Desc
				.TransformUsing(Transformers.AliasToBean<FastDeliveryAdditionalLoadingReportRow>())
				.List<FastDeliveryAdditionalLoadingReportRow>();
		}

		#region Commands

		public DelegateCommand ExportCommand { get; }

		public DelegateCommand GenerateCommand { get; }

		public DelegateCommand GenerateFastDeliveryRemainingBottlesReportCommand { get; }

		#endregion Commands

		public DateTime? CreateDateFrom { get; set; }

		public DateTime? CreateDateTo { get; set; }

		[PropertyChangedAlso(nameof(IsHasRows))]
		public FastDeliveryAdditionalLoadingReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public string ProgressText
		{
			get
			{
				if(!IsHasDates)
				{
					return "Не был выбран период";
				}

				if(IsRunning)
				{
					return "Отчёт формируется...";
				}

				if(Report != null && !Report.Rows.Any())
				{
					return "По данному запросу отсутствуют записи";
				}

				if(IsHasRows)
				{
					return "Отчёт сформирован";
				}

				return "";
			}
		}

		[PropertyChangedAlso(nameof(ProgressText))]
		public bool IsRunning
		{
			get => _isRunning;
			set => SetField(ref _isRunning, value);
		}

		[PropertyChangedAlso(nameof(ProgressText))]
		public bool IsHasRows => Report != null && Report.Rows != null && Report.Rows.Any();

		public bool IsHasDates => CreateDateFrom != null && CreateDateTo != null;

		private void GenerateReport()
		{
			Report = null;
			IsRunning = true;

			Report = new FastDeliveryAdditionalLoadingReport
			{
				Rows = GenerateReportRows()
			};

			IsRunning = false;
			UoW.Session.Clear();
		}

		private void ExportReport()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"Отчёт по дозагрузке МЛ {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			var template = new XLTemplate(_templatePath);

			template.AddVariable(Report);
			template.Generate();
			template.SaveAs(result.Path);
		}

		private void GenerateFastDeliveryRemainingBottlesReport()
		{
			if(!CreateDateFrom.HasValue || !CreateDateTo.HasValue)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Не указан интервал", "Ошибка");
				return;
			}

			var reportName = typeof(FastDeliveryRemainingBottlesReport).GetClassUserFriendlyName().Nominative.CapitalizeSentence();

			var saveDialogSettings = new DialogSettings
			{
				Title = $"Сохранить {reportName}",
				DefaultFileExtention = ".xlsx",
				FileName = $"{reportName} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(saveDialogSettings);

			if(!saveDialogResult.Successful)
			{
				return;
			}

			var template = new XLTemplate(_fastDeliveryRemainingBottlesReportPath);

			var report = FastDeliveryRemainingBottlesReport.Generate(UoW, CreateDateFrom.Value, CreateDateTo.Value);

			template.AddVariable(report);
			template.Generate();
			template.SaveAs(saveDialogResult.Path);
		}
	}
}
