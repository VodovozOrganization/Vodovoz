using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operator.Client;
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
	public class PacsOperatorViewModel : WidgetViewModelBase, IObserver<BreakAvailabilityEvent>, IDisposable
	{
		private readonly Employee _employee;
		private readonly IOperatorStateAgent _operatorStateAgent;
		private readonly IInteractiveService _interactiveService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IPacsRepository _pacsRepository;
		private readonly IOperatorClient _operatorClient;
		private readonly IDisposable _breakAvailabilitySubscription;

		private string _phoneNumber;
		private bool _breakAvailable;

		public DelegateCommand StartWorkShiftCommand { get; private set; }
		public DelegateCommand EndWorkShiftCommand { get; private set; }
		public DelegateCommand ChangePhoneCommand { get; private set; }
		public DelegateCommand StartBreakCommand { get; private set; }
		public DelegateCommand EndBreakCommand { get; private set; }

		public PacsOperatorViewModel(
			IOperatorStateAgent operatorStateAgent,
			IInteractiveService interactiveService,
			IEmployeeService employeeService,
			IGuiDispatcher guiDispatcher,
			IOperatorClientFactory operatorClientFactory,
			IPacsRepository pacsRepository,
			IObservable<BreakAvailabilityEvent> breakAvailabilityPublisher
			)
		{
			if(employeeService is null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			if(operatorClientFactory is null)
			{
				throw new ArgumentNullException(nameof(operatorClientFactory));
			}

			_operatorStateAgent = operatorStateAgent ?? throw new ArgumentNullException(nameof(operatorStateAgent));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
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

			ConfigureCommands();

			_breakAvailabilitySubscription = breakAvailabilityPublisher.Subscribe(this);

			try
			{
				CurrentState = _operatorClient.Connect().Result;
				_breakAvailable = _operatorClient.GetBreakAvailability().Result;
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
		}

		private void OnStateChanged(object sender, OperatorState e)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CurrentState = e;
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
				OnPropertyChanged(nameof(CanStartBreak));
				OnPropertyChanged(nameof(CanEndBreak));
				OnPropertyChanged(nameof(CanChangePhone));
			}
		}

		private void ConfigureCommands()
		{
			StartWorkShiftCommand = new DelegateCommand(() => StartWorkShift().Wait(), () => CanStartWorkShift);
			StartWorkShiftCommand.CanExecuteChangedWith(this, x => x.CanStartWorkShift);

			EndWorkShiftCommand = new DelegateCommand(() => EndWorkShift().Wait(), () => CanEndWorkShift);
			EndWorkShiftCommand.CanExecuteChangedWith(this, x => x.CanEndWorkShift);

			ChangePhoneCommand = new DelegateCommand(() => ChangePhone().Wait(), () => CanChangePhone);
			ChangePhoneCommand.CanExecuteChangedWith(this, x => x.CanChangePhone);

			StartBreakCommand = new DelegateCommand(() => StartBreak().Wait(), () => CanStartBreak);
			StartBreakCommand.CanExecuteChangedWith(this, x => x.CanStartBreak);

			EndBreakCommand = new DelegateCommand(() => EndBreak().Wait(), () => CanEndBreak);
			EndBreakCommand.CanExecuteChangedWith(this, x => x.CanEndBreak);
		}

		/*private void LoadDetails()
		{
			var history = _operatorRepository.GetOperatorHistory(_employee.Id);
		}*/

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
				_guiDispatcher.RunInGuiTread(() =>
				{
					CurrentState = state;
				});
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}


		public bool CanEndWorkShift => _operatorStateAgent.CanEndWorkShift;

		private async Task EndWorkShift()
		{
			try
			{
				var state = await _operatorClient.EndWorkShift();
				_guiDispatcher.RunInGuiTread(() =>
				{
					CurrentState = state;
				});
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
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
				_guiDispatcher.RunInGuiTread(() =>
				{
					CurrentState = state;
				});
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}


		public bool CanStartBreak => _operatorStateAgent.CanStartBreak && _breakAvailable;

		private async Task StartBreak()
		{
			try
			{
				var state = await _operatorClient.StartBreak();
				_guiDispatcher.RunInGuiTread(() =>
				{
					CurrentState = state;
				});
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
				_guiDispatcher.RunInGuiTread(() =>
				{
					CurrentState = state;
				});
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
				});
			}
		}

		void IObserver<BreakAvailabilityEvent>.OnCompleted()
		{
			_breakAvailabilitySubscription.Dispose();
		}

		void IObserver<BreakAvailabilityEvent>.OnError(Exception error)
		{
		}

		void IObserver<BreakAvailabilityEvent>.OnNext(BreakAvailabilityEvent value)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				_breakAvailable = value.BreakAvailable;
				OnPropertyChanged(nameof(CanStartBreak));
			});
		}

		public void Dispose()
		{
			_breakAvailabilitySubscription.Dispose();
		}
	}
}
