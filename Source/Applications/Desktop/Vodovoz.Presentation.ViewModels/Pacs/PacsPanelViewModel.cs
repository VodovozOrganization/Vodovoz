using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsPanelViewModel : WidgetViewModelBase
	{
		private readonly IOperatorClient _operatorClient;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly INavigationManager _navigationManager;

		private bool _pacsEnabled;
		private Operator _operatorState;
		private MangoState _mangoState;

		public DelegateCommand BreakCommand { get; }
		public DelegateCommand RefreshCommand { get; }
		public DelegateCommand OpenPacsDialogCommand { get; }
		public DelegateCommand OpenMangoDialogCommand { get; }

		public PacsPanelViewModel(IOperatorClient operatorClient, IGuiDispatcher guiDispatcher, INavigationManager navigationManager)
		{
			_operatorClient = operatorClient ?? throw new ArgumentNullException(nameof(operatorClient));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			BreakCommand = new DelegateCommand(Break);
			BreakCommand.CanExecuteChangedWith(this, x => x.CanBreak);

			RefreshCommand = new DelegateCommand(Refresh);
			RefreshCommand.CanExecuteChangedWith(this, x => x.CanRefresh);

			OpenPacsDialogCommand = new DelegateCommand(OpenPacsDialog);
			OpenPacsDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenPacsDialog);

			OpenMangoDialogCommand = new DelegateCommand(OpenMangoDialog);
			OpenMangoDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenMangoDialog);

			_operatorClient.StateChanged += OperatorStateChanged;
		}

		#region Pacs

		[PropertyChangedAlso(nameof(BreakState))]
		[PropertyChangedAlso(nameof(PacsState))]
		public virtual Operator Operator
		{
			get => _operatorState;
			set => SetField(ref _operatorState, value);
		}

		public virtual bool PacsEnabled
		{
			get => _pacsEnabled;
			set => SetField(ref _pacsEnabled, value);
		}

		private void OperatorStateChanged(object sender, Operator state)
		{
			_guiDispatcher.RunInGuiTread(() => {
				Operator = state;
			});
		}

		[PropertyChangedAlso(nameof(CanOpenPacsDialog))]
		public virtual PacsState PacsState
		{
			get
			{
				switch(Operator.State)
				{
					case OperatorState.Connected:
						return PacsState.Connected;
					case OperatorState.WaitingForCall:
						return PacsState.WorkShift;
					case OperatorState.Talk:
						return PacsState.Talk;
					case OperatorState.Break:
						return PacsState.Break;
					case OperatorState.Disconnected:
					default:
						return PacsState.Disconnected;
				}
			}
		}

		public bool CanOpenPacsDialog { get; set; }

		private void OpenPacsDialog()
		{
			_navigationManager.OpenViewModel<PacsViewModel>(null);
		}

		#endregion Pacs

		#region Break

		[PropertyChangedAlso(nameof(CanBreak))]
		public virtual BreakState BreakState
		{
			get {
				switch(Operator.State)
				{
					case OperatorState.WaitingForCall:
						return BreakState.CanStartBreak;
					case OperatorState.Break:
						return BreakState.CanEndBreak;
					case OperatorState.New:
					case OperatorState.Connected:
					case OperatorState.Disconnected:
					case OperatorState.Talk:
					default:
						return BreakState.BreakDenied;
				}
			}
		}

		public bool CanBreak =>
			BreakState == BreakState.CanStartBreak ||
			BreakState == BreakState.CanEndBreak;

		private void Break()
		{
			if(BreakState == BreakState.CanStartBreak)
			{
				_operatorClient.StartBreak();
				return;
			}

			if(BreakState == BreakState.CanEndBreak)
			{
				_operatorClient.EndBreak();
				return;
			}
		}

		#endregion Break

		#region Refresh

		public bool CanRefresh { get; }

		private void Refresh()
		{
			_operatorState = _operatorClient.GetState();
		}

		#endregion Refresh

		#region Mango

		[PropertyChangedAlso(nameof(CanOpenMangoDialog))]
		public virtual MangoState MangoState
		{
			get => _mangoState;
			set => SetField(ref _mangoState, value);
		}

		public bool CanOpenMangoDialog { get; set; }

		private void OpenMangoDialog()
		{
			//Встроить MangoManager
		}

		#endregion Mango
	}

	public enum BreakState
	{
		BreakDenied,
		CanStartBreak,
		CanEndBreak	
	}

	public enum PacsState
	{
		Disconnected,
		Connected,
		WorkShift,
		Break,
		Talk
	}

	public enum MangoState
	{
		Disable,
		Disconnected,
		Connected,
		Ring,
		Talk
	}

	public interface IOperatorClient : IObservable<Operator>
	{
		event EventHandler<Operator> StateChanged;
		Operator GetState();

		Operator StartWorkshift(string phoneNumber);
		Operator ChangeNumber(string phoneNumber);
		Operator EndWorkshift();
		Operator StartBreak();
		Operator EndBreak();
	}

	/*
	public class Producer : IObservable<string>
	{
		private List<IObserver<string>> _observers;

		public Producer()
		{
			_observers = new List<IObserver<string>>();
		}

		public IDisposable Subscribe(IObserver<string> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<string>> _observers;
			private readonly IObserver<string> _observer;

			public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
			{
				this._observers = observers;
				this._observer = observer;
			}

			public void Dispose()
			{
				if(_observer == null)
				{
					return;
				}

				if(!_observers.Contains(_observer))
				{
					return;
				}
				
				_observers.Remove(_observer);
			}
		}
	}

	public class Consumer : IObserver<string>
	{
		public void OnCompleted()
		{
			throw new NotImplementedException();
		}

		public void OnError(Exception error)
		{
			throw new NotImplementedException();
		}

		public void OnNext(string value)
		{
			throw new NotImplementedException();
		}
	}*/

	public class PacsViewModel : DialogViewModelBase
	{
		private readonly IPacsViewModelFactory _pacsViewModelFactory;
		private readonly Employee _employee;
		private bool _isOperator;
		private bool _isAdmin;

		public PacsViewModel(IPacsViewModelFactory pacsViewModelFactory, IEmployeeService employeeService, INavigationManager navigation) : base(navigation)
		{
			if(employeeService is null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}
			_pacsViewModelFactory = pacsViewModelFactory ?? throw new ArgumentNullException(nameof(pacsViewModelFactory));

			_employee = employeeService.GetEmployeeForCurrentUser();
			if(_employee == null)
			{
				throw new AbortCreatingPageException("Должен быть привязан сотрудник к пользователю. Обратитесь в отдел кадров.", "На настроен пользователь");
			}

			//Открытие диалога оператора, администратора, (отчетов?)

			if(_isOperator)
			{
				OperatorViewModel = _pacsViewModelFactory.CreateOperatorViewModel();
			}

			if(_isAdmin)
			{
				AdminViewModel = _pacsViewModelFactory.CreateAdminViewModel();
			}
		}

		public PacsOperatorViewModel OperatorViewModel { get; private set; }
		public PacsAdminViewModel AdminViewModel { get; private set; }
		public bool CanEdit { get; private set; }
	}

	public class PacsOperatorViewModel : WidgetViewModelBase
	{
		//Состояние оператора
		//Доступные действия
	}

	public class PacsAdminViewModel : WidgetViewModelBase
	{
		//Сводка по всем операторам
		//Сводка по последним звонкам
		//Отчеты
	}

	public interface IPacsViewModelFactory
	{
		PacsOperatorViewModel CreateOperatorViewModel();
		PacsAdminViewModel CreateAdminViewModel();
	}

	public class PacsViewModelFactory : IPacsViewModelFactory
	{
		private readonly ILifetimeScope _scope;

		public PacsViewModelFactory(ILifetimeScope scope)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		public PacsAdminViewModel CreateAdminViewModel()
		{
			return _scope.Resolve<PacsAdminViewModel>();
		}

		public PacsOperatorViewModel CreateOperatorViewModel()
		{
			return _scope.Resolve<PacsOperatorViewModel>();
		}
	}
}
