using Microsoft.Extensions.Logging;
using Pacs.Admin.Client;
using Pacs.Admin.Client.Consumers;
using Pacs.Core.Messages.Events;
using QS.DomainModel.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Services;

namespace Vodovoz.Application.Pacs
{
	public class PacsDashboardModel : PropertyChangedBase, 
		IObserver<OperatorState>, 
		IObserver<PacsCallEvent>, 
		IObserver<SettingsEvent>,
		IDisposable
	{
		private readonly TimeSpan _operatorsRecentTimespan = TimeSpan.FromHours(-5);

		private readonly ConcurrentDictionary<int, OperatorModel> _operatorIdToStates;
		private readonly ConcurrentDictionary<string, CallModel> _callNumberToCalls;
		private readonly ConcurrentDictionary<string, MissedCallModel> _missedCallsNumberToCalls;
		private readonly IDisposable _operatorSubscription;
		private readonly IDisposable _callSubscription;
		private readonly IDisposable _settingsSubscription;
		private readonly ILogger<PacsDashboardModel> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly IPacsRepository _repository;
		private readonly IAdminClient _adminClient;
		private readonly ConcurrentQueue<OperatorState> _operatorStatesQueue = new ConcurrentQueue<OperatorState>();
		private readonly ConcurrentQueue<PacsCallEvent> _callEventsQueue = new ConcurrentQueue<PacsCallEvent>();
		private Timer _operatorStatesWorker;
		private Timer _callsWorker;
		private bool _callWorkerInProgress;
		private System.Timers.Timer _callsTimer;
		private CancellationTokenSource _cts;

		private IPacsDomainSettings _settings;

		public PacsDashboardModel(
			ILogger<PacsDashboardModel> logger,
			IEmployeeService employeeService,
			IPacsRepository repository,
			OperatorStateAdminConsumer operatorStateAdminConsumer,
			IObservable<SettingsEvent> settingsPublisher,
			IAdminClient adminClient,
			IObservable<PacsCallEvent> callPublisher)
		{
			if(operatorStateAdminConsumer is null)
			{
				throw new ArgumentNullException(nameof(operatorStateAdminConsumer));
			}

			if(callPublisher is null)
			{
				throw new ArgumentNullException(nameof(callPublisher));
			}
			_logger = logger;
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
			_operatorIdToStates = new ConcurrentDictionary<int, OperatorModel>();
			_callNumberToCalls = new ConcurrentDictionary<string, CallModel>();
			_missedCallsNumberToCalls = new ConcurrentDictionary<string, MissedCallModel>();
			OperatorsOnBreak = new ObservableCollection<OperatorModel>();
			Operators = new ObservableCollection<OperatorModel>();
			Calls = new ObservableCollection<CallModel>();
			MissedCalls = new ObservableCollection<MissedCallModel>();

			_cts = new CancellationTokenSource();

			try
			{
				_settings = _adminClient.GetSettings().Result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Произошла ошабка при обращении к серверу СКУД: {ExceptionMessage}", ex.Message);
				return;
			}

			_settingsSubscription = settingsPublisher.Subscribe(this);

			//для правильной загрузки:
			//сначала подписка на события
			_operatorSubscription = operatorStateAdminConsumer.Subscribe(this);
			_callSubscription = callPublisher.Subscribe(this);
			//потом загрузка из базы
			//сначала операторов, потом звонки
			var recentDate = DateTime.Now.Add(_operatorsRecentTimespan);
			LoadOperatorsFromDateTime();
			LoadRecentCalls(recentDate);
			//потом запуск обработчиков очередей событий
			StartOperatorStatesWorker();
			StartCallsWorker();
		}

		public event EventHandler OperatorsLoaded;
		public ObservableCollection<OperatorModel> OperatorsOnBreak { get; }
		public ObservableCollection<OperatorModel> Operators { get; }
		public ObservableCollection<CallModel> Calls { get; }
		public ObservableCollection<MissedCallModel> MissedCalls { get; }

		private void StartOperatorStatesWorker()
		{
			_operatorStatesWorker = new Timer((e) =>
			{
				while(_operatorStatesQueue.TryDequeue(out var state))
				{
					AddOperatorState(state);
				}
			}, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
		}

		private void StartCallsWorker()
		{
			_callsWorker = new Timer((e) =>
			{
				while(_callEventsQueue.TryDequeue(out var callEvent))
				{
					UpdateCall(callEvent.Call);
				}
			}, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
		}

		public void LoadOperatorsFromDateTime(DateTime? from = null)
		{
			if(from == null)
			{
				from = DateTime.Now.Add(_operatorsRecentTimespan);
			}

			ClearOperators();

			var recentOperators = _repository.GetOperatorStatesFrom(from.Value);
			foreach(var operatorState in recentOperators)
			{
				AddOperatorState(operatorState);
			}

			OperatorsLoaded?.Invoke(this, EventArgs.Empty);
		}

		private void ClearOperators()
		{
			_operatorIdToStates.Clear();
			Operators.Clear();
		}

		private void LoadRecentCalls(DateTime from)
		{
			var activeCallsEvents = _repository.GetCalls(from);
			foreach(var callEvent in activeCallsEvents)
			{
				UpdateCall(callEvent);
			}
		}

		#region IObserver<OperatorState>

		void IObserver<OperatorState>.OnCompleted()
		{
			_operatorSubscription?.Dispose();
		}

		void IObserver<OperatorState>.OnError(Exception error)
		{
		}

		void IObserver<OperatorState>.OnNext(OperatorState value)
		{
			_operatorStatesQueue.Enqueue(value);
		}

		private void OnBreakStarted(object sender, EventArgs e)
		{
			var model = (OperatorModel)sender;

			var existingModel = OperatorsOnBreak.FirstOrDefault(x => x.Employee.Id == model.Employee.Id);

			if(existingModel != null && !OperatorsOnBreak.Contains(model))
			{
				OperatorsOnBreak.Remove(existingModel);
			}

			if(!OperatorsOnBreak.Contains(model))
			{
				OperatorsOnBreak.Insert(0, model);
			}
		}

		private void OnBreakEnded(object sender, EventArgs e)
		{
			var model = (OperatorModel)sender;
			OperatorsOnBreak.Remove(model);
		}

		#endregion IObserver<OperatorState>

		#region IObserver<CallEvent>

		void IObserver<PacsCallEvent>.OnCompleted()
		{
			_callSubscription?.Dispose();
		}

		void IObserver<PacsCallEvent>.OnError(Exception error)
		{
		}

		void IObserver<PacsCallEvent>.OnNext(PacsCallEvent value)
		{
			_callEventsQueue.Enqueue(value);
		}

		private void CheckCallMissed(CallModel model)
		{
			if(model.Call.EntryResult != CallEntryResult.Missed)
			{
				return;
			}

			if(_missedCallsNumberToCalls.ContainsKey(model.Call.EntryId))
			{
				return;
			}

			var missedCallModel = new MissedCallModel(model, _operatorIdToStates.Values);

			lock(MissedCalls)
			{
				_missedCallsNumberToCalls.TryAdd(model.Call.EntryId, missedCallModel);
				MissedCalls.Insert(0, missedCallModel);
			}
		}

		#endregion IObserver<CallEvent>

		private void AddOperatorState(OperatorState state)
		{
			if(_operatorIdToStates.TryGetValue(state.OperatorId, out var model))
			{
				model.AddState(state);
				return;
			}

			model = new OperatorModel(_employeeService);
			model.Settings = _settings;
			model.BreakStarted += OnBreakStarted;
			model.BreakEnded += OnBreakEnded;
			model.AddState(state);
			if(_operatorIdToStates.TryAdd(state.OperatorId, model))
			{
				Operators.Insert(0, model);
			}
		}

		private void UpdateCall(Call call)
		{
			if(_callNumberToCalls.TryGetValue(call.EntryId, out var model))
			{
				model.UpdateCall(call);
				CheckCallMissed(model);
				return;
			}

			model = new CallModel(_operatorIdToStates.Values);
			model.UpdateCall(call);
			if(!model.IsIncomingCall)
			{
				return;
			}

			if(_callNumberToCalls.TryAdd(call.EntryId, model))
			{
				lock(Calls)
				{
					Calls.Insert(0, model);
				}
			}
			CheckCallMissed(model);
		}

		#region IObserver<SettingsEvent>

		void IObserver<SettingsEvent>.OnCompleted()
		{
			_settingsSubscription?.Dispose();
		}

		void IObserver<SettingsEvent>.OnError(Exception error)
		{
		}

		void IObserver<SettingsEvent>.OnNext(SettingsEvent value)
		{
			_settings = value.Settings;
			ShareSettingsToOperatorModels();
		}

		private void ShareSettingsToOperatorModels()
		{
			foreach(var om in OperatorsOnBreak.ToList())
			{
				om.Settings = _settings;
			}

			foreach(var om in Operators.ToList())
			{
				om.Settings = _settings;
			}
		}

		#endregion IObserver<SettingsEvent>

		public void Dispose()
		{
			_cts.Cancel();

			_operatorSubscription?.Dispose();
			_callSubscription?.Dispose();
			_settingsSubscription?.Dispose();

			_operatorStatesWorker?.Dispose();
			_callsWorker?.Dispose();
			_callsTimer?.Dispose();
		}
	}
}
