using Core.Infrastructure;
using Pacs.Admin.Client;
using Pacs.Core;
using QS.Commands;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardOperatorDetailsViewModel : WidgetViewModelBase
	{
		private readonly OperatorModel _model;
		private readonly IAdminClient _adminClient;
		private readonly IInteractiveService _interactiveService;
		private string _title;
		private string _breakReason;
		private string _endWorkShiftReason;

		public DashboardOperatorDetailsViewModel(OperatorModel model, IAdminClient adminClient, IInteractiveService interactiveService)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			Title = $"История оператора: {_model.Employee.GetPersonNameWithInitials()}";

			StartLongBreakCommand = new DelegateCommand(StartLongBreak, () => CanStartBreak);
			StartLongBreakCommand.CanExecuteChangedWith(this, x => x.CanStartBreak);
			StartShortBreakCommand = new DelegateCommand(StartShortBreak, () => CanStartBreak);
			StartShortBreakCommand.CanExecuteChangedWith(this, x => x.CanStartBreak);
			EndBreakCommand = new DelegateCommand(EndBreak, () => CanEndBreak);
			EndBreakCommand.CanExecuteChangedWith(this, x => x.CanEndBreak);
			EndWorkShiftCommand = new DelegateCommand(EndWorkShift, () => CanEndWorkShift);
			EndWorkShiftCommand.CanExecuteChangedWith(this, x => x.CanEndWorkShift);

			_model.PropertyChanged += OnStateChange;
		}

		public DelegateCommand StartLongBreakCommand { get; }
		public DelegateCommand StartShortBreakCommand { get; }
		public DelegateCommand EndBreakCommand { get; }
		public DelegateCommand EndWorkShiftCommand { get; }

		public virtual string Title
		{
			get => _title;
			set => SetField(ref _title, value);
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

		public virtual string EndWorkShiftReason
		{
			get => _endWorkShiftReason;
			set
			{
				if(SetField(ref _endWorkShiftReason, value))
				{
					OnPropertyChanged(nameof(CanEndWorkShift));
				}
			}
		}

		public bool CanStartBreak => _model.Agent.CanStartBreak && !BreakReason.IsNullOrWhiteSpace();
		public bool CanEndBreak => _model.Agent.CanEndBreak && !BreakReason.IsNullOrWhiteSpace();
		public bool CanEndWorkShift => !EndWorkShiftReason.IsNullOrWhiteSpace();

		private void OnStateChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName != nameof(OperatorModel.CurrentState))
			{
				return;
			}

			OnPropertyChanged(nameof(CanStartBreak));
			OnPropertyChanged(nameof(CanEndBreak));
		}

		private void StartLongBreak()
		{
			try
			{
				_adminClient.StartBreak(_model.CurrentState.OperatorId, BreakReason, OperatorBreakType.Long).Wait();
				ClearBreakReason();
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
				ClearBreakReason();
			}
			catch(PacsException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
			}
		}

		private void EndBreak()
		{
			try
			{
				_adminClient.EndBreak(_model.CurrentState.OperatorId, BreakReason).Wait();
				ClearBreakReason();
			}
			catch(PacsException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
			}
		}

		private void EndWorkShift()
		{
			try
			{
				_adminClient.EndWorkShift(_model.CurrentState.OperatorId, EndWorkShiftReason).Wait();
				CleanEndWorkShiftReason();
			}
			catch(PacsException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, ex.Message);
			}
		}

		private void ClearBreakReason()
		{
			BreakReason = string.Empty;
		}

		private void CleanEndWorkShiftReason()
		{
			EndWorkShiftReason = string.Empty;
		}
	}
}
