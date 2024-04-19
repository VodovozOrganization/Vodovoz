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
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class DistrictSetDiffReportConfirmationViewModel : WindowDialogViewModelBase
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

			CloseCommand = new DelegateCommand(CloseHandler);
			GenerateDiffReportCommand = new DelegateCommand(GenerateDiffReport);
		}

		public event EventHandler Closed;

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

		public DelegateCommand CloseCommand { get; }
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

				var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(report);

				var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

				if(saveDialogResult.Successful)
				{
					report.Export(saveDialogResult.Path);
					CloseCommand.Execute();
				}
			}
		}

		private void CloseHandler()
		{
			Close(false, CloseSource.Cancel);
			Closed?.Invoke(this, EventArgs.Empty);
		}
	}
}
