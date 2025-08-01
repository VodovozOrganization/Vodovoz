using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Fuel;
using Vodovoz.Journals;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;
using VodovozInfrastructure.Utils;

namespace Vodovoz.ViewModels.ReportsParameters.Fuel
{
	public class FuelApiRequestReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private DateTime? _startDate;
		private DateTime? _endDate;
		private User _user;
		private FuelApiResponseResult? _fuelApiResponseResult;

		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly RdlViewerViewModel _rdlViewerViewModel;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ViewModelEEVMBuilder<User> _userViewModelEEVMBuilder;

		public FuelApiRequestReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ViewModelEEVMBuilder<User> userViewModelEEVMBuilder,
			IReportInfoFactory reportInfoFactory
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_rdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_userViewModelEEVMBuilder = userViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(userViewModelEEVMBuilder));

			Title = "Отчет по запросам к API Газпром-нефть";
			Identifier = "Cash.FuelApiRequestReport";

			_interactiveService = commonServices.InteractiveService;
			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			CreateReportCommand = new DelegateCommand(CreateReport, () => CanCreateReport);
			UserViewModel = GetUserViewModel();

			StartDate = GeneralUtils.GetCurrentMonthStartDate();
			EndDate = DateTime.Today;
		}

		public DelegateCommand CreateReportCommand { get; }
		public IEntityEntryViewModel UserViewModel { get; }

		[PropertyChangedAlso(nameof(CanCreateReport))]
		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[PropertyChangedAlso(nameof(CanCreateReport))]
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public User User
		{
			get => _user;
			set => SetField(ref _user, value);
		}

		public FuelApiResponseResult? FuelApiResponseResult
		{
			get => _fuelApiResponseResult;
			set => SetField(ref _fuelApiResponseResult, value);
		}

		public bool CanCreateReport => StartDate.HasValue && EndDate.HasValue;

		protected override Dictionary<string, object> Parameters =>
			new Dictionary<string, object>
			{
				{ "start_date", StartDate?.ToString("yyyy-MM-dd") },
				{ "end_date", EndDate?.ToString("yyyy-MM-dd") },
				{ "user_id", User?.Id ?? 0 },
				{ "response_result", FuelApiResponseResult?.ToString() ?? "0" }
			};

		private void CreateReport()
		{
			if(!CanCreateReport)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для формирования отчета необходимо указать период");
			}

			LoadReport();
		}

		private IEntityEntryViewModel GetUserViewModel()
		{
			var viewModel = _userViewModelEEVMBuilder
				.SetViewModel(_rdlViewerViewModel)
				.SetUnitOfWork(_unitOfWork)
				.ForProperty(this, x => x.User)
				.UseViewModelJournalAndAutocompleter<SelectUserJournalViewModel, UsersJournalFilterViewModel>(filter =>
				{
				})
				.Finish();

			return viewModel;
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
