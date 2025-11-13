using ClosedXML.Report;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using RestSharp.Extensions;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public partial class DistrictSetDiffReportConfirmationViewModel : WindowDialogViewModelBase
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IInteractiveService _interactiveService;
		private string _sourceDistrictSetName;
		private string _targetDistrictSetName;

		public DistrictSetDiffReportConfirmationViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService,
			IInteractiveService interactiveService,
			INavigationManager navigation) : base(navigation)
		{
			_dialogSettingsFactory = dialogSettingsFactory
				?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			WindowPosition = WindowGravity.None;

			Title = "Сравнение версий районов";

			CloseCommand = new DelegateCommand<bool>(CloseHandler);
			GenerateDiffReportCommand = new DelegateCommand(GenerateDiffReport);
		}

		public event EventHandler<DistrictSetDiffReportConfirmationClosedArgs> Closed;

		public int? SourceDistrictSetId { get; set; }
		public int? TargetDistrictSetId { get; set; }

		public string SourceDistrictSetName
		{
			get => _sourceDistrictSetName;
			set => SetField(ref _sourceDistrictSetName, value);
		}

		public string TargetDistrictSetName
		{
			get => _targetDistrictSetName;
			set => SetField(ref _targetDistrictSetName, value);
		}

		public DelegateCommand<bool> CloseCommand { get; }
		public DelegateCommand GenerateDiffReportCommand { get; }

		private void GenerateDiffReport()
		{
			var reportName = typeof(DistrictsSetDiffReport).GetAttribute<AppellativeAttribute>().Nominative;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(reportName))
			{
				var reportResult = DistrictsSetDiffReport.Generate(unitOfWork, SourceDistrictSetId, TargetDistrictSetId);

				DistrictsSetDiffReport report = null;

				reportResult.Match(
					r => report = r,
					errors => _interactiveService.ShowMessage(
						ImportanceLevel.Error,
						string.Join("\n", errors.Select(e => e.Message)),
						"Ошибка при формировании отчета"));

				if(report is null)
				{
					return;
				}

				if(!report.DistrictRemoved.Any()
					&& !report.DistrictAdded.Any()
					&& !report.DistrictDiffs.Any())
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Info,
						$"В версиях районов {SourceDistrictSetId} и {TargetDistrictSetId} нет различий",
						"Нет данных для выгрузки");
					return;
				}

				var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(report);

				var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

				if(saveDialogResult.Successful)
				{
					var renderedTemplate = PostProcess(
						report.RenderTemplate(),
						report);

					renderedTemplate.Export(saveDialogResult.Path);
					CloseCommand.Execute(false);
				}
			}
		}

		private XLTemplate PostProcess(XLTemplate renderedTemplate, DistrictsSetDiffReport report)
		{
			if(!report.DistrictRemoved.Any())
			{
				renderedTemplate.Workbook.Worksheets.Delete(3);
			}

			if(!report.DistrictAdded.Any())
			{
				renderedTemplate.Workbook.Worksheets.Delete(2);
			}

			if(!report.DistrictDiffs.Any())
			{
				renderedTemplate.Workbook.Worksheets.Delete(1);
				return renderedTemplate;
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryRulesSpecialNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryRulesSpecialOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(29).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(28).Delete();
			}

			if(report.DistrictDiffs.All(x => !x.RegionChanged))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(27).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsSaturdayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsSaturdayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(26).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(25).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsSundayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsSundayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(24).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(23).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsFridayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsFridayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(22).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(21).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsThursdayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsThursdayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(20).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(19).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsWednesdayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsWednesdayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(18).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(17).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsTuesdayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsTuesdayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(16).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(15).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsMondayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsMondayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(14).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(13).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DeliveryShiftsTodayNew)
				&& string.IsNullOrWhiteSpace(x.DeliveryShiftsTodayOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(12).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(11).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.DelikveryRulesGeneralNew)
				&& string.IsNullOrWhiteSpace(x.DelikveryRulesGeneralOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(10).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(9).Delete();
			}

			if(report.DistrictDiffs.All(x => x.MinimalBottlesCountNew == null
				&& x.MinimalBottlesCountOld == null))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(8).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(7).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.GeoGroupNew)
				&& string.IsNullOrWhiteSpace(x.GeoGroupOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(6).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(5).Delete();
			}

			if(report.DistrictDiffs.All(x => string.IsNullOrWhiteSpace(x.TariffZoneNameNew)
				&& string.IsNullOrWhiteSpace(x.TariffZoneNameOld)))
			{
				renderedTemplate.Workbook.Worksheet(1).Column(4).Delete();
				renderedTemplate.Workbook.Worksheet(1).Column(3).Delete();
			}

			return renderedTemplate;
		}

		private void CloseHandler(bool canceled)
		{
			Close(false, CloseSource.Cancel);
			Closed?.Invoke(this, new DistrictSetDiffReportConfirmationClosedArgs { Canceled = canceled });
		}
	}
}
