using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.ReportsParameters.Payments
{
	public class PaymentsFromBankClientReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly RdlViewerViewModel _rdlViewerViewModel;
		private ILifetimeScope _lifetimeScope;

		private Counterparty _counterparty;
		private bool _allSubdivisions;
		private Subdivision _subdivision;
		private bool _sortByDate;

		public PaymentsFromBankClientReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IUserRepository userRepository,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			IReportInfoFactory reportInfoFactory
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			_interactiveService = commonServices.InteractiveService;
			_rdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			Title = "Отчет по оплатам";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			CounterpartySelectorFactory = counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope);
			var currentUserSettings = userRepository.GetUserSettings(_unitOfWork, commonServices.UserService.CurrentUserId);
			if(currentUserSettings.DefaultCounterpartyId != null)
			{
				Counterparty = _unitOfWork.GetById<Counterparty>(currentUserSettings.DefaultCounterpartyId.Value);
			}
			SubdivisionViewModel = CreateSubdivisionViewModel();
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}


		public Subdivision Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		public bool AllSubdivisions
		{
			get => _allSubdivisions;
			set => SetField(ref _allSubdivisions, value);
		}

		public bool SortByDate
		{
			get => _sortByDate;
			set => SetField(ref _sortByDate, value);
		}

		public INavigationManager NavigationManager { get; }

		public IEntityEntryViewModel SubdivisionViewModel { get; }

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public override string Identifier => _allSubdivisions 
			? "Payments.PaymentsFromBankClientAllSubdivisionsReport" 
			: "Payments.PaymentsFromBankClientBySubdivisionReport";

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
			{ "counterparty_id", Counterparty?.Id ?? 0 },
			{ "subdivision_id", Subdivision?.Id },
			{ "sort_date", SortByDate },
			{ "date", DateTime.Today }
		};

		public void Dispose()
		{
			_lifetimeScope = null;
			_unitOfWork?.Dispose();
		}

		public bool Validate()
		{
			string errorString = string.Empty;

			if(Subdivision == null && !AllSubdivisions)
			{
				errorString += "Не заполнено подразделение!\n";
			}

			if(Subdivision != null && AllSubdivisions)
			{
				errorString += "Данные установки протеворечат логике работы!\n";
			}

			if(!string.IsNullOrWhiteSpace(errorString))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, errorString);
				return false;
			}
			return true;
		}

		private IEntityEntryViewModel CreateSubdivisionViewModel()
		{
			return new CommonEEVMBuilderFactory<PaymentsFromBankClientReportViewModel>(_rdlViewerViewModel, this, _unitOfWork, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}
	}
}
