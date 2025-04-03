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
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.ViewModels.TrueMark.CodesPool
{
	public partial class CodesPoolViewModel : DialogTabViewModelBase
	{
		private IEnumerable<CodesPoolDataNode> _codesPoolData = new List<CodesPoolDataNode>();
		private readonly ILogger<CodesPoolViewModel> _logger;
		private readonly IInteractiveService _interactiveService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFileDialogService _fileDialogService;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly TrueMarkCodePoolLoader _codePoolLoader;
		private readonly TrueMarkCodesPoolManager _trueMarkCodesPoolManager;

		private bool _isDataRefreshInProgress;

		public CodesPoolViewModel(
			ILogger<CodesPoolViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IGuiDispatcher guiDispatcher,
			IFileDialogService fileDialogService,
			ITrueMarkRepository trueMarkRepository,
			TrueMarkCodePoolLoader codePoolLoader,
			TrueMarkCodesPoolManager trueMarkCodesPoolManager)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fileDialogService = fileDialogService ?? throw new System.ArgumentNullException(nameof(fileDialogService));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_codePoolLoader = codePoolLoader ?? throw new System.ArgumentNullException(nameof(codePoolLoader));
			_trueMarkCodesPoolManager = trueMarkCodesPoolManager ?? throw new System.ArgumentNullException(nameof(trueMarkCodesPoolManager));

			Title = "Пул кодов маркировки";

			RefreshCommand = new AsyncCommand(guiDispatcher, UpdateCodesPoolData, () => !IsDataRefreshInProgress);
			RefreshCommand.CanExecuteChangedWith(this, vm => vm.IsDataRefreshInProgress);

			LoadCodesToPoolCommand = new DelegateCommand(LoadCodesToPool);

			RefreshCommand.Execute();
		}

		public IEnumerable<CodesPoolDataNode> CodesPoolData
		{
			get => _codesPoolData;
			set => SetField(ref _codesPoolData, value);
		}

		public bool IsDataRefreshInProgress
		{
			get => _isDataRefreshInProgress;
			set => SetField(ref _isDataRefreshInProgress, value);
		}

		public AsyncCommand RefreshCommand { get; }
		public DelegateCommand LoadCodesToPoolCommand { get; }

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
