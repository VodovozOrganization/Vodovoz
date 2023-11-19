using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsDashboardModel : PropertyChangedBase, IObserver<OperatorState>, IObserver<CallEvent>
	{
		private readonly Dictionary<int, OperatorModel> _operatorStatesDic;
		private readonly Dictionary<string, CallModel> _callsDic;
		private readonly IDisposable _operatorSubscription;
		private readonly IDisposable _callSubscription;
		private readonly IEmployeeService _employeeService;

		public ObservableCollection<OperatorModel> OperatorsOnBreak { get; }
		public ObservableCollection<OperatorModel> Operators { get; }
		public ObservableCollection<CallModel> Calls { get; }
		public ObservableCollection<MissedCallModel> MissedCalls { get; }

		// загрузка состояний за выбранный период

		// получение состояний всех операторов
		// получение событий всех звонков
		// получение событий об изменении настроек

		// сбор статистики и связей:
		// кто мог принять пропущенный звонок

		public PacsDashboardModel(IEmployeeService employeeService, IObservable<OperatorState> operatorPublisher, IObservable<CallEvent> callPublisher)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));

			_operatorStatesDic = new Dictionary<int, OperatorModel>();
			_callsDic = new Dictionary<string, CallModel>();
			Operators = new ObservableCollection<OperatorModel>();
			Calls = new ObservableCollection<CallModel>();
			MissedCalls = new ObservableCollection<MissedCallModel>();

			_operatorSubscription = operatorPublisher.Subscribe(this);
			_callSubscription = callPublisher.Subscribe(this);
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
			lock(_operatorStatesDic)
			{
				if(_operatorStatesDic.TryGetValue(value.OperatorId, out var model))
				{
					model.AddState(value);
					return;
				}

				model = new OperatorModel(_employeeService);
				model.BreakStarted += OnBreakStarted;
				model.BreakEnded += OnBreakEnded;
				model.AddState(value);
				_operatorStatesDic.Add(value.OperatorId, model);
				Operators.Insert(0, model);
			}
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

		void IObserver<CallEvent>.OnCompleted()
		{
			_callSubscription?.Dispose();
		}

		void IObserver<CallEvent>.OnError(Exception error)
		{
		}

		void IObserver<CallEvent>.OnNext(CallEvent value)
		{
			lock(_callsDic)
			{
				if(_callsDic.TryGetValue(value.CallId, out var model))
				{
					model.AddEvent(value);
					return;
				}

				model = new CallModel();
				model.CallMissed += OnCallMissed;
				model.AddEvent(value);
				_callsDic.Add(value.CallId, model);
				Calls.Insert(0, model);
			}
		}

		private void OnCallMissed(object sender, EventArgs e)
		{
			var model = (CallModel)sender;
			var missedCallModel = new MissedCallModel(model, _operatorStatesDic.Values);
			MissedCalls.Insert(0, missedCallModel);
		}

		#endregion IObserver<CallEvent>
	}
}
