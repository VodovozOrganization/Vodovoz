using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Problem.Routine.Services;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.ViewModels.TrueMark.CodesPool
{
	public partial class CodesPoolViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<CodesPoolViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFileDialogService _fileDialogService;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly TrueMarkCodePoolLoader _codePoolLoader;
		private readonly TrueMarkCodesPoolManager _trueMarkCodesPoolManager;
		private readonly IEdoRepository _edoRepository;
		private readonly OrderEdoCodePoolMissingProblemService _codePoolMissingProblemService;

		private IEnumerable<CodesPoolDataNode> _codesPoolData = new List<CodesPoolDataNode>();
		private bool _isDataRefreshInProgress;
		private IEnumerable<CodesPoolProblemDataNode> _codesPoolProblemData = new List<CodesPoolProblemDataNode>();
		private DateTime _edoProblemStartDate = DateTime.Now.Date.AddDays(-7);
		private DateTime _edoProblemEndDate = DateTime.Now;
		private string _edoTaskIdForSearch;
		private CodesPoolProblemDataNode _selectedCodesPoolProblemNode;
		private bool _isProblemsDataRefreshInProgress;
		private bool _isResendEnable;

		private List<string> _targetProblems = new List<string>()
		{
			"EdoCodePoolMissingCodeException"
		};
		
		public CodesPoolViewModel(
			ILogger<CodesPoolViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IGuiDispatcher guiDispatcher,
			IFileDialogService fileDialogService,
			ITrueMarkRepository trueMarkRepository,
			TrueMarkCodePoolLoader codePoolLoader,
			TrueMarkCodesPoolManager trueMarkCodesPoolManager,
			IEdoRepository edoRepository,
			OrderEdoCodePoolMissingProblemService codePoolMissingProblemService
			)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fileDialogService = fileDialogService ?? throw new System.ArgumentNullException(nameof(fileDialogService));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_codePoolLoader = codePoolLoader ?? throw new System.ArgumentNullException(nameof(codePoolLoader));
			_trueMarkCodesPoolManager = trueMarkCodesPoolManager ?? throw new System.ArgumentNullException(nameof(trueMarkCodesPoolManager));
			_edoRepository = edoRepository ?? throw new System.ArgumentNullException(nameof(edoRepository));
			_codePoolMissingProblemService = codePoolMissingProblemService ?? throw new System.ArgumentNullException(nameof(codePoolMissingProblemService));

			Title = "Пул кодов маркировки";

			RefreshCommand = new AsyncCommand(guiDispatcher, UpdateCodesPoolData, () => !IsDataRefreshInProgress);
			RefreshCommand.CanExecuteChangedWith(this, vm => vm.IsDataRefreshInProgress);

			LoadCodesToPoolCommand = new DelegateCommand(LoadCodesToPool);
			
			RefreshProblemsCommand = new AsyncCommand(guiDispatcher, UpdateEdoProblemsData, () => !IsProblemsDataRefreshInProgress);
			RefreshCommand.CanExecuteChangedWith(this, vm => vm.IsProblemsDataRefreshInProgress);

			ResendEdoTaskCommand = new AsyncCommand(guiDispatcher, ResendEdoTaskCommandHandler);
			
			RefreshCommand.Execute();
			RefreshProblemsCommand.Execute();
		}

		public IEnumerable<CodesPoolDataNode> CodesPoolData
		{
			get => _codesPoolData;
			set => SetField(ref _codesPoolData, value);
		}

		public IEnumerable<CodesPoolProblemDataNode> CodesPoolProblemData
		{
			get => _codesPoolProblemData;
			set => SetField(ref _codesPoolProblemData, value);
		}

		public CodesPoolProblemDataNode SelectedCodesPoolProblemNode
		{
			get => _selectedCodesPoolProblemNode;
			set
			{
				SetField(ref _selectedCodesPoolProblemNode, value);
				IsResendEnable = value != null;
			}
		}

		public bool IsResendEnable
		{
			get => _isResendEnable;
			set => SetField(ref _isResendEnable, value);
		}

		public DateTime EdoProblemStartDate
		{
			get => _edoProblemStartDate;
			set => SetField(ref _edoProblemStartDate, value);
		}

		public DateTime EdoProblemEndDate
		{
			get => _edoProblemEndDate;
			set => SetField(ref _edoProblemEndDate, value);
		}

		public string EdoTaskIdForSearch
		{
			get => _edoTaskIdForSearch;
			set => SetField(ref _edoTaskIdForSearch, value);
		}

		public bool IsDataRefreshInProgress
		{
			get => _isDataRefreshInProgress;
			set => SetField(ref _isDataRefreshInProgress, value);
		}

		public bool IsProblemsDataRefreshInProgress
		{
			get => _isProblemsDataRefreshInProgress;
			set => SetField(ref _isProblemsDataRefreshInProgress, value);
		}

		public AsyncCommand RefreshCommand { get; }
		public DelegateCommand LoadCodesToPoolCommand { get; }
		
		public AsyncCommand RefreshProblemsCommand { get; }
		
		public AsyncCommand ResendEdoTaskCommand { get; }

		private async Task UpdateCodesPoolData(CancellationToken cancellationToken)
		{
			if(IsDataRefreshInProgress)
			{
				return;
			}

			var codesPoolData = new List<CodesPoolDataNode>();

			IsDataRefreshInProgress = true;

			try
			{
				codesPoolData = (await GetCodesPoolData(cancellationToken)).ToList();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обновлении данных по кодам в пуле возникла ошибка: {ExceptionMessage}", ex.Message);

				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"При обновлении данных по кодам в пуле возникла ошибка: {ex.Message}");
				});
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					CodesPoolData = codesPoolData;
					IsDataRefreshInProgress = false;
				});
			}
		}

		private async Task<IEnumerable<CodesPoolDataNode>> GetCodesPoolData(CancellationToken cancellationToken)
		{
			var gtinsData = await _trueMarkRepository.GetGtinsNomenclatureData(UoW, cancellationToken);

			var codesInPool = await _trueMarkCodesPoolManager.GetTotalCountByGtinAsync(cancellationToken);
			var soldYesterdayGtinsCount = await _trueMarkRepository.GetSoldYesterdayGtinsCount(UoW, cancellationToken);
			var missingCodesCount = await _trueMarkRepository.GetMissingCodesCount(UoW, cancellationToken);

			var codesPoolDataNodes = new List<CodesPoolDataNode>();

			foreach(var gtinData in gtinsData)
			{
				codesInPool.TryGetValue(gtinData.Key, out var countInPool);
				soldYesterdayGtinsCount.TryGetValue(gtinData.Key, out var soldYesterdayCount);
				missingCodesCount.TryGetValue(gtinData.Key, out var gtinMissingCodesCount);

				var dataNode = new CodesPoolDataNode
				{
					Gtin = gtinData.Key,
					Nomenclatures = string.Join(" | ", gtinData.Value),
					CountInPool = (int)countInPool,
					SoldYesterday = soldYesterdayCount,
					MissingCodesInOrdersCount = gtinMissingCodesCount
				};

				codesPoolDataNodes.Add(dataNode);
			}

			return codesPoolDataNodes;
		}

		#region Edo problems

		private async Task UpdateEdoProblemsData(CancellationToken cancellationToken)
		{
			if(IsProblemsDataRefreshInProgress)
			{
				return;
			}

			var poolProblemDataNodes = new List<CodesPoolProblemDataNode>();

			IsProblemsDataRefreshInProgress = true;

			try
			{
				poolProblemDataNodes = (await GetProblemsEdoData(cancellationToken)).ToList();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обновлении данных по ошибкам ЭДО задач возникла ошибка: {ExceptionMessage}", ex.Message);

				_guiDispatcher.RunInGuiTread(() =>
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"При обновлении данных по ошибкам ЭДО задач возникла ошибка: {ex.Message}");
				});
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					CodesPoolProblemData = poolProblemDataNodes;
					IsProblemsDataRefreshInProgress = false;
				});
			}
		}

		private async Task<IEnumerable<CodesPoolProblemDataNode>> GetProblemsEdoData(CancellationToken cancellationToken)
		{
			var codesPoolProblemDataNodes = new List<CodesPoolProblemDataNode>();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot(nameof(CodesPoolViewModel)))
			{
				var problemName = _targetProblems.First();
				
				var documentTasks =
					await _edoRepository.GetProblemEdoTasks<DocumentEdoTask>(uow, problemName, EdoProblemStartDate, cancellationToken, EdoProblemEndDate);
				var receiptTasks =
					await _edoRepository.GetProblemEdoTasks<ReceiptEdoTask>(uow, problemName, EdoProblemStartDate, cancellationToken, EdoProblemEndDate);
				var tenderTasks =
					await _edoRepository.GetProblemEdoTasks<TenderEdoTask>(uow, problemName, EdoProblemStartDate, cancellationToken, EdoProblemEndDate);

				var tasks = documentTasks
					.Concat<OrderEdoTask>(receiptTasks)
					.Concat(tenderTasks)
					.ToList();

				_logger.LogInformation("Найдено {Count} задач ЭДО с активной проблемой {ProblemName}",
					tasks.Count, problemName);

				foreach(var edoTask in tasks)
				{
					if(string.IsNullOrEmpty(EdoTaskIdForSearch))
					{
						codesPoolProblemDataNodes.Add(new CodesPoolProblemDataNode
						{
							EdoTask = edoTask,
							EdoTaskId = edoTask.Id,
							OrderId = edoTask.FormalEdoRequest.Order.Id,
							ErrorName = problemName,
							EdoTaskStartDate = edoTask.CreationTime
						});
					}
					else
					{
						if(edoTask.Id.ToString().Contains(EdoTaskIdForSearch) ||
						   edoTask.FormalEdoRequest.Order.Id.ToString().Contains(EdoTaskIdForSearch))
						{
							codesPoolProblemDataNodes.Add(new CodesPoolProblemDataNode
							{
								EdoTask = edoTask,
								EdoTaskId = edoTask.Id,
								OrderId = edoTask.FormalEdoRequest.Order.Id,
								ErrorName = problemName,
								EdoTaskStartDate = edoTask.CreationTime
							});
						}
					}
				}
			}
			
			return codesPoolProblemDataNodes;
		}
		
		private async Task ResendEdoTaskCommandHandler(CancellationToken cancellationToken)
		{
			if(SelectedCodesPoolProblemNode == null)
			{
				return;
			}

			try
			{
				if(SelectedCodesPoolProblemNode.EdoTask is OrderEdoTask orderEdoTask)
				{
					await _codePoolMissingProblemService.TryResumeTask(orderEdoTask, cancellationToken);
					
					_interactiveService.ShowMessage(ImportanceLevel.Info,
						$"Задача #{orderEdoTask.Id} запущена на повторную отправку");
				}
				else
				{
					_interactiveService.ShowMessage(ImportanceLevel.Info,
						"Данную задачу нельзя переотправить");
				}
			}
			catch(Exception e)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error,
					"Произошла ошибка при переотправке задачи: " + 
					e.Message);
			}
		}

		#endregion
		
		private void LoadCodesToPool()
		{
			var dialogSettings = CreateDialogSettings();

			var result = _fileDialogService.RunOpenFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			try
			{
				var loadingResult = _codePoolLoader.LoadFromFile(result.Path);

				_interactiveService.ShowMessage(ImportanceLevel.Info,
					$"Найдено кодов: {loadingResult.TotalFound}\n" +
					$"Загружено: {loadingResult.SuccessfulLoaded}\n" +
					$"Уже существуют в системе: {loadingResult.TotalFound - loadingResult.SuccessfulLoaded}");
			}
			catch(IOException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, ex.Message);
			}
		}

		private DialogSettings CreateDialogSettings()
		{
			var dialogSettings = new DialogSettings
			{
				SelectMultiple = false,
				Title = "Выберите файл содержащий коды"
			};
			dialogSettings.FileFilters.Add(new DialogFileFilter("Файлы содержащие коды", "*.xlsx", "*.mxl", "*.csv", "*.txt"));

			return dialogSettings;
		}
	}
}
