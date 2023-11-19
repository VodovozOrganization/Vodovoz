using QS.Dialog;
using QS.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Data.Bindings.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsDashboardViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly PacsDashboardModel _pacsDashboardModel;
		private readonly IPacsDashboardViewModelFactory _pacsDashboardViewModelFactory;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly BlockingCollection<Action> _invocationQueue = new BlockingCollection<Action>();
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly Task _queueProcessor;

		public PacsDashboardViewModel(
			PacsDashboardModel pacsDashboardModel, 
			IPacsDashboardViewModelFactory pacsDashboardViewModelFactory, 
			IGuiDispatcher guiDispatcher)
		{
			_pacsDashboardModel = pacsDashboardModel ?? throw new ArgumentNullException(nameof(pacsDashboardModel));
			_pacsDashboardViewModelFactory = pacsDashboardViewModelFactory ?? throw new ArgumentNullException(nameof(pacsDashboardViewModelFactory));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			_cancellationTokenSource = new CancellationTokenSource();
			_invocationQueue = new BlockingCollection<Action>();
			OperatorsOnBreak = new GenericObservableList<DashboardOperatorOnBreakViewModel>();
			OperatorsOnWorkshift = new GenericObservableList<DashboardOperatorViewModel>();
			MissedCalls = new GenericObservableList<DashboardMissedCallViewModel>();
			Calls = new GenericObservableList<DashboardCallViewModel>();

			_pacsDashboardModel.OperatorsOnBreak.CollectionChanged += OnOperatorsOnBreakChanged;
			_pacsDashboardModel.Operators.CollectionChanged += OnOperatorsChanged;
			_pacsDashboardModel.MissedCalls.CollectionChanged += OnMissedCallsChanged;
			_pacsDashboardModel.Calls.CollectionChanged += OnCallsChanged;

			_queueProcessor = Task.Run(() => ProcessQueue(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
		}

		public GenericObservableList<DashboardOperatorOnBreakViewModel> OperatorsOnBreak { get; }
		public GenericObservableList<DashboardOperatorViewModel> OperatorsOnWorkshift { get; }
		public GenericObservableList<DashboardMissedCallViewModel> MissedCalls { get; }
		public GenericObservableList<DashboardCallViewModel> Calls { get; }

		private void OnOperatorsOnBreakChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var newItem = (OperatorModel)e.NewItems[0];
					var newItemVM = _pacsDashboardViewModelFactory.CreateOperatorOnBreakViewModel(newItem);
					_invocationQueue.Add(() => OperatorsOnBreak.Insert(e.NewStartingIndex, newItemVM));
					break;
				case NotifyCollectionChangedAction.Remove:
					_invocationQueue.Add(() => OperatorsOnBreak.RemoveAt(e.OldStartingIndex));
					break;
				default:
					break;
			}
		}

		private void OnOperatorsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var newItem = (OperatorModel)e.NewItems[0];
					var newItemVM = _pacsDashboardViewModelFactory.CreateOperatorViewModel(newItem);
					_invocationQueue.Add(() => OperatorsOnWorkshift.Insert(e.NewStartingIndex, newItemVM));
					break;
				case NotifyCollectionChangedAction.Remove:
					_invocationQueue.Add(() => OperatorsOnWorkshift.RemoveAt(e.OldStartingIndex));
					break;
				default:
					break;
			}
		}

		private void OnMissedCallsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var newItem = (MissedCallModel)e.NewItems[0];
					var newItemVM = _pacsDashboardViewModelFactory.CreateMissedCallViewModel(newItem);
					_invocationQueue.Add(() => MissedCalls.Insert(e.NewStartingIndex, newItemVM));
					break;
				case NotifyCollectionChangedAction.Remove:
					_invocationQueue.Add(() => MissedCalls.RemoveAt(e.OldStartingIndex));
					break;
				default:
					break;
			}
		}

		private void OnCallsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var newItem = (CallModel)e.NewItems[0];
					var newItemVM = _pacsDashboardViewModelFactory.CreateCallViewModel(newItem);
					_invocationQueue.Add(() => Calls.Insert(e.NewStartingIndex, newItemVM));
					break;
				case NotifyCollectionChangedAction.Remove:
					_invocationQueue.Add(() => Calls.RemoveAt(e.OldStartingIndex));
					break;
				default:
					break;
			}
		}

		private void ProcessQueue(CancellationToken cancellationToken)
		{
			while(cancellationToken.IsCancellationRequested)
			{
				var action = _invocationQueue.Take();
				_guiDispatcher.RunInGuiTread(action);
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_invocationQueue?.Dispose();
		}
	}
}
