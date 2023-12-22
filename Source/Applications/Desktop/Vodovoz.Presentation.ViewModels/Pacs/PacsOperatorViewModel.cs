using Core.Infrastructure;
using Pacs.Admin.Client;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client;
using Pacs.Server;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Utilities;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsOperatorViewModel : WidgetViewModelBase,
		IObserver<GlobalBreakAvailability>,
		IObserver<OperatorsOnBreakEvent>,
		IObserver<SettingsEvent>,
		IDisposable
	{
		private readonly Employee _employee;
		private readonly IOperatorStateAgent _operatorStateAgent;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeService _employeeService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IPacsRepository _pacsRepository;
		private readonly IPacsDashboardViewModelFactory _pacsDashboardViewModelFactory;
		private readonly OperatorSettingsConsumer _operatorSettingsConsumer;
		private readonly IOperatorClient _operatorClient;
		private readonly IDisposable _breakAvailabilitySubscription;
		private readonly IDisposable _operatorsOnBreakSubscription;
		private readonly IDisposable _settingsSubscription;

		private string _phoneNumber;
		private OperatorBreakAvailability _breakAvailability;
		private GlobalBreakAvailability _globalBreakAvailability;
		private IList<DashboardOperatorOnBreakViewModel> _operatorsOnBreak = new List<DashboardOperatorOnBreakViewModel>();
		private IPacsDomainSettings _settings;

		public DelegateCommand StartWorkShiftCommand { get; private set; }
		public DelegateCommand EndWorkShiftCommand { get; private set; }
		public DelegateCommand CancelEndWorkShiftReasonCommand { get; private set; }
		public DelegateCommand ChangePhoneCommand { get; private set; }
		public DelegateCommand StartLongBreakCommand { get; private set; }
		public DelegateCommand StartShortBreakCommand { get; private set; }
		public DelegateCommand EndBreakCommand { get; private set; }

		public PacsOperatorViewModel(
			IOperatorStateAgent operatorStateAgent,
			IInteractiveService interactiveService,
			IEmployeeService employeeService,
			IGuiDispatcher guiDispatcher,
			IOperatorClientFactory operatorClientFactory,
			IPacsRepository pacsRepository,
			IPacsDashboardViewModelFactory pacsDashboardViewModelFactory,
			IObservable<GlobalBreakAvailability> breakAvailabilityPublisher,
			IObservable<OperatorsOnBreakEvent> operatorsOnBreakPublisher,
			OperatorSettingsConsumer operatorSettingsConsumer
			)
		{
			if(operatorClientFactory is null)
			{
				throw new ArgumentNullException(nameof(operatorClientFactory));
			}

			_operatorStateAgent = operatorStateAgent ?? throw new ArgumentNullException(nameof(operatorStateAgent));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			_pacsDashboardViewModelFactory = pacsDashboardViewModelFactory ?? throw new ArgumentNullException(nameof(pacsDashboardViewModelFactory));
			_operatorSettingsConsumer = operatorSettingsConsumer ?? throw new ArgumentNullException(nameof(operatorSettingsConsumer));
			AvailablePhones = new List<string>();

			_employee = employeeService.GetEmployeeForCurrentUser();
			if(_employee == null)
			{
				throw new AbortCreatingPageException(
					"Должен быть привязан сотрудник к пользователю. Обратитесь в отдел кадров.",
					"Не настроен пользователь");
			}

			_operatorClient = operatorClientFactory.CreateOperatorClient(_employee.Id);
			_operatorClient.StateChanged += OnStateChanged;

			AvailablePhones = _pacsRepository.GetAvailablePhones();
			_settings = _pacsRepository.GetPacsDomainSettings();

			ConfigureCommands();

			_breakAvailabilitySubscription = breakAvailabilityPublisher.Subscribe(this);
			_operatorsOnBreakSubscription = operatorsOnBreakPublisher.Subscribe(this);
			_settingsSubscription = _operatorSettingsConsumer.Subscribe(this);

			try
			{
				var stateEvent = _operatorClient.Connect().Result;
				CurrentState = stateEvent.State;
				BreakAvailability = stateEvent.BreakAvailability;
				GlobalBreakAvailability = _operatorClient.GetGlobalBreakAvailability().Result;
			}
			catch(Exception ex)
			{
				var pacsEx = ex.FindExceptionTypeInInner<PacsException>();
				if(pacsEx != null)
				{
					throw new AbortCreatingPageException(ex.Message, "");
				}
			}

			PhoneNumber = CurrentState?.PhoneNumber;
			var operatorsOnBreakEvent = _operatorClient.GetOperatorsOnBreak().Result;
			PrepareOperatorsOnBreakViewModels(operatorsOnBreakEvent.OnBreak);
		}

		private void OnStateChanged(object sender, OperatorStateEvent e)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CurrentState = e.State;
				BreakAvailability = e.BreakAvailability;
			});
		}

		public OperatorState CurrentState
		{
			get => _operatorStateAgent.OperatorState;
			set
			{
				_operatorStateAgent.OperatorState = value;
				PhoneNumber = _operatorStateAgent.OperatorState?.PhoneNumber;
				OnPropertyChanged(nameof(CanStartWorkShift));
				OnPropertyChanged(nameof(CanEndWorkShift));
				OnPropertyChanged(nameof(CanStartLongBreak));
				OnPropertyChanged(nameof(CanStartShortBreak));
				OnPropertyChanged(nameof(CanEndBreak));
				OnPropertyChanged(nameof(CanChangePhone));
			}
		}

		public OperatorBreakAvailability BreakAvailability
		{
			get => _breakAvailability;
			private set
			{
				_breakAvailability = value;
				OnPropertyChanged(nameof(CanStartLongBreak));
				OnPropertyChanged(nameof(CanStartShortBreak));
				OnPropertyChanged(nameof(HasBreakInfo));
				OnPropertyChanged(nameof(BreakInfo));
			}
		}
		public GlobalBreakAvailability GlobalBreakAvailability
		{
			get => _globalBreakAvailability;
			private set
			{
				_globalBreakAvailability = value;
				OnPropertyChanged(nameof(CanStartLongBreak));
				OnPropertyChanged(nameof(CanStartShortBreak));
				OnPropertyChanged(nameof(HasBreakInfo));
				OnPropertyChanged(nameof(BreakInfo));
			}
		}

		public IList<DashboardOperatorOnBreakViewModel> OperatorsOnBreak
		{
			get => _operatorsOnBreak;
			private set => SetField(ref _operatorsOnBreak, value);
		}

		public bool HasBreakInfo => !CanStartLongBreak || !CanStartShortBreak;

		public string BreakInfo
		{
			get
			{
				string result = "";
				if(!GlobalBreakAvailability.LongBreakAvailable)
				{
					result += $"\n{GlobalBreakAvailability.LongBreakDescription}";
				}
				if(!BreakAvailability.LongBreakAvailable)
				{
					result += $"\n{BreakAvailability.LongBreakDescription}";
				}

				if(!GlobalBreakAvailability.ShortBreakAvailable)
				{
					result += $"\n{GlobalBreakAvailability.ShortBreakDescription}";
				}
				if(!BreakAvailability.ShortBreakAvailable)
				{
					result += $"\n{BreakAvailability.ShortBreakDescription}";
					if(BreakAvailability.ShortBreakSupposedlyAvailableAfter.HasValue)
					{
						result += $"\nМалый перерыв будет доступен после: {BreakAvailability.ShortBreakSupposedlyAvailableAfter.Value.ToString("dd.MM HH:mm")}";
						//ПО ИСТЕЧЕНИИ ВРЕМЕНИ СДЕЛАТЬ ЗАПРОС ДОСТУПНОСТИ ПЕРЕРЫВА
					}
				}

				return result.Trim('\n');
			}
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

		public IEnumerable<string> AvailablePhones { get; }

		[PropertyChangedAlso(nameof(CanStartWorkShift))]
		[PropertyChangedAlso(nameof(CanChangePhone))]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}


		public bool CanStartWorkShift => !string.IsNullOrWhiteSpace(PhoneNumber)
			&& AvailablePhones.Contains(PhoneNumber)
			&& _operatorStateAgent.CanStartWorkShift;

		private async Task StartWorkShift()
		{
			try
			{
				var state = await _operatorClient.StartWorkShift(PhoneNumber);
				UpdateState(state);
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}

		//СУДА ДОБАВИТЬ ХРАНЕНИЕ И ПРОВЕРКУ ВРЕМЕНИ КОГДА МОЖНО ЗАКОНЧИТЬ СМЕНУ
		//и действия по запросу причины в команде

		public bool CanEndWorkShift => _operatorStateAgent.CanEndWorkShift && CurrentState.WorkShift != null;

		private bool _endWorkShiftReasonRequired;
		public virtual bool EndWorkShiftReasonRequired
		{
			get => _endWorkShiftReasonRequired;
			set => SetField(ref _endWorkShiftReasonRequired, value);
		}

		private string _endWorkShiftReason;
		public virtual string EndWorkShiftReason
		{
			get => _endWorkShiftReason;
			set => SetField(ref _endWorkShiftReason, value);
		}

		private async Task EndWorkShift()
		{
			try
			{
				if(DateTime.Now > CurrentState.WorkShift.GetPlannedEndTime())
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

				var state = await _operatorClient.EndWorkShift(EndWorkShiftReason);
				UpdateState(state);
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}

		private void ClearEndWorkShiftReason()
		{
			EndWorkShiftReason = null;
			EndWorkShiftReasonRequired = false;
		}


		public bool CanChangePhone => !string.IsNullOrWhiteSpace(PhoneNumber)
			&& AvailablePhones.Contains(PhoneNumber)
			&& CurrentState != null
			&& CurrentState.PhoneNumber != PhoneNumber
			&& _operatorStateAgent.CanChangePhone;

		private async Task ChangePhone()
		{
			try
			{
				var state = await _operatorClient.ChangeNumber(PhoneNumber);
				UpdateState(state);
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}


		public bool CanStartLongBreak => _operatorStateAgent.CanStartBreak
			&& GlobalBreakAvailability.LongBreakAvailable
			&& BreakAvailability.LongBreakAvailable;

		public bool CanStartShortBreak => _operatorStateAgent.CanStartBreak
			&& GlobalBreakAvailability.ShortBreakAvailable
			&& BreakAvailability.ShortBreakAvailable;

		private async Task StartLongBreak()
		{
			try
			{
				var state = await _operatorClient.StartBreak(OperatorBreakType.Long);
				UpdateState(state);
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}

		private async Task StartShortBreak()
		{
			try
			{
				var state = await _operatorClient.StartBreak(OperatorBreakType.Short);
				UpdateState(state);
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}


		public bool CanEndBreak => _operatorStateAgent.CanEndBreak;

		private async Task EndBreak()
		{
			try
			{
				var state = await _operatorClient.EndBreak();
				UpdateState(state);
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}

		private void UpdateState(OperatorStateEvent operatorState)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CurrentState = operatorState.State;
				BreakAvailability = operatorState.BreakAvailability;
			});
		}

		void IObserver<GlobalBreakAvailability>.OnCompleted()
		{
			_breakAvailabilitySubscription.Dispose();
		}

		void IObserver<GlobalBreakAvailability>.OnError(Exception error)
		{
		}

		void IObserver<GlobalBreakAvailability>.OnNext(GlobalBreakAvailability value)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				GlobalBreakAvailability = value;
				OnPropertyChanged(nameof(CanStartLongBreak));
				OnPropertyChanged(nameof(CanStartShortBreak));
			});
		}

		public void Dispose()
		{
			_breakAvailabilitySubscription.Dispose();
			_operatorsOnBreakSubscription.Dispose();
			_settingsSubscription.Dispose();
		}

		#region IObserver<OperatorsOnBreakEvent>

		void IObserver<OperatorsOnBreakEvent>.OnCompleted()
		{
			_operatorsOnBreakSubscription.Dispose();
		}

		void IObserver<OperatorsOnBreakEvent>.OnError(Exception error)
		{
		}

		void IObserver<OperatorsOnBreakEvent>.OnNext(OperatorsOnBreakEvent value)
		{
			PrepareOperatorsOnBreakViewModels(value.OnBreak);
		}

		#endregion IObserver<OperatorsOnBreakEvent>

		private void PrepareOperatorsOnBreakViewModels(IEnumerable<OperatorState> operatorsOnBreak)
		{
			foreach(var operatorOnBreak in OperatorsOnBreak)
			{
				operatorOnBreak.Dispose();
			}

			var operatorViewModels = operatorsOnBreak.Select(x =>
			{
				var model = new OperatorModel(_employeeService);
				model.AddState(x);
				model.Settings = _settings;
				var vm = _pacsDashboardViewModelFactory.CreateOperatorOnBreakViewModel(model);
				return vm;
			});

			_guiDispatcher.RunInGuiTread(() =>
			{
				OperatorsOnBreak = operatorViewModels.ToList();
			});
		}

		#region IObserver<SettingsEvent>

		void IObserver<SettingsEvent>.OnCompleted()
		{
			_settingsSubscription.Dispose();
		}

		void IObserver<SettingsEvent>.OnError(Exception error)
		{
		}

		void IObserver<SettingsEvent>.OnNext(SettingsEvent value)
		{
			_settings = value.Settings;
			_guiDispatcher.RunInGuiTread(() =>
			{
				foreach(var operatorOnBreak in OperatorsOnBreak)
				{
					operatorOnBreak.Model.Settings = _settings;
				}
			});
		}

		#endregion IObserver<SettingsEvent>
	}
}
