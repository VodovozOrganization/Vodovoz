using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Services;
using QS.ViewModels;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Settings;
using Vodovoz.ViewModels.AdministrationTools.OrdersLoadTesting;

namespace Vodovoz.ViewModels.AdministrationTools
{
	public class OrdersLoadTestingToolViewModel : DialogTabViewModelBase
	{
		private const int DefaultThreadCount = 10;
		private const int MinThreadCount = 1;
		private const int MaxThreadCount = 100;
		private const int MaxLogLength = 100_000;

		private readonly IInteractiveService _interactiveService;
		private readonly IUserService _userService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly OrdersLoadTestingRunner _runner;
		private readonly StringBuilder _logBuilder = new StringBuilder();
		private readonly object _logSync = new object();

		private int _threadCount = DefaultThreadCount;
		private bool _isRunning;
		private string _logText = string.Empty;
		private string _statusText = "Остановлено";
		private CancellationTokenSource _cancellationTokenSource;

		public OrdersLoadTestingToolViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IUserService userService,
			IEmployeeRepository employeeRepository,
			IDataBaseInfo dataBaseInfo,
			ISettingsController settingsController)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_runner = new OrdersLoadTestingRunner(
				unitOfWorkFactory,
				dataBaseInfo,
				settingsController);

			TabName = "Нагрузочное тестирование заказов и МЛ";

			StartCommand = new DelegateCommand(
				() => Start(),
				() => !IsRunning);
			StartCommand.CanExecuteChangedWith(this, vm => vm.IsRunning);

			StopCommand = new DelegateCommand(
				Stop,
				() => IsRunning);
			StopCommand.CanExecuteChangedWith(this, vm => vm.IsRunning);

			ClearLogCommand = new DelegateCommand(ClearLog);

			AppendLog(
				$"БД: «{_runner.CurrentDatabaseName}». " +
				$"Тестовая (Pacs.Test.Database): «{_runner.ExpectedTestDatabaseName}». " +
				$"Режим теста: {(_runner.IsTestDatabase() ? "да" : "нет")}.");
		}

		public int ThreadCount
		{
			get => _threadCount;
			set
			{
				var normalized = Math.Max(MinThreadCount, Math.Min(MaxThreadCount, value));
				SetField(ref _threadCount, normalized);
			}
		}

		public bool IsRunning
		{
			get => _isRunning;
			private set
			{
				if(SetField(ref _isRunning, value))
				{
					OnPropertyChanged(nameof(CanEditThreadCount));
				}
			}
		}

		public bool CanEditThreadCount => !IsRunning;

		public string LogText
		{
			get => _logText;
			private set => SetField(ref _logText, value);
		}

		public string StatusText
		{
			get => _statusText;
			private set => SetField(ref _statusText, value);
		}

		public DelegateCommand StartCommand { get; }
		public DelegateCommand StopCommand { get; }
		public DelegateCommand ClearLogCommand { get; }

		/// <summary>
		/// Выставляет View (GTK) для выполнения действий в UI-потоке.
		/// </summary>
		public Action<Action> UiMarshal { get; set; }

		public override bool HasChanges
		{
			get => false;
			set { }
		}

		public override void Dispose()
		{
			Stop();
			base.Dispose();
		}

		private void Start()
		{
			_ = StartAsync();
		}

		private async Task StartAsync()
		{
			if(IsRunning)
			{
				return;
			}

			if(!_userService.GetCurrentUser().IsAdmin)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Доступно только администраторам.");
				return;
			}

			if(!_runner.IsTestDatabase())
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Запуск разрешён только на тестовой БД.\n\n" +
					$"Текущая: «{_runner.CurrentDatabaseName}»\n" +
					$"Ожидается (Pacs.Test.Database): «{_runner.ExpectedTestDatabaseName}»");
				return;
			}

			if(ThreadCount < MinThreadCount || ThreadCount > MaxThreadCount)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Количество потоков должно быть от {MinThreadCount} до {MaxThreadCount}.");
				return;
			}

			Employee author;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot(TabName))
			{
				author = _employeeRepository.GetEmployeeForCurrentUser(uow);
			}

			if(author == null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Не найден сотрудник для текущего пользователя. Генерация невозможна.");
				return;
			}

			_cancellationTokenSource = new CancellationTokenSource();
			IsRunning = true;
			StatusText = $"Работает ({ThreadCount} поток(ов))…";
			AppendLog($"Старт вставки: потоков={ThreadCount}, автор={author.ShortName} (Id={author.Id}).");

			try
			{
				await _runner.RunAsync(
					ThreadCount,
					author,
					_cancellationTokenSource.Token,
					AppendLog).ConfigureAwait(true);
			}
			catch(OperationCanceledException)
			{
				AppendLog("Генерация отменена.");
			}
			catch(AggregateException aggregateException)
			{
				var message = FormatAggregateError(aggregateException);
				AppendLog($"ОСТАНОВКА: {message}");
				ShowError(message);
			}
			catch(Exception ex)
			{
				AppendLog($"ОСТАНОВКА: {ex.Message}");
				ShowError(ex.Message);
			}
			finally
			{
				RunOnUi(() =>
				{
					IsRunning = false;
					StatusText = "Остановлено";
				});
				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
			}
		}

		private void Stop()
		{
			if(_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
			{
				return;
			}

			AppendLog("Запрошена остановка…");
			_cancellationTokenSource.Cancel();
		}

		private void ClearLog()
		{
			lock(_logSync)
			{
				_logBuilder.Clear();
				LogText = string.Empty;
			}
		}

		private void AppendLog(string message)
		{
			if(string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			RunOnUi(() => AppendLogCore(message));
		}

		private void AppendLogCore(string message)
		{
			var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

			lock(_logSync)
			{
				_logBuilder.Append(line);
				if(_logBuilder.Length > MaxLogLength)
				{
					_logBuilder.Remove(0, _logBuilder.Length - MaxLogLength);
				}

				LogText = _logBuilder.ToString();
			}
		}

		private void ShowError(string message)
		{
			RunOnUi(() => _interactiveService.ShowMessage(ImportanceLevel.Error, message));
		}

		private void RunOnUi(Action action)
		{
			if(action == null)
			{
				return;
			}

			if(UiMarshal != null)
			{
				UiMarshal(action);
				return;
			}

			action();
		}

		private static string FormatAggregateError(AggregateException aggregateException)
		{
			aggregateException = aggregateException.Flatten();
			if(aggregateException.InnerExceptions.Count == 1)
			{
				return FormatExceptionChain(aggregateException.InnerExceptions[0]);
			}

			return FormatExceptionChain(aggregateException);
		}

		private static string FormatExceptionChain(Exception ex)
		{
			var sb = new StringBuilder();
			var current = ex;
			while(current != null)
			{
				if(sb.Length > 0)
				{
					sb.AppendLine().Append("→ ");
				}

				sb.Append(current.Message);
				current = current.InnerException;
			}

			return sb.ToString();
		}
	}
}
