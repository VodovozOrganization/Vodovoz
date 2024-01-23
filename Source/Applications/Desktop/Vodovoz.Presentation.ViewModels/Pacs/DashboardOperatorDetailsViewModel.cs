using Core.Infrastructure;
using Pacs.Admin.Client;
using Pacs.Core;
using Pacs.Server;
using QS.Commands;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardOperatorDetailsViewModel : WidgetViewModelBase
	{
		private readonly OperatorModel _model;
		private readonly AdminClient _adminClient;
		private readonly IInteractiveService _interactiveService;
		private string _tittle;
		private string _breakReason;

		public DelegateCommand StartLongBreakCommand { get; set; }
		public DelegateCommand StartShortBreakCommand { get; set; }
		public DelegateCommand EndBreakCommand { get; set; }

		public DashboardOperatorDetailsViewModel(OperatorModel model, AdminClient adminClient, IInteractiveService interactiveService)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			Tittle = $"История оператора: {_model.Employee.GetPersonNameWithInitials()}";

			StartLongBreakCommand = new DelegateCommand(StartLongBreak, () => CanStartBreak);
			StartLongBreakCommand.CanExecuteChangedWith(this, x => x.CanStartBreak);
			StartShortBreakCommand = new DelegateCommand(StartShortBreak, () => CanStartBreak);
			StartShortBreakCommand.CanExecuteChangedWith(this, x => x.CanStartBreak);
			EndBreakCommand = new DelegateCommand(EndBreak, () => CanEndBreak);
			EndBreakCommand.CanExecuteChangedWith(this, x => x.CanEndBreak);

			_model.PropertyChanged += OnStateChange;
		}

		private void OnStateChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName != nameof(OperatorModel.CurrentState))
			{
				return;
			}

			OnPropertyChanged(nameof(CanStartBreak));
			OnPropertyChanged(nameof(CanEndBreak));
		}

		public virtual string Tittle
		{
			get => _tittle;
			set => SetField(ref _tittle, value);
		}

		public GenericObservableList<OperatorState> States => _model.States;

		public virtual string BreakReason
		{
			get => _breakReason;
			set
			{
				if(SetField(ref _breakReason, value))
				{
					OnPropertyChanged(nameof(CanStartBreak));
					OnPropertyChanged(nameof(CanEndBreak));
				}
			}
		}

		public bool CanStartBreak => _model.Agent.CanStartBreak && !BreakReason.IsNullOrWhiteSpace();

		private void StartLongBreak()
		{
			try
			{
				_adminClient.StartBreak(_model.CurrentState.OperatorId, BreakReason, OperatorBreakType.Long).Wait();
				ClearReason();
			}
			catch(PacsException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
			}
		}

		private void StartShortBreak()
		{
			try
			{
				_adminClient.StartBreak(_model.CurrentState.OperatorId, BreakReason, OperatorBreakType.Short).Wait();
				ClearReason();
			}
			catch(PacsException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
			}
		}

		public bool CanEndBreak => _model.Agent.CanEndBreak && !BreakReason.IsNullOrWhiteSpace();

		private void EndBreak()
		{
			try
			{
				_adminClient.EndBreak(_model.CurrentState.OperatorId, BreakReason).Wait();
				ClearReason();
			}
			catch(PacsException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
			}
		}

		private void ClearReason()
		{
			BreakReason = "";
		}
	}

	public class DashboardCallDetailsViewModel : WidgetViewModelBase
	{
		private readonly CallModel _model;

		private string _detailsInfo;

		public DashboardCallDetailsViewModel(CallModel model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			DetailsInfo = "Детализация звонка";
		}

		public virtual string DetailsInfo
		{
			get => _detailsInfo;
			set => SetField(ref _detailsInfo, value);
		}

		public GenericObservableList<CallEvent> CallEvents => _model.CallEvents;
	}

	public class DashboardMissedCallDetailsViewModel : WidgetViewModelBase
	{
		private readonly MissedCallModel _model;

		private string _details;

		public DashboardMissedCallDetailsViewModel(MissedCallModel model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));

			Details =
$@"
Пропущенный звонок: {_model.Call.CurrentState.FromNumber}
Начат: {_model.Call.Started.ToString("MM.dd HH:mm")}
Завершен: {_model.Call.Ended.ToString("MM.dd HH:mm")}
Ожидание: {_model.Call.Duration.ToString("hh\\:mm\\:ss")}
Могли принять {_model.PossibleOperatorsCount} операторов:
{string.Join("\n", _model.PossibleOperators.Select(x => $"{x.Employee.GetPersonNameWithInitials()}. Тел. {x.CurrentState.PhoneNumber}"))}
";
		}

		public virtual string Details
		{
			get => _details;
			set => SetField(ref _details, value);
		}
	}
}
