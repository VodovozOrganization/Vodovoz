using Core.Infrastructure;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Application.Pacs;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsOperatorViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly ILogger<PacsOperatorViewModel> _logger;
		private readonly OperatorService _operatorService;
		private readonly IEmployeeService _employeeService;
		private readonly IInteractiveService _interactiveService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IPacsDashboardViewModelFactory _pacsDashboardViewModelFactory;

		private GenericObservableList<DashboardOperatorOnBreakViewModel> _operatorsOnBreak;
		private string _phoneNumber;
		private IEnumerable<string> _availablePhones;
		private string _endWorkShiftReason;
		private bool _endWorkShiftReasonRequired;
		private string _breakInfo;

		public PacsOperatorViewModel(
			ILogger<PacsOperatorViewModel> logger,
			OperatorService operatorService,
			IEmployeeService employeeService,
			IInteractiveService interactiveService,
			IGuiDispatcher guiDispatcher,
			IPacsDashboardViewModelFactory pacsDashboardViewModelFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operatorService = operatorService ?? throw new ArgumentNullException(nameof(operatorService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_pacsDashboardViewModelFactory = pacsDashboardViewModelFactory ?? throw new ArgumentNullException(nameof(pacsDashboardViewModelFactory));

			_operatorsOnBreak = new GenericObservableList<DashboardOperatorOnBreakViewModel>();

			if(!(_operatorService.IsInitialized && _operatorService.IsOperator))
			{
				throw new AbortCreatingPageException(
					"Пользователь должен быть оператором.",
					"Не настроен пользователь");
			}

			ConfigureCommands();

			PhoneNumber = _operatorService.OperatorState?.PhoneNumber;
			AvailablePhones = _operatorService.AvailablePhones;
			PrepareOperatorsOnBreakViewModels();

			_operatorService.PropertyChanged += OperatorServicePropertyChanged;
		}

		public DelegateCommand StartWorkShiftCommand { get; private set; }
		public DelegateCommand EndWorkShiftCommand { get; private set; }
		public DelegateCommand CancelEndWorkShiftReasonCommand { get; private set; }
		public DelegateCommand ChangePhoneCommand { get; private set; }
		public DelegateCommand StartLongBreakCommand { get; private set; }
		public DelegateCommand StartShortBreakCommand { get; private set; }
		public DelegateCommand EndBreakCommand { get; private set; }

		public GenericObservableList<DashboardOperatorOnBreakViewModel> OperatorsOnBreak
		{
			get => _operatorsOnBreak;
			private set => SetField(ref _operatorsOnBreak, value);
		}

		public bool CanStartWorkShift => !string.IsNullOrWhiteSpace(PhoneNumber)
			&& _operatorService.AvailablePhones.Contains(PhoneNumber)
			&& _operatorService.CanStartWorkShift;

		public virtual bool EndWorkShiftReasonRequired
		{
			get => _endWorkShiftReasonRequired;
			set => SetField(ref _endWorkShiftReasonRequired, value);
		}

		public virtual string EndWorkShiftReason
		{
			get => _endWorkShiftReason;
			set => SetField(ref _endWorkShiftReason, value);
		}

		[PropertyChangedAlso(nameof(CanStartWorkShift))]
		[PropertyChangedAlso(nameof(CanChangePhone))]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		public virtual string CurrentOperatorId =>
			_operatorService.OperatorState?.OperatorId is null
			? "Номер оператора: -"
			: $"Номер оператора: {_operatorService.OperatorState?.OperatorId ?? 0}";

		public virtual string WorkShiftId =>
			_operatorService.OperatorState?.WorkShift?.Id is null
			? "Номер смены: -"
			: $"Номер смены: {_operatorService.OperatorState?.WorkShift?.Id ?? 0}";

		public virtual string CurrentOperatorStatus =>
			_operatorService.OperatorState is null
			? "Статус оператора: -"
			: $"Статус оператора: {_operatorService.OperatorState.State.GetEnumTitle()}";

		public virtual string ShortBreaksUsedCount =>
			$"Количество использованных малых перерывов: 0";

		public virtual IEnumerable<string> AvailablePhones
		{
			get => _availablePhones;
			private set => SetField(ref _availablePhones, value);
		}

		public bool CanChangePhone => !string.IsNullOrWhiteSpace(PhoneNumber)
			&& _operatorService.AvailablePhones.Contains(PhoneNumber)
			&& _operatorService.OperatorState != null
			&& _operatorService.OperatorState.PhoneNumber != PhoneNumber
			&& _operatorService.CanChangePhone;

		public bool CanStartLongBreak => _operatorService.CanStartLongBreak && !_operatorService.BreakInProgress;

		public bool CanStartShortBreak => _operatorService.CanStartShortBreak && !_operatorService.BreakInProgress;

		public bool CanEndBreak => _operatorService.CanEndBreak && !_operatorService.BreakInProgress;

		public bool ShowBreakInfo => !_operatorService.CanStartLongBreak || !_operatorService.CanStartShortBreak;

		public bool CanEndWorkShift => _operatorService.CanEndWorkShift;

		public virtual string BreakInfo
		{
			get => _breakInfo;
			private set => SetField(ref _breakInfo, value);
		}

		private void OperatorServicePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				switch(e.PropertyName)
				{
					case nameof(OperatorService.OperatorState):
						PhoneNumber = _operatorService.OperatorState?.PhoneNumber;
						OnPropertyChanged(nameof(CanChangePhone));
						OnPropertyChanged(nameof(CurrentOperatorId));
						OnPropertyChanged(nameof(CurrentOperatorStatus));
						break;
					case nameof(OperatorService.OperatorsOnBreak):
						PrepareOperatorsOnBreakViewModels();
						break;
					case nameof(OperatorService.AvailablePhones):
						AvailablePhones = _operatorService.AvailablePhones;
						OnPropertyChanged(nameof(CanStartWorkShift));
						OnPropertyChanged(nameof(CanChangePhone));
						break;
					case nameof(OperatorService.CanStartWorkShift):
						OnPropertyChanged(nameof(CanStartWorkShift));
						OnPropertyChanged(nameof(WorkShiftId));
						break;
					case nameof(OperatorService.CanChangePhone):
						OnPropertyChanged(nameof(CanChangePhone));
						break;
					case nameof(OperatorService.BreakInfo):
						BreakInfo = _operatorService.BreakInfo;
						break;
					case nameof(OperatorService.Settings):
						UpdateSettings();
						break;
					case nameof(OperatorService.CanStartLongBreak):
						OnPropertyChanged(nameof(ShowBreakInfo));
						OnPropertyChanged(nameof(CanStartLongBreak));
						break;
					case nameof(OperatorService.CanStartShortBreak):
						OnPropertyChanged(nameof(ShowBreakInfo));
						OnPropertyChanged(nameof(CanStartShortBreak));
						break;
					case nameof(OperatorService.CanEndBreak):
						OnPropertyChanged(nameof(CanEndBreak));
						break;
					case nameof(OperatorService.BreakInProgress):
						OnPropertyChanged(nameof(CanStartLongBreak));
						OnPropertyChanged(nameof(CanStartShortBreak));
						OnPropertyChanged(nameof(ShortBreaksUsedCount));
						OnPropertyChanged(nameof(CanEndBreak));
						break;
					case nameof(OperatorService.CanEndWorkShift):
						OnPropertyChanged(nameof(CanEndWorkShift));
						OnPropertyChanged(nameof(WorkShiftId));
						break;
					case nameof(OperatorService.MangoPhone):
						PhoneNumber = _operatorService.OperatorState?.PhoneNumber;
						OnPropertyChanged(nameof(PhoneNumber));
						break;
					default:
						break;
				}
			});
		}

		private void ConfigureCommands()
		{
			StartWorkShiftCommand = new DelegateCommand(() => StartWorkShift().Wait(), () => CanStartWorkShift);
			StartWorkShiftCommand.CanExecuteChangedWith(this, x => x.CanStartWorkShift);

			EndWorkShiftCommand = new DelegateCommand(() => EndWorkShift().Wait(), () => CanEndWorkShift);
			EndWorkShiftCommand.CanExecuteChangedWith(this, x => x.CanEndWorkShift);

			CancelEndWorkShiftReasonCommand = new DelegateCommand(() => ClearEndWorkShiftReason());

			ChangePhoneCommand = new DelegateCommand(() => ChangePhone().Wait(), () => CanChangePhone);
			ChangePhoneCommand.CanExecuteChangedWith(this, x => x.CanChangePhone);

			StartLongBreakCommand = new DelegateCommand(() => StartLongBreak().Wait(), () => CanStartLongBreak);
			StartLongBreakCommand.CanExecuteChangedWith(this, x => x.CanStartLongBreak);

			StartShortBreakCommand = new DelegateCommand(() => StartShortBreak().Wait(), () => CanStartShortBreak);
			StartShortBreakCommand.CanExecuteChangedWith(this, x => x.CanStartShortBreak);

			EndBreakCommand = new DelegateCommand(() => EndBreak().Wait(), () => CanEndBreak);
			EndBreakCommand.CanExecuteChangedWith(this, x => x.CanEndBreak);
		}

		private async Task StartWorkShift()
		{
			try
			{
				await _operatorService.StartWorkShift(PhoneNumber);
			}
			catch(PacsException ex)
			{
				_logger.LogError(ex, "Произошла ошибка при начале смены: {ExceptionMessage}", ex.Message);
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при начале смены: {ExceptionMessage}", ex.Message);
				throw;
			}
		}

		private async Task EndWorkShift()
		{
			try
			{
				if(DateTime.Now > _operatorService.OperatorState.WorkShift.GetPlannedEndTime())
				{
					_guiDispatcher.RunInGuiTread(() =>
					{
						ClearEndWorkShiftReason();
					});
				}
				else
				{
					_guiDispatcher.RunInGuiTread(() =>
					{
						EndWorkShiftReasonRequired = true;
					});

					if(EndWorkShiftReason.IsNullOrWhiteSpace())
					{
						return;
					}
				}

				await _operatorService.EndWorkShift(EndWorkShiftReason);
				_guiDispatcher.RunInGuiTread(() =>
				{
					ClearEndWorkShiftReason();
				});
			}
			catch(PacsException ex)
			{
				_logger.LogError(ex, "Произошла ошибка при завершении смены: {ExceptionMessage}", ex.Message);
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при завершении смены: {ExceptionMessage}", ex.Message);
				throw;
			}
		}

		private void ClearEndWorkShiftReason()
		{
			EndWorkShiftReason = "";
			EndWorkShiftReasonRequired = false;
		}

		private async Task ChangePhone()
		{
			try
			{
				await _operatorService.ChangePhone(PhoneNumber);
			}
			catch(PacsException ex)
			{
				_logger.LogError(ex, "Произошла ошибка при смене номера телефона: {ExceptionMessage}", ex.Message);
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при смене номера телефона: {ExceptionMessage}", ex.Message);
				throw;
			}
		}

		private async Task StartLongBreak()
		{
			if(!_interactiveService.Question("Хотите взять большой перерыв?", "Перерыв"))
			{
				return;
			}

			try
			{
				await _operatorService.StartLongBreak();
			}
			catch(PacsException ex)
			{
				_logger.LogError(ex, "Произошла ошибка при начале длинного перерыва: {ExceptionMessage}", ex.Message);
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при начале длинного перерыва: {ExceptionMessage}", ex.Message);
				throw;
			}
		}

		private async Task StartShortBreak()
		{
			if(!_interactiveService.Question("Хотите взять малый перерыв?", "Перерыв"))
			{
				return;
			}

			try
			{
				await _operatorService.StartShortBreak();
			}
			catch(PacsException ex)
			{
				_logger.LogError(ex, "Произошла ошибка при начале короткого перерыва: {ExceptionMessage}", ex.Message);
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при начале короткого перерыва: {ExceptionMessage}", ex.Message);
				throw;
			}
		}

		private async Task EndBreak()
		{
			if(!_interactiveService.Question("Закончить перерыв?", "Перерыв"))
			{
				return;
			}

			try
			{
				await _operatorService.EndBreak();
			}
			catch(PacsException ex)
			{
				_logger.LogError(ex, "Произошла ошибка при завершении перерыва: {ExceptionMessage}", ex.Message);
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при завершении перерыва: {ExceptionMessage}", ex.Message);
				throw;
			}
		}

		private void PrepareOperatorsOnBreakViewModels()
		{
			foreach(var operatorOnBreak in OperatorsOnBreak)
			{
				operatorOnBreak.Dispose();
			}

			var operatorViewModels = _operatorService.OperatorsOnBreak.Select(x =>
			{
				var model = new OperatorModel(_employeeService);
				model.AddState(x);
				model.Settings = _operatorService.Settings;
				var vm = _pacsDashboardViewModelFactory.CreateOperatorOnBreakViewModel(model);
				return vm;
			});

			_guiDispatcher.RunInGuiTread(() =>
			{
				_logger.LogInformation("RunInGuiTread operatorViewModels.ToList()");
				OperatorsOnBreak = new GenericObservableList<DashboardOperatorOnBreakViewModel>(operatorViewModels.ToList());
			});
		}

		private void UpdateSettings()
		{
			foreach(var operatorOnBreak in OperatorsOnBreak)
			{
				operatorOnBreak.Model.Settings = _operatorService.Settings;
			}
		}

		public void Dispose()
		{
			_operatorService.PropertyChanged -= OperatorServicePropertyChanged;
		}
	}
}
