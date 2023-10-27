using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using static Vodovoz.ViewModels.Counterparties.ClientClassification.CounterpartyClassificationCalculationEmailSettingsViewModel;

namespace Vodovoz.ViewModels.Counterparties.ClientClassification
{
	public class CounterpartyClassificationCalculationViewModel : DialogTabViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IInteractiveService _interactiveService;
		private readonly ILogger<CounterpartyClassificationCalculationViewModel> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly IUserService _userService;
		private bool _isCalculationInProcess;
		private bool _isCalculationCompleted;
		private string _currentUserEmail;
		private string _additionalEmail;

		public CounterpartyClassificationCalculationViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILogger<CounterpartyClassificationCalculationViewModel> logger,
			IEmployeeService employeeService,
			IUserService userService
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();

			Title = "Пересчёт классификации контрагентов";

			CreateCalculationSettings();
			GetCurrentUserEmail();
		}

		#region Properties

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

			var lastSettings = _unitOfWork.GetAll<CounterpartyClassificationCalculationSettings>()
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
			var currentEmployee = _employeeService.GetEmployeeForUser(_unitOfWork, _userService.CurrentUserId);

			_currentUserEmail = currentEmployee?.Email ?? string.Empty;
		}

		private void OnEmailSettingsDialogStartClassificationCalculationClicked(object sender, StartClassificationCalculationEventArgs e)
		{
			_currentUserEmail = e.CurrentUserEmail;
			_additionalEmail = e.AdditionalEmail;

			IsCalculationInProcess = true;

			UpdateCalculationSettingsCreationDate();

			var allCounterpartiesIds = GetAllCounterpartiesIds(_unitOfWork).ToList();
			var classificationsByOrders = GetClassificationsByOrdersPerCounterparty(_unitOfWork).ToList();

			foreach(var classification in classificationsByOrders)
			{
				classification.ClassificationByBottlesCount = GetClassificationByBottlesCount(classification.BottlesPerMonthAverageCount);
				classification.ClassificationByOrdersCount = GetClassificationByOrdersCount(classification.OrdersPerMonthAverageCount);
				classification.ClassificationCalculationDate = DateTime.Now;

				if(classification.CounterpartyId < 100)
				{
					_unitOfWork.Save(classification);
				}

				//_unitOfWork.Save(classification);
			}

			_unitOfWork.Save(CalculationSettings);
			_unitOfWork.Commit();

			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				$"Пересчёт классификации контрагентов завершен"
				);

			IsCalculationInProcess = false;
			IsCalculationCompleted = true;
		}

		private IQueryable<int> GetAllCounterpartiesIds(IUnitOfWork unitOfWork)
		{
			var ids = unitOfWork.GetAll<Counterparty>()
				.Select(c => c.Id);

			return ids;
		}

		private IQueryable<CounterpartyClassification> GetClassificationsByOrdersPerCounterparty(IUnitOfWork unitOfWork)
		{
			var dateFrom = DateTime.Now.Date.AddMonths(-CalculationSettings.PeriodInMonths);
			var dateTo = DateTime.Now.Date.AddDays(1);

			var ordersPerCounterparty = from o in unitOfWork.Session.Query<Order>()
										join oi in unitOfWork.GetAll<OrderItem>() on o.Id equals oi.Order.Id
										join n in unitOfWork.GetAll<Nomenclature>() on oi.Nomenclature.Id equals n.Id
										where
											o.DeliveryDate < dateTo
											&& o.DeliveryDate >= dateFrom
											&& o.OrderStatus == OrderStatus.Closed

										group new { Order = o, Item = oi, Nomenclature = n } by new { CleintId = o.Client.Id } into clientsGroups

										select new CounterpartyClassification
										{
											CounterpartyId = clientsGroups.Key.CleintId,

											BottlesPerMonthAverageCount =
												clientsGroups.Sum(data =>
													data.Nomenclature.Category == NomenclatureCategory.water && data.Nomenclature.TareVolume == TareVolume.Vol19L
													? data.Item.Count
													: 0) / CalculationSettings.PeriodInMonths,

											OrdersPerMonthAverageCount =
												(decimal)(clientsGroups.Select(data => data.Order.Id).Distinct().Count()) / CalculationSettings.PeriodInMonths,

											MoneyTurnoverPerMonthAverageSum =
												clientsGroups.Sum(data => (data.Item.ActualCount ?? data.Item.Count) * data.Item.Price - data.Item.DiscountMoney) / CalculationSettings.PeriodInMonths
										};

			return ordersPerCounterparty;
		}

		private CounterpartyClassificationByBottlesCount GetClassificationByBottlesCount(decimal bottlesPerMonthAverageCount)
		{
			if(bottlesPerMonthAverageCount <= CalculationSettings.BottlesCountCClassificationTo)
			{
				return CounterpartyClassificationByBottlesCount.C;
			}

			if(bottlesPerMonthAverageCount >= CalculationSettings.BottlesCountAClassificationFrom)
			{
				return CounterpartyClassificationByBottlesCount.A;
			}

			return CounterpartyClassificationByBottlesCount.B;
		}

		private CounterpartyClassificationByOrdersCount GetClassificationByOrdersCount(decimal ordersPerMonthAverageCount)
		{
			if(ordersPerMonthAverageCount <= CalculationSettings.OrdersCountZClassificationTo)
			{
				return CounterpartyClassificationByOrdersCount.Z;
			}

			if(ordersPerMonthAverageCount >= CalculationSettings.OrdersCountXClassificationFrom)
			{
				return CounterpartyClassificationByOrdersCount.X;
			}

			return CounterpartyClassificationByOrdersCount.Y;
		}

		private void UpdateCalculationSettingsCreationDate()
		{
			CalculationSettings.SettingsCreationDate = DateTime.Now;
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


		#endregion

		#region IDisposable implementation
		public override void Dispose()
		{
			_unitOfWork?.Dispose();

			base.Dispose();
		}
		#endregion
	}
}
