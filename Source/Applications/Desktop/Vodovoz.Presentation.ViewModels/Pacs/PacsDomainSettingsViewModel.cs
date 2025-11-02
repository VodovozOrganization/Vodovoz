using Microsoft.Extensions.Logging;
using Pacs.Admin.Client;
using Pacs.Core.Messages.Events;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsDomainSettingsViewModel : WidgetViewModelBase, IObserver<SettingsEvent>
	{
		private readonly ILogger<PacsDomainSettingsViewModel> _logger;
		private readonly Employee _employee;
		private readonly IAdminClient _adminClient;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly bool _canEdit;

		private bool _saveInProgress;
		private IDisposable _settingsSubscription;
		private DomainSettings _originalSettings;
		private DomainSettings _settingsBeforeChange;
		private int _longBreakDuration;
		private int _operatorsOnLongBreak;
		private int _longBreakCountPerDay;
		private int _shortBreakDuration;
		private int _operatorsOnShortBreak;
		private int _shortBreakInterval;

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public PacsDomainSettingsViewModel(
			ILogger<PacsDomainSettingsViewModel> logger,
			IEmployeeService employeeService,
			IPermissionService permissionService,
			IAdminClient adminClient, 
			IObservable<SettingsEvent> settingsPublisher, 
			IGuiDispatcher guiDispatcher)
		{
			if(permissionService is null)
			{
				throw new ArgumentNullException(nameof(permissionService));
			}

			if(settingsPublisher is null)
			{
				throw new ArgumentNullException(nameof(settingsPublisher));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			try
			{
				_originalSettings = _adminClient.GetSettings().Result;
			}
			catch(Exception ex)
			{
				if(ex is AggregateException aggregateException)
				{
					_logger.LogError(ex, "Произошла ошабка при обращении к серверу СКУД: {ExceptionMessage}", string.Join("\n", aggregateException.InnerExceptions.Select(e => e.Message)));
					throw new AbortCreatingPageException(string.Join("\n", aggregateException.InnerExceptions.Select(e => e.Message)), "Сервер СКУД не доступен");
				}
				else
				{
					_logger.LogError(ex, "Произошла ошабка при обращении к серверу СКУД: {ExceptionMessage}", ex.Message);
					throw new AbortCreatingPageException(ex.Message, "Сервер СКУД не доступен");
				}
			}

			_employee = employeeService.GetEmployeeForCurrentUser();
			if(_employee == null)
			{
				throw new AbortCreatingPageException(
					"Должен быть привязан сотрудник к пользователю. Обратитесь в отдел кадров.",
					"Не настроен пользователь");
			}

			_canEdit = permissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.PacsPermissions.IsAdministrator, _employee.User.Id);

			SaveCommand = new DelegateCommand(() => Save().Wait(), () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			CancelCommand = new DelegateCommand(Cancel, () => HasChanges);
			CancelCommand.CanExecuteChangedWith(this, x => x.HasChanges);

			ResetSettings();

			_settingsSubscription = settingsPublisher.Subscribe(this);
		}

		public bool CanEdit => _canEdit;

		public bool HasChanges => CanEdit && _originalSettings != null
			&& (
				LongBreakDuration != (int)_originalSettings.LongBreakDuration.TotalMinutes
				|| OperatorsOnLongBreak != _originalSettings.OperatorsOnLongBreak
				|| LongBreakCountPerDay != _originalSettings.LongBreakCountPerDay
				|| ShortBreakDuration != (int) _originalSettings.ShortBreakDuration.TotalMinutes
				|| OperatorsOnShortBreak != _originalSettings.OperatorsOnShortBreak
				|| ShortBreakInterval != (int)_originalSettings.ShortBreakInterval.TotalMinutes
			)
			;

		public bool HasExternalChanges => _settingsBeforeChange != null 
			&& !_settingsBeforeChange.Equals(_originalSettings);

		public bool CanSave => HasChanges && !_saveInProgress && !HasExternalChanges;

		#region Settings

		public int LongBreakDurationMinValue => 5;
		public int LongBreakDurationMaxValue => 1440;

		[PropertyChangedAlso(nameof(HasChanges))]
		[PropertyChangedAlso(nameof(CanSave))]
		public virtual int LongBreakDuration
		{
			get => _longBreakDuration;
			set
			{
				StartTrackChanges();
				SetField(ref _longBreakDuration, value);
			}
		}


		public int OperatorsOnLongBreakMinValue => 0;
		public int OperatorsOnLongBreakMaxValue => 500;

		[PropertyChangedAlso(nameof(HasChanges))]
		[PropertyChangedAlso(nameof(CanSave))]
		public virtual int OperatorsOnLongBreak
		{
			get => _operatorsOnLongBreak;
			set
			{
				StartTrackChanges();
				SetField(ref _operatorsOnLongBreak, value);
			}
		}

		public int LongBreakCountPerDayMinValue => 1;
		public int LongBreakCountPerDayMaxValue => 5;

		[PropertyChangedAlso(nameof(HasChanges))]
		[PropertyChangedAlso(nameof(CanSave))]
		public virtual int LongBreakCountPerDay
		{
			get => _longBreakCountPerDay;
			set
			{
				StartTrackChanges();
				SetField(ref _longBreakCountPerDay, value);
			}
		}


		public int ShortBreakDurationMinValue => 5;
		public int ShortBreakDurationMaxValue => 500;

		[PropertyChangedAlso(nameof(HasChanges))]
		[PropertyChangedAlso(nameof(CanSave))]
		public virtual int ShortBreakDuration
		{
			get => _shortBreakDuration;
			set
			{
				StartTrackChanges();
				SetField(ref _shortBreakDuration, value);
			}
		}


		public int OperatorsOnShortBreakMinValue => 0;
		public int OperatorsOnShortBreakMaxValue => 500;

		[PropertyChangedAlso(nameof(HasChanges))]
		[PropertyChangedAlso(nameof(CanSave))]
		public virtual int OperatorsOnShortBreak
		{
			get => _operatorsOnShortBreak;
			set
			{
				StartTrackChanges();
				SetField(ref _operatorsOnShortBreak, value);
			}
		}


		public int ShortBreakIntervalMinValue => 5;
		public int ShortBreakIntervalMaxValue => 500;

		[PropertyChangedAlso(nameof(HasChanges))]
		[PropertyChangedAlso(nameof(CanSave))]
		public virtual int ShortBreakInterval
		{
			get => _shortBreakInterval;
			set
			{
				StartTrackChanges();
				SetField(ref _shortBreakInterval, value);
			}
		}

		#endregion Settings

		#region Settings subscription

		public void OnCompleted()
		{
			_settingsSubscription.Dispose();
			_settingsSubscription = null;
		}

		public void OnError(Exception error)
		{
			_logger.LogError(error, "Возникло исключение в процессе уведомления об изменении настроек СКУД");
		}

		public void OnNext(SettingsEvent value)
		{
			_guiDispatcher.RunInGuiTread(() => {
				_originalSettings = value.Settings;
				OnPropertyChanged(nameof(HasChanges));
				OnPropertyChanged(nameof(CanSave));
				OnPropertyChanged(nameof(HasExternalChanges));
			});
		}

		#endregion Settings subscription

		private async Task Save()
		{
			try
			{
				_saveInProgress = true;

				var newSettings = new DomainSettings
				{
					AdministratorId = _employee.Id,
					Timestamp = DateTime.Now,
					LongBreakDuration = TimeSpan.FromMinutes(LongBreakDuration),
					OperatorsOnLongBreak = OperatorsOnLongBreak,
					LongBreakCountPerDay = LongBreakCountPerDay,
					ShortBreakDuration = TimeSpan.FromMinutes(ShortBreakDuration),
					OperatorsOnShortBreak = OperatorsOnShortBreak,
					ShortBreakInterval = TimeSpan.FromMinutes(ShortBreakInterval),
				};

				await _adminClient.SetSettings(newSettings);
				_originalSettings = newSettings;
			}
			finally
			{
				_saveInProgress = false;
				StopTrackChanges();
				_guiDispatcher.RunInGuiTread(() =>
				{
					OnPropertyChanged(nameof(HasChanges));
					OnPropertyChanged(nameof(CanSave));
					OnPropertyChanged(nameof(HasExternalChanges));
				});
			}
		}

		public void Cancel()
		{
			ResetSettings();
		}

		private void ResetSettings()
		{
			_longBreakDuration = (int)_originalSettings.LongBreakDuration.TotalMinutes;
			_operatorsOnLongBreak = _originalSettings.OperatorsOnLongBreak;
			_longBreakCountPerDay = _originalSettings.LongBreakCountPerDay;
			_shortBreakDuration = (int)_originalSettings.ShortBreakDuration.TotalMinutes;
			_operatorsOnShortBreak = _originalSettings.OperatorsOnShortBreak;
			_shortBreakInterval = (int)_originalSettings.ShortBreakInterval.TotalMinutes;

			StopTrackChanges();
			OnPropertyChanged(nameof(HasChanges));
			OnPropertyChanged(nameof(HasExternalChanges));
			OnPropertyChanged(nameof(LongBreakDuration));
			OnPropertyChanged(nameof(OperatorsOnLongBreak));
			OnPropertyChanged(nameof(LongBreakCountPerDay));
			OnPropertyChanged(nameof(ShortBreakDuration));
			OnPropertyChanged(nameof(OperatorsOnShortBreak));
			OnPropertyChanged(nameof(ShortBreakInterval));
		}

		private void StartTrackChanges()
		{
			if(_settingsBeforeChange == null)
			{
				_settingsBeforeChange = _originalSettings;
			}
		}

		private void StopTrackChanges()
		{
			_settingsBeforeChange = null;
		}
	}
}
