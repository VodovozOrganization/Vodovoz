using QS.Commands;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsPanelViewModel : WidgetViewModelBase
	{
		private readonly IOperatorClient _operatorClient;
		private readonly INavigationManager _navigationManager;

		private bool _pacsEnabled;
		private object _operatorState;
		private BreakState _breakState;
		private PacsState _pacsState;
		private MangoState _mangoState;

		public DelegateCommand BreakCommand { get; }
		public DelegateCommand RefreshCommand { get; }
		public DelegateCommand OpenPacsDialogCommand { get; }
		public DelegateCommand OpenMangoDialogCommand { get; }

		public PacsPanelViewModel(IOperatorClient operatorClient, INavigationManager navigationManager)
		{
			_operatorClient = operatorClient ?? throw new ArgumentNullException(nameof(operatorClient));
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

		public virtual bool PacsEnabled
		{
			get => _pacsEnabled;
			set => SetField(ref _pacsEnabled, value);
		}

		private void OperatorStateChanged(object sender, object state)
		{
			_operatorState = state;
		}


		[PropertyChangedAlso(nameof(CanBreak))]
		public virtual BreakState BreakState
		{
			get => _breakState;
			set => SetField(ref _breakState, value);
		}

		public bool CanBreak =>
			BreakState == BreakState.CanStartBreak ||
			BreakState == BreakState.CanEndBreak;

		private void Break()
		{
			_operatorClient.StartBreak();
		}


		public bool CanRefresh { get; }

		private void Refresh()
		{
			_operatorState = _operatorClient.GetState();
		}

		[PropertyChangedAlso(nameof(CanOpenPacsDialog))]
		public virtual PacsState PacsState
		{
			get => _pacsState;
			set => SetField(ref _pacsState, value);
		}


		public bool CanOpenPacsDialog { get; set; }

		private void OpenPacsDialog()
		{

		}

		[PropertyChangedAlso(nameof(CanOpenMangoDialog))]
		public virtual MangoState MangoState
		{
			get => _mangoState;
			set => SetField(ref _mangoState, value);
		}


		public bool CanOpenMangoDialog { get; set; }

		private void OpenMangoDialog()
		{

		}

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

	public interface IOperatorClient
	{
		event EventHandler<object> StateChanged;
		object GetState();

		void StartWorkshift(string phoneNumber);
		void ChangeNumber(string phoneNumber);
		void EndWorkshift();
		void StartBreak();
		void EndBreak();
	}
}
