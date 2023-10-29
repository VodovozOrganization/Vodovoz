using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
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
	public partial class CounterpartyClassificationCalculationViewModel : DialogTabViewModelBase
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
		private DateTime _creationDate;

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

			_creationDate = DateTime.Now;

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
			_creationDate = DateTime.Now;
			_currentUserEmail = e.CurrentUserEmail;
			_additionalEmail = e.AdditionalEmail;

			IsCalculationInProcess = true;

			UpdateCalculationSettingsCreationDate();

			var allCounterpartiesIdsNames = GetAllCounterpartyIdsNames(_unitOfWork);

			var oldCounterpartyClassifications = GetLastExistingClassificationsForCounterparties(_unitOfWork)
				.ToDictionary(c => c.CounterpartyId);

			var newCounterpartyClassifications =
				CalculateCounterpartyClassifications(_unitOfWork, CalculationSettings)
				.ToDictionary(c => c.CounterpartyId);

			foreach(var counterpartyId in allCounterpartiesIdsNames.Keys)
			{
				bool counterpartyHasNewCaculatedClassification = newCounterpartyClassifications.ContainsKey(counterpartyId);

				if(!counterpartyHasNewCaculatedClassification)
				{
					var classification = new CounterpartyClassification(
						counterpartyId,
						0,
						0,
						0,
						_creationDate,
						CalculationSettings);

					newCounterpartyClassifications.Add(counterpartyId, classification);
				}
			}

			foreach(var classification in newCounterpartyClassifications)
			{

				//if(classification.Value.CounterpartyId < 100)
				//{
				//	_unitOfWork.Save(classification.Value);
				//}

				//_unitOfWork.Session.Save(classification.Value);
			}

			ClassificationCalculationReport.GenerateReport(
				newCounterpartyClassifications,
				oldCounterpartyClassifications,
				allCounterpartiesIdsNames);

			_unitOfWork.Save(CalculationSettings);

			_unitOfWork.Commit();

			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				$"Пересчёт классификации контрагентов завершен"
				);

			IsCalculationInProcess = false;
			IsCalculationCompleted = true;
		}

		private IDictionary<int, string> GetAllCounterpartyIdsNames(IUnitOfWork unitOfWork)
		{
			return (from c in unitOfWork.GetAll<Counterparty>()
					select new { c.Id, c.Name })
					.ToDictionary(c => c.Id, c => c.Name);
		}

		private IQueryable<CounterpartyClassification> GetLastExistingClassificationsForCounterparties(IUnitOfWork unitOfWork)
		{
			return unitOfWork.GetAll<CounterpartyClassification>()
				.OrderByDescending(c => c.ClassificationCalculationDate)
				.DistinctBy(c => c.CounterpartyId)
				.AsQueryable();
		}

		private IQueryable<CounterpartyClassification> CalculateCounterpartyClassifications(
			IUnitOfWork unitOfWork,
			CounterpartyClassificationCalculationSettings calculationSettings)
		{
			var dateFrom = _creationDate.Date.AddMonths(-calculationSettings.PeriodInMonths);
			var dateTo = _creationDate.Date.AddDays(1);

			var classifications = from o in unitOfWork.Session.Query<Order>()
								  join oi in unitOfWork.GetAll<OrderItem>() on o.Id equals oi.Order.Id
								  join n in unitOfWork.GetAll<Nomenclature>() on oi.Nomenclature.Id equals n.Id
								  where
									  o.DeliveryDate < dateTo
									  && o.DeliveryDate >= dateFrom
									  && o.OrderStatus == OrderStatus.Closed

								  group new { Order = o, Item = oi, Nomenclature = n } by new { CleintId = o.Client.Id } into clientsGroups

								  select new CounterpartyClassification
								  (
									  clientsGroups.Key.CleintId,
									  clientsGroups.Sum(data =>
											  (data.Nomenclature.Category == NomenclatureCategory.water
												  && data.Nomenclature.TareVolume == TareVolume.Vol19L)
											  ? data.Item.Count
											  : 0),
									  clientsGroups.Select(data =>
											  data.Order.Id).Distinct().Count(),
									  clientsGroups.Sum(data =>
											  (data.Item.ActualCount ?? data.Item.Count) * data.Item.Price - data.Item.DiscountMoney),
									  _creationDate,
									  calculationSettings);

			return classifications;
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
			_unitOfWork?.Dispose();

			base.Dispose();
		}
		#endregion IDisposable implementation
	}
}
