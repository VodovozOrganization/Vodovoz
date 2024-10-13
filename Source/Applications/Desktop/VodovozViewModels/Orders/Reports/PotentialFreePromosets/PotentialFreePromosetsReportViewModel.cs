using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.ViewModels.Factories;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReportViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private PotentialFreePromosetsReport _report;
		private bool _isReportGenerationInProgress;

		public PotentialFreePromosetsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService)
			: base(navigation)
		{
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot(nameof(PotentialFreePromosetsReportViewModel));

			Title = "Отчет по потенциальным халявщикам";

			FillPromotionalSets();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
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
				.Get(_uow)
				.Select(ps => new PromosetNode
				{
					Id = ps.Id,
					Name = ps.Name,
					IsSelected = ps.PromotionalSetForNewClients
				})
				.ToList();
		}

		private void GenerateReport()
		{
			if(IsReportGenerationInProgress)
			{
				return;
			}

			IsReportGenerationInProgress = true;

			IsReportGenerationInProgress = false;
		}

		private void AbortReportGeneration()
		{
			if(!IsReportGenerationInProgress)
			{
				return;
			}

			IsReportGenerationInProgress &= !IsReportGenerationInProgress;
		}

		private void SaveReport()
		{

		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
