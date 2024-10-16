﻿using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using static Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets.PotentialFreePromosetsReport;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReportViewModel : DialogTabViewModelBase, IDisposable, ITDICloseControlTab
	{
		private readonly ILogger<PotentialFreePromosetsReportViewModel> _logger;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private PotentialFreePromosetsReport _report;
		private bool _isReportGenerationInProgress;
		private CancellationTokenSource _cancellationTokenSource;

		public PotentialFreePromosetsReportViewModel(
			ILogger<PotentialFreePromosetsReportViewModel> logger,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService)
			: base(uowFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			Title = "Отчет по потенциальным халявщикам";

			FillPromotionalSets();

			GenerateReportCommand = new DelegateCommand(async () => await GenerateReport());
			AbortReportGenerationCommand = new DelegateCommand(AbortReportGeneration);
			SaveReportCommand = new DelegateCommand(SaveReport);
		}

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand AbortReportGenerationCommand { get; }
		public DelegateCommand SaveReportCommand { get; }

		public IEnumerable<PromosetNode> PromotionalSets { get; private set; }

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public PotentialFreePromosetsReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public bool IsReportGenerationInProgress
		{
			get => _isReportGenerationInProgress;
			set => SetField(ref _isReportGenerationInProgress, value);
		}

		private void FillPromotionalSets()
		{
			PromotionalSets =
				_promotionalSetRepository
				.Get(UoW)
				.Select(ps => new PromosetNode
				{
					Id = ps.Id,
					Name = ps.Name,
					IsSelected = ps.PromotionalSetForNewClients
				})
				.ToList();
		}

		private async Task GenerateReport()
		{
			if(IsReportGenerationInProgress)
			{
				return;
			}

			if(StartDate is null || EndDate is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Необходимо ввести полный период");

				return;
			}

			if(_cancellationTokenSource != null)
			{
				return;
			}

			var selectedPromosets = GetSelectedPromotionalSets();

			_logger.LogInformation(
				"Формируем отчет по потенциальным халявщикам.\nStartDate^ {StartDate}, EndDate: {EndDate}, Promosets: {Promosets}",
				StartDate.Value,
				EndDate.Value,
				string.Join(", ", selectedPromosets));

			IsReportGenerationInProgress = true;

			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				var report = await Create(
					UoW,
					StartDate.Value,
					EndDate.Value,
					selectedPromosets,
					_cancellationTokenSource.Token);

				Report = report;

				_logger.LogInformation(
					"Отчет по потенциальным халявщикам успешно сформирован");
			}
			catch(OperationCanceledException ex)
			{
				Report = null;

				var message = "Формирование отчета было прервано вручную";

				LogErrorAndShowMessageInGuiThread(ex, message);
			}
			catch(Exception ex)
			{
				Report = null;

				var message = $"При формировании отчета возникла ошибка:\n{ex.Message}";

				LogErrorAndShowMessageInGuiThread(ex, message);

			}
			finally
			{
				IsReportGenerationInProgress = false;

				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}
		}

		private void AbortReportGeneration()
		{
			if(!IsReportGenerationInProgress
				|| _cancellationTokenSource is null
				|| _cancellationTokenSource.IsCancellationRequested)
			{
				return;
			}

			_cancellationTokenSource.Cancel();
		}

		private void SaveReport()
		{
			if(Report is null)
			{
				return;
			}

			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(_report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				_report.RenderTemplate().Export(saveDialogResult.Path);
			}
		}

		private IEnumerable<int> GetSelectedPromotionalSets()
		{
			if(PromotionalSets.Any(x => x.IsSelected))
			{
				return PromotionalSets.Where(x => x.IsSelected).Select(x => x.Id);
			}

			return PromotionalSets.Select(x => x.Id);
		}

		private void LogErrorAndShowMessageInGuiThread(Exception ex, string message)
		{
			_logger.LogError(ex, message);

			_guiDispatcher.RunInGuiTread(() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, message);
			});
		}

		public bool CanClose()
		{
			if(!IsReportGenerationInProgress)
			{
				return true;
			}

			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				   "Формирование отчета в процессе.\n\n" +
				   "Чтобы закрыть вкладку необходимо либо дождаться результата формирования отчета,\n" +
				   "либо отменить его выполнение");

			return false;
		}

		public override void Dispose()
		{
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			base.Dispose();
		}
	}
}
