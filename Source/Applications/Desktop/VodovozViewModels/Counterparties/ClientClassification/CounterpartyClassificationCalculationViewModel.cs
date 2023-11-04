using Microsoft.Extensions.Logging;
using MoreLinq;
using NHibernate;
using NHibernate.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Parameters;
using Vodovoz.Services;
using static Vodovoz.ViewModels.Counterparties.ClientClassification.CounterpartyClassificationCalculationEmailSettingsViewModel;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public partial class CounterpartyClassificationCalculationViewModel : DialogTabViewModelBase
	{
		private const int _insertQueryElementsMaxCount = 10_000;

		private readonly IUnitOfWork _uow;
		private readonly IInteractiveService _interactiveService;
		private readonly ILogger<CounterpartyClassificationCalculationViewModel> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly IUserService _userService;
		private readonly IFileDialogService _fileDialogService;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private bool _isCalculationInProcess;
		private bool _isCalculationCompleted;
		private string _currentUserName;
		private string _currentUserEmail;
		private string _additionalEmail;
		private DateTime _creationDate;
		private double _calculationProgressValue;
		private byte[] _reportData;

		private DelegateCommand _openEmailSettingsDialogCommand;
		private DelegateCommand _cancelCommand;
		private DelegateCommand _saveReportCommand;
		private DelegateCommand _quiteCommand;
		private DelegateCommand _updatePropertiesAfterCancellationCommand;
		private DelegateCommand _updatePropertiesAfterExceptionCommand;

		public CounterpartyClassificationCalculationViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILogger<CounterpartyClassificationCalculationViewModel> logger,
			IEmployeeService employeeService,
			IUserService userService,
			IFileDialogService fileDialogService,
			ICounterpartyRepository counterpartyRepository,
			IEmailParametersProvider emailParametersProvider
			) : base(uowFactory, interactiveService, navigation)
		{
			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));

			_uow = uowFactory.CreateWithoutRoot();

			_creationDate = DateTime.Now;

			Title = "Пересчёт классификации контрагентов";

			CreateCalculationSettings();
			GetCurrentUserEmail();
		}

		#region Properties

		public event EventHandler CommandToStartCalculationReceived;

		public CounterpartyClassificationCalculationSettings CalculationSettings { get; private set; }
		public CancellationTokenSource ReportCancelationTokenSource { get; set; }
		public IInteractiveService InteractiveService => _interactiveService;

		public string ProgressInfoLabelValue =>
			IsCalculationCompleted
			? "Пересчёт завершен, отчет\nбыл отправлен Вам на почту"
			: "Пересчёт займет некоторое\nвремя, не закрывайте окно до завершения";

		[PropertyChangedAlso(
			nameof(CanOpenEmailSettingsDialog),
			nameof(CanCancel),
			nameof(CanSaveReport),
			nameof(CanQuite))]
		public bool IsCalculationInProcess
		{
			get => _isCalculationInProcess;
			set => SetField(ref _isCalculationInProcess, value);
		}

		[PropertyChangedAlso(
			nameof(CanOpenEmailSettingsDialog),
			nameof(CanCancel),
			nameof(CanSaveReport),
			nameof(CanQuite),
			nameof(ProgressInfoLabelValue))]
		public bool IsCalculationCompleted
		{
			get => _isCalculationCompleted;
			set => SetField(ref _isCalculationCompleted, value);
		}

		public double CalculationProgressValue
		{
			get => _calculationProgressValue;
			set => SetField(ref _calculationProgressValue, value);
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
			_currentUserName = currentEmployee?.Name ?? string.Empty;
		}

		private void OnEmailSettingsDialogStartClassificationCalculationClicked(object sender, StartClassificationCalculationEventArgs e)
		{
			_creationDate = DateTime.Now;

			_currentUserEmail = e.CurrentUserEmail;
			_additionalEmail = e.AdditionalEmail;

			CommandToStartCalculationReceived?.Invoke(this, e);
		}

		public async Task StartClassificationCalculation(CancellationToken cancellationToken)
		{
			IsCalculationInProcess = true;
			CalculationProgressValue = 0;
			_reportData = null;

			UpdateCalculationSettingsCreationDate();

			var calculatedClassificationsForCounterpartiesWithOrders = await _counterpartyRepository
					.CalculateCounterpartyClassifications(_uow, CalculationSettings)
					.ToListAsync(cancellationToken);

			CalculationProgressValue = 20;

			var newClassificationsForAllCounterparties = await GetNewClassificationsForAllCounterparties(
				_uow,
				calculatedClassificationsForCounterpartiesWithOrders,
				cancellationToken);

			CalculationProgressValue = 40;

			var report = await ClassificationCalculationReport.CreateReport(
				_uow,
				_counterpartyRepository,
				newClassificationsForAllCounterparties,
				CalculationSettings.PeriodInMonths,
				cancellationToken);

			CalculationProgressValue = 60;

			using(var transaction = _uow.Session.BeginTransaction())
			{
				InsertClassificationValuesToDatabase(_uow.Session,
					newClassificationsForAllCounterparties,
					_insertQueryElementsMaxCount,
					cancellationToken);

				CalculationProgressValue = 90;

				cancellationToken.ThrowIfCancellationRequested();

				_uow.Save(CalculationSettings);

				cancellationToken.ThrowIfCancellationRequested();

				transaction.Commit();
			}

			_reportData = report.Export();

			if(!string.IsNullOrEmpty(_currentUserEmail)
				|| !string.IsNullOrEmpty(_additionalEmail))
			{
				if(_reportData?.Length == 0)
				{
					InteractiveService.ShowMessage(ImportanceLevel.Error,"Ошибка отправки отчета. Данные отсутствуют.");
				}
				else
				{
					_employeeService.SendCounterpartyClassificationCalculationReportToEmail(
						_uow,
						_emailParametersProvider,
						_currentUserName,
						_additionalEmail,
						_reportData);
				}
			}

			CalculationProgressValue = 100;

			IsCalculationInProcess = false;
			IsCalculationCompleted = true;
		}

		private async Task<IEnumerable<CounterpartyClassification>> GetNewClassificationsForAllCounterparties(
			IUnitOfWork uow,
			IEnumerable<CounterpartyClassification> calculatedClassifications,
			CancellationToken cancellationToken)
		{
			var allCounterpartyIds = await uow.GetAll<Counterparty>().Select(co => co.Id).ToListAsync(cancellationToken);

			var classificationForAllCounterparties =
				from counterpartyId in allCounterpartyIds
				join classification in calculatedClassifications on counterpartyId equals classification.CounterpartyId into classifications
				from counterpartyClassification in classifications.DefaultIfEmpty()
				select counterpartyClassification ??
				new CounterpartyClassification
				{
					CounterpartyId = counterpartyId,
					ClassificationCalculationDate = _creationDate
				};

			return classificationForAllCounterparties;
		}

		private void InsertClassificationValuesToDatabase(
			ISession session,
			IEnumerable<CounterpartyClassification> classifications,
			int insertQueryElementsMaxCount,
			CancellationToken cancellationToken)
		{
			var classificationsCount = classifications.Count();

			for(int i = 0; i < classificationsCount; i += insertQueryElementsMaxCount)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var classificationsSubset = classifications
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

			var insertQuery = @"INSERT INTO counterparty_classification 
				(counterparty_id, 
				classification_by_bottles_count, 
				classification_by_orders_count, 
				bottles_per_month_average_count, 
				orders_per_month_average_count, 
				money_turnover_per_month_average_sum, 
				calculation_date) 
				VALUES ";

			return $"{insertQuery} {string.Join(",", valuesData)};";
		}

		private void UpdateCalculationSettingsCreationDate()
		{
			CalculationSettings.SettingsCreationDate = _creationDate;
		}

		#region Commands		

		#region OpenEmailSettingsDialog
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

		public bool CanOpenEmailSettingsDialog => !IsCalculationInProcess;

		private void OpenEmailSettingsDialog()
		{
			var emailSettingsDialog =
				NavigationManager.OpenViewModel<CounterpartyClassificationCalculationEmailSettingsViewModel, string>(this, _currentUserEmail)
				.ViewModel;

			emailSettingsDialog.StartClassificationCalculationClicked += OnEmailSettingsDialogStartClassificationCalculationClicked;
		}
		#endregion OpenEmailSettingsDialog

		#region Cancel
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
			ReportCancelationTokenSource?.Cancel();
		}
		#endregion Cancel

		#region SaveReport
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
			if(_reportData == null || _reportData.Length == 0)
			{
				InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Отсутствую данные для сохранения. Возможно, отчет не сформирован!");

				return;
			}

			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{Title} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var saveDialogResul = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!saveDialogResul.Successful)
			{
				return;
			}

			File.WriteAllBytes(saveDialogResul.Path, _reportData);
		}
		#endregion SaveReport

		#region Quite
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

		public bool CanQuite => !IsCalculationInProcess;

		private void Quite()
		{
			this.Close(false, CloseSource.Self);
		}
		#endregion Quite

		#region UpdatePropertiesAfterCancellation
		public DelegateCommand UpdatePropertiesAfterCancellationCommand
		{
			get
			{
				if(_updatePropertiesAfterCancellationCommand == null)
				{
					_updatePropertiesAfterCancellationCommand = new DelegateCommand(UpdatePropertiesAfterCancellation, () => CanUpdatePropertiesAfterCancellation);
					_updatePropertiesAfterCancellationCommand.CanExecuteChangedWith(this, x => x.CanUpdatePropertiesAfterCancellation);
				}
				return _updatePropertiesAfterCancellationCommand;
			}
		}

		public bool CanUpdatePropertiesAfterCancellation => true;

		private void UpdatePropertiesAfterCancellation()
		{
			IsCalculationInProcess = false;
			IsCalculationCompleted = false;
		}
		#endregion UpdatePropertiesAfterCancellation

		#region UpdatePropertiesAfterException
		public DelegateCommand UpdatePropertiesAfterExceptionCommand
		{
			get
			{
				if(_updatePropertiesAfterExceptionCommand == null)
				{
					_updatePropertiesAfterExceptionCommand = new DelegateCommand(UpdatePropertiesAfterException, () => CanUpdatePropertiesAfterException);
					_updatePropertiesAfterExceptionCommand.CanExecuteChangedWith(this, x => x.CanUpdatePropertiesAfterException);
				}
				return _updatePropertiesAfterExceptionCommand;
			}
		}

		public bool CanUpdatePropertiesAfterException => true;

		private void UpdatePropertiesAfterException()
		{
			IsCalculationInProcess = false;
			IsCalculationCompleted = false;
		}
		#endregion UpdatePropertiesAfterException

		#endregion Commands

		#region IDisposable implementation
		public override void Dispose()
		{
			_uow?.Dispose();

			base.Dispose();
		}
		#endregion IDisposable implementation
	}
}
