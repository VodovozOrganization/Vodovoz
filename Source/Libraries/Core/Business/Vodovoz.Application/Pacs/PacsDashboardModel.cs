using Pacs.Admin.Client;
using Pacs.Admin.Client.Consumers;
using Pacs.Core.Messages.Events;
using QS.DomainModel.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		private readonly Dictionary<int, OperatorModel> _operatorStatesDic;
		private readonly Dictionary<string, CallModel> _callsDic;
		private readonly IDisposable _operatorSubscription;
		private readonly IDisposable _callSubscription;
		private readonly IDisposable _settingsSubscription;
		private readonly IEmployeeService _employeeService;
		private readonly IPacsRepository _repository;
		private readonly AdminClient _adminClient;
		private readonly ConcurrentQueue<OperatorState> _operatorStatesQueue = new ConcurrentQueue<OperatorState>();
		private readonly ConcurrentQueue<PacsCallEvent> _callEventsQueue = new ConcurrentQueue<PacsCallEvent>();
		private Timer _operatorStatesWorker;
		private Timer _callsWorker;

		private IPacsDomainSettings _settings;

		public ObservableCollection<OperatorModel> OperatorsOnBreak { get; }
		public ObservableCollection<OperatorModel> Operators { get; }
		public ObservableCollection<CallModel> Calls { get; }
		public ObservableCollection<MissedCallModel> MissedCalls { get; }

		public PacsDashboardModel(
			IEmployeeService employeeService,
			IPacsRepository repository,
			OperatorStateAdminConsumer operatorStateAdminConsumer,
			IObservable<SettingsEvent> settingsPublisher,
			AdminClient adminClient,
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

			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
			_operatorStatesDic = new Dictionary<int, OperatorModel>();
			_callsDic = new Dictionary<string, CallModel>();
			OperatorsOnBreak = new ObservableCollection<OperatorModel>();
			Operators = new ObservableCollection<OperatorModel>();
			Calls = new ObservableCollection<CallModel>();
			MissedCalls = new ObservableCollection<MissedCallModel>();

			_settings = _adminClient.GetSettings().Result;
			_settingsSubscription = settingsPublisher.Subscribe(this);

			//для правильной загрузки:
			//сначала подписка на события
			_operatorSubscription = operatorStateAdminConsumer.Subscribe(this);
			_callSubscription = callPublisher.Subscribe(this);
			//потом загрузка из базы
			//сначала операторов, потом звонки
			var recentDate = DateTime.Now.AddHours(-5);
			LoadRecentOperators(recentDate);
			LoadRecentCalls(recentDate);
			//потом запуск обработчиков очередей событий
			StartOperatorStatesWorker();
			StartCallsWorker();
		}

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

		private void LoadRecentOperators(DateTime from)
		{
			var recentOperators = _repository.GetOperators(from);
			foreach(var operatorState in recentOperators)
			{
				AddOperatorState(operatorState);
			}
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

			var missedCallModel = new MissedCallModel(model, _operatorStatesDic.Values);
			lock(MissedCalls)
			{
				MissedCalls.Insert(0, missedCallModel);
			}
		}

		#endregion IObserver<CallEvent>

		private void AddOperatorState(OperatorState state)
		{
			lock(_operatorStatesDic)
			{
				if(_operatorStatesDic.TryGetValue(state.OperatorId, out var model))
				{
					model.AddState(state);
					return;
				}

				model = new OperatorModel(_employeeService);
				model.Settings = _settings;
				model.BreakStarted += OnBreakStarted;
				model.BreakEnded += OnBreakEnded;
				model.AddState(state);
				_operatorStatesDic.Add(state.OperatorId, model);
				Operators.Insert(0, model);
			}
		}

		private void UpdateCall(Call call)
		{
			lock(_callsDic)
			{
				if(!_callsDic.TryGetValue(call.CallId, out var model))
				{
					model = new CallModel(_operatorStatesDic.Values);
					_callsDic.Add(call.CallId, model);
					Calls.Insert(0, model);
				}

				model.UpdateCall(call);
				CheckCallMissed(model);
			}
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
			_operatorSubscription?.Dispose();
			_callSubscription?.Dispose();
			_settingsSubscription?.Dispose();

			_operatorStatesWorker?.Dispose();
			_callsWorker?.Dispose();
		}
	}
}
