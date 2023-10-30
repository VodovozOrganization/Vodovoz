using Microsoft.Extensions.Logging;
using MoreLinq;
using NHibernate;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Services;
using static Vodovoz.ViewModels.Counterparties.ClientClassification.CounterpartyClassificationCalculationEmailSettingsViewModel;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationViewModel : DialogTabViewModelBase
	{
		private const int _insertQueryElementsMaxCount = 10_000;

		private readonly IUnitOfWork _uow;
		private readonly ILogger<CounterpartyClassificationCalculationViewModel> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly IUserService _userService;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private bool _isCalculationInProcess;
		private bool _isCalculationCompleted;
		private string _currentUserEmail;
		private string _additionalEmail;
		private DateTime _creationDate;

		public CounterpartyClassificationCalculationViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILogger<CounterpartyClassificationCalculationViewModel> logger,
			IEmployeeService employeeService,
			IUserService userService,
			ICounterpartyRepository counterpartyRepository
			) : base(uowFactory, interactiveService, navigation)
		{
			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_uow = uowFactory.CreateWithoutRoot();

			_creationDate = DateTime.Now;

			Title = "Пересчёт классификации контрагентов";

			CreateCalculationSettings();
			GetCurrentUserEmail();
		}

		#region Properties

		public event EventHandler<CalculationCompletedEventArgs> CalculationCompleted;

		public CounterpartyClassificationCalculationSettings CalculationSettings { get; private set; }

		[PropertyChangedAlso(nameof(CanOpenEmailSettingsDialog), nameof(CanCancel), nameof(CanSaveReport), nameof(CanQuite))]
		public bool IsCalculationInProcess
		{
			get => _isCalculationInProcess;
			set => SetField(ref _isCalculationInProcess, value);
		}

		[PropertyChangedAlso(nameof(CanOpenEmailSettingsDialog), nameof(CanCancel), nameof(CanSaveReport), nameof(CanQuite))]
		public bool IsCalculationCompleted
		{
			get => _isCalculationCompleted;
			set => SetField(ref _isCalculationCompleted, value);
		}

		#endregion Properties

		private void CreateCalculationSettings()
		{
			CalculationSettings = new CounterpartyClassificationCalculationSettings();

			var lastSettings = _uow.GetAll<CounterpartyClassificationCalculationSettings>()
				.OrderByDescending(x => x.SettingsCreationDate)
				.FirstOrDefault();

			if(lastSettings != null)
			{
				CalculationSettings.PeriodInMonths = lastSettings.PeriodInMonths;
				CalculationSettings.BottlesCountAClassificationFrom = lastSettings.BottlesCountAClassificationFrom;
				CalculationSettings.BottlesCountCClassificationTo = lastSettings.BottlesCountCClassificationTo;
				CalculationSettings.OrdersCountXClassificationFrom = lastSettings.OrdersCountXClassificationFrom;
				CalculationSettings.OrdersCountZClassificationTo = lastSettings.OrdersCountZClassificationTo;
			}
		}

		private void GetCurrentUserEmail()
		{
			var currentEmployee = _employeeService.GetEmployeeForUser(_uow, _userService.CurrentUserId);

			_currentUserEmail = currentEmployee?.Email ?? string.Empty;
		}

		private async void OnEmailSettingsDialogStartClassificationCalculationClicked(object sender, StartClassificationCalculationEventArgs e)
		{
			_creationDate = DateTime.Now;
			_currentUserEmail = e.CurrentUserEmail;
			_additionalEmail = e.AdditionalEmail;

			var task = Task.Run(() =>
			{
				StartClassificationCalculation();
			});

			await task;
		}

		private void StartClassificationCalculation()
		{
			bool isCalculationSuccessful = false;

			IsCalculationInProcess = true;

			UpdateCalculationSettingsCreationDate();

			var allCounterpartiesIdsAndNames = _counterpartyRepository
				.GetAllCounterpartyIdsAndNames(_uow);

			var oldCounterpartyClassifications = _counterpartyRepository
				.GetLastExistingClassificationsForCounterparties(_uow);

			var newCounterpartyClassifications = _counterpartyRepository
				.CalculateCounterpartyClassifications(_uow, CalculationSettings)
				.ToDictionary(c => c.CounterpartyId);

			try
			{
				using(var transaction = _uow.Session.BeginTransaction())
				{
					InsertClassificationValuesToDatabase(_uow.Session, newCounterpartyClassifications, _insertQueryElementsMaxCount);

					_uow.Save(CalculationSettings);

					ClassificationCalculationReport.GenerateReport(
						newCounterpartyClassifications,
						oldCounterpartyClassifications,
						allCounterpartiesIdsAndNames,
						CalculationSettings.PeriodInMonths);

					transaction.Commit();
				}

				isCalculationSuccessful = true;
				IsCalculationInProcess = false;
				IsCalculationCompleted = true;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);

				return;
			}
			finally
			{
				CalculationCompleted?.Invoke(
					this,
					 new CalculationCompletedEventArgs { IsCalculationSuccessful = isCalculationSuccessful });
			}
		}

		private void InsertClassificationValuesToDatabase(
			ISession session,
			IDictionary<int, CounterpartyClassification> classifications,
			int insertQueryElementsMaxCount)
		{
			for(int i = 0; i < classifications.Values.Count; i += insertQueryElementsMaxCount)
			{
				var classificationsSubset = classifications.Values
					.Skip(i)
					.Take(insertQueryElementsMaxCount)
					.ToList();

				var sql = GetSqlInsertQuery(classificationsSubset);

				session.CreateSQLQuery(sql)
					.ExecuteUpdate();
			}
		}

		private string GetSqlInsertQuery(IEnumerable<CounterpartyClassification> classifications)
		{
			var valuesData = new List<string>();

			foreach(var classification in classifications)
			{
				var c = classification;

				valuesData.Add($"({c.CounterpartyId}, " +
					$"'{c.ClassificationByBottlesCount}', " +
					$"'{c.ClassificationByOrdersCount}', " +
					$"{c.BottlesPerMonthAverageCount.ToString(CultureInfo.InvariantCulture)}, " +
					$"{c.OrdersPerMonthAverageCount.ToString(CultureInfo.InvariantCulture)}, " +
					$"{c.MoneyTurnoverPerMonthAverageSum.ToString(CultureInfo.InvariantCulture)}, " +
					$"'{c.ClassificationCalculationDate.ToString("yyyy-MM-dd HH:mm:ss")}') ");
			}

			var insertQuery = $"INSERT INTO counterparty_classification " +
				$"(counterparty_id, " +
				$"classification_by_bottles_count, " +
				$"classification_by_orders_count, " +
				$"bottles_per_month_average_count, " +
				$"orders_per_month_average_count, " +
				$"money_turnover_per_month_average_sum, " +
				$"calculation_date) " +
				$"VALUES ";

			return $"{insertQuery} {string.Join(",", valuesData)};";
		}

		private void UpdateCalculationSettingsCreationDate()
		{
			CalculationSettings.SettingsCreationDate = _creationDate;
		}

		#region Commands

		#region OpenEmailSettingsDialog
		private DelegateCommand _openEmailSettingsDialogCommand;
		public DelegateCommand OpenEmailSettingsDialogCommand
		{
			get
			{
				if(_openEmailSettingsDialogCommand == null)
				{
					_openEmailSettingsDialogCommand = new DelegateCommand(OpenEmailSettingsDialog, () => CanOpenEmailSettingsDialog);
					_openEmailSettingsDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenEmailSettingsDialog);
				}
				return _openEmailSettingsDialogCommand;
			}
		}

		public bool CanOpenEmailSettingsDialog => !IsCalculationInProcess && !IsCalculationCompleted;

		private void OpenEmailSettingsDialog()
		{
			var emailSettingsDialog =
				NavigationManager.OpenViewModel<CounterpartyClassificationCalculationEmailSettingsViewModel, string>(this, _currentUserEmail)
				.ViewModel;

			emailSettingsDialog.StartClassificationCalculationClicked += OnEmailSettingsDialogStartClassificationCalculationClicked;
		}
		#endregion OpenEmailSettingsDialog

		#region Cancel
		private DelegateCommand _cancelCommand;
		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(Cancel, () => CanCancel);
					_cancelCommand.CanExecuteChangedWith(this, x => x.CanCancel);
				}
				return _cancelCommand;
			}
		}

		public bool CanCancel => IsCalculationInProcess && !IsCalculationCompleted;

		private void Cancel()
		{

		}
		#endregion Cancel

		#region SaveReport
		private DelegateCommand _saveReportCommand;
		public DelegateCommand SaveReportCommand
		{
			get
			{
				if(_saveReportCommand == null)
				{
					_saveReportCommand = new DelegateCommand(SaveReport, () => CanSaveReport);
					_saveReportCommand.CanExecuteChangedWith(this, x => x.CanSaveReport);
				}
				return _saveReportCommand;
			}
		}

		public bool CanSaveReport => !IsCalculationInProcess && IsCalculationCompleted;

		private void SaveReport()
		{

		}
		#endregion SaveReport

		#region Quite
		private DelegateCommand _quiteCommand;
		public DelegateCommand QuiteCommand
		{
			get
			{
				if(_quiteCommand == null)
				{
					_quiteCommand = new DelegateCommand(Quite, () => CanQuite);
					_quiteCommand.CanExecuteChangedWith(this, x => x.CanQuite);
				}
				return _quiteCommand;
			}
		}

		public bool CanQuite => !IsCalculationInProcess && IsCalculationCompleted;

		private void Quite()
		{
			this.Close(false, CloseSource.Self);
		}
		#endregion Quite


		#endregion Commands

		#region IDisposable implementation
		public override void Dispose()
		{
			_uow?.Dispose();

			base.Dispose();
		}
		#endregion IDisposable implementation

		public class CalculationCompletedEventArgs : EventArgs
		{
			public bool IsCalculationSuccessful;
		}
	}
}
