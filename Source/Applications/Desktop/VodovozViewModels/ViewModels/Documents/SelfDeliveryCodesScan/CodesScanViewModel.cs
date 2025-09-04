using Edo.Common;
using Edo.Contracts.Messages.Events;
using Gamma.Binding.Core.RecursiveTreeConfig;
using MassTransit;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Models.TrueMark;
using VodovozBusiness.Domain.Goods;
using VodovozBusiness.Services.TrueMark;
using VodovozInfrastructure.Keyboard;
using static Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan.CodesScanViewModel;

namespace Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan
{
	public partial class CodesScanViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly SelfDeliveryDocument _selfDeliveryDocument;
		private readonly IUnitOfWork _unitOfWork;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService _codesProcessingService;
		private readonly IGenericRepository<GroupGtin> _groupGtinrepository;
		private readonly IGenericRepository<Gtin> _gtinRepository;
		private readonly ITrueMarkCodesValidator _trueMarkValidator;
		private readonly IBus _messageBus;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<TrueMarkProductCode> _trueMarkProductCodesRepository;
		private readonly ILogger<CodesScanViewModel> _logger;
		private string _organizationInn;
		private List<GtinFromNomenclatureDto> _allGtins;
		private List<GtinFromNomenclatureDto> _allGroupGtins;
		private List<string> _gtinsInOrder;
		private readonly BlockingCollection<CodeToCheck> _newCodesToCheck = new BlockingCollection<CodeToCheck>();
		private readonly BlockingCollection<CodeToCheck> _oldCodesToRechek = new BlockingCollection<CodeToCheck>();
		private CancellationTokenSource _cancelationTokenSource;
		private CodeScanRow _selectedRow;

		private bool _scannedStagingCodesUpdateInProgress = false;
		private bool _isAllCodesScanned = false;

		private IDictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>> _scannedStagingCodes =
			new Dictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>>();

		public CodesScanViewModel(
			ILogger<CodesScanViewModel> logger,
			INavigationManager navigationManager,
			SelfDeliveryDocument selfDeliveryDocument,
			IUnitOfWork unitOfWork,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService codesProcessingService,
			IGenericRepository<GroupGtin> groupGtinrepository,
			IGenericRepository<Gtin> gtinRepository,
			ITrueMarkCodesValidator trueMarkValidator,
			IBus messageBus,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			IGenericRepository<TrueMarkProductCode> trueMarkProductCodesRepository)
			: base(navigationManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_selfDeliveryDocument = selfDeliveryDocument ?? throw new ArgumentNullException(nameof(selfDeliveryDocument));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_codesProcessingService = codesProcessingService ?? throw new ArgumentNullException(nameof(codesProcessingService));
			_groupGtinrepository = groupGtinrepository ?? throw new ArgumentNullException(nameof(groupGtinrepository));
			_gtinRepository = gtinRepository ?? throw new ArgumentNullException(nameof(gtinRepository));
			_trueMarkValidator = trueMarkValidator ?? throw new ArgumentNullException(nameof(trueMarkValidator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_trueMarkProductCodesRepository = trueMarkProductCodesRepository ?? throw new ArgumentNullException(nameof(trueMarkProductCodesRepository));

			WindowPosition = WindowGravity.None;

			Initialize();
		}

		public List<CodeScanRow> CodeScanRows { get; } = new List<CodeScanRow>();

		public List<CodesScanProgressRow> CodesScanProgressRows { get; } = new List<CodesScanProgressRow>();

		public DelegateCommand CloseCommand { get; private set; }
		public DelegateCommand DeleteCodeCommand { get; private set; }

		public IRecursiveConfig RecursiveConfig { get; set; }

		public Action RefreshScanningNomenclaturesAction { get; set; }

		public string CurrentCodeInProcess { get; private set; }

		public bool IsAllCodesScanned =>
			!_scannedStagingCodes.Any(x => x.Key.Amount > x.Value.Where(c => c.IsIdentification).Count());

		public CodeScanRow SelectedRow
		{
			get => _selectedRow;
			set => SetField(ref _selectedRow, value);
		}

		private void Initialize()
		{
			CloseCommand = new DelegateCommand(CloseScanning, () => IsAllCodesScanned);
			CloseCommand.CanExecuteChangedWith(this, vm => vm.IsAllCodesScanned);

			DeleteCodeCommand = new DelegateCommand(() => DeleteCode());

			_organizationInn = _selfDeliveryDocument.Order.Contract.Organization.INN;

			_allGtins = _gtinRepository.GetValue(
					_unitOfWork,
					x =>
						new GtinFromNomenclatureDto
						{
							GtinNumber = x.GtinNumber,
							NomenclatureName = x.Nomenclature.Name,
						})
				.ToList();

			_allGroupGtins = _groupGtinrepository.GetValue(
					_unitOfWork,
					x =>
						new GtinFromNomenclatureDto
						{
							GtinNumber = x.GtinNumber,
							NomenclatureName = x.Nomenclature.Name,
							CodesCount = x.CodesCount
						})
				.ToList();

			var gtinsInOrder = _selfDeliveryDocument.Items
				.SelectMany(i => i.Nomenclature.Gtins)
				.Select(g => g.GtinNumber)
				.ToList();

			var groupGtinsInOrder = _selfDeliveryDocument.Items
				.SelectMany(i => i.Nomenclature.GroupGtins)
				.Select(g => g.GtinNumber)
				.ToList();

			_gtinsInOrder = gtinsInOrder.Union(groupGtinsInOrder).ToList();

			RecursiveConfig = new RecursiveConfig<CodeScanRow>(x => x.Parent, x => x.Children);

			FillAlreadyScannedNomenclatures();

			CheckAllCodeScanned();

			StartUpdater();
		}

		private void DeleteCode()
		{
			lock(CodeScanRows)
			{
				var rawCode = SelectedRow?.RawCode;

				var parentRawCode = SelectedRow?.Parent;

				if(rawCode is null || CurrentCodeInProcess != null)
				{
					return;
				}

				var stagingCodeToDelete = GetAddedStagingCodeByRawCode(rawCode);

				var unitCodes = SelectedRow.Children.Any()
					? SelectedRow.Children.Select(x => x.RawCode).ToList()
					: new List<string> { SelectedRow.RawCode };

				_guiDispatcher.RunInGuiTread(() =>
				{
					if(!_interactiveService.Question($"Действительно хотите удалить код {rawCode}?"))
					{
						return;
					}

					if(parentRawCode != null || stagingCodeToDelete?.ParentCodeId != null)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя удалять дочерний код из агрегатного.");

						return;
					}

					var hasOrderEdoRequest = _unitOfWork
						.GetAll<OrderEdoRequest>()
						.Any(x => x.Order.Id == _selfDeliveryDocument.Order.Id);

					if(hasOrderEdoRequest)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error, $"По данному заказу уже создана заявка");

						return;
					}

					if(stagingCodeToDelete != null)
					{
						_unitOfWork.Delete(stagingCodeToDelete);
					}

					RemoveCodeFromScanRows(rawCode);

					RefreshCodeScanRows();
				});
			}
		}

		private void RemoveCodeFromScanRows(string rawCode)
		{
			var scanRowToRemove = GetCodeScanRowByRawCode(rawCode);
			RemoveCodeScanRow(scanRowToRemove);
		}

		private CodeScanRow GetCodeScanRowByRawCode(string rawCode)
		{
			var scanRowToRemove = CodeScanRows.FirstOrDefault(x => x.RawCode == rawCode);
			return scanRowToRemove;
		}

		private void RemoveCodeScanRows(IEnumerable<CodeScanRow> codeScanRows)
		{
			foreach(var codeScanRow in codeScanRows)
			{
				CodeScanRows.Remove(codeScanRow);
			}
		}

		private void RemoveCodeScanRow(CodeScanRow codeScanRow)
		{
			if(!CodeScanRows.Contains(codeScanRow))
			{
				throw new InvalidOperationException("Попытка удалить строку, которой нет в списке");
			}

			CodeScanRows.Remove(codeScanRow);
		}

		private StagingTrueMarkCode GetAddedStagingCodeByRawCode(string rawCode)
		{
			ParseRawCode(rawCode, out var code);

			var stagingCode = _scannedStagingCodes
				.SelectMany(x => x.Value)
				.FirstOrDefault(x => x.RawCode == rawCode);

			return stagingCode;
		}

		private void CheckAllCodeScanned()
		{
			if(IsAllCodesScanned)
			{
				_guiDispatcher.RunInGuiTread(() =>
					{
						_interactiveService.ShowMessage(ImportanceLevel.Info,
							"Сейчас нечего сканировать. Проверьте количество номенклатур на форме самовывоза");
					}
				);
			}
		}

		private async Task UpdateScannedStagingCodes()
		{
			if(_scannedStagingCodesUpdateInProgress)
			{
				return;
			}

			_scannedStagingCodesUpdateInProgress = true;

			_scannedStagingCodes = await GetSelfDeliveryDocumentItemStagingTrueMarkCodes();

			await Task.Delay(1000);

			_scannedStagingCodesUpdateInProgress = false;
		}

		private async Task<IDictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>>> GetSelfDeliveryDocumentItemStagingTrueMarkCodes()
		{
			var result = new Dictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>>();

			var allScannedCodes =
				await _codesProcessingService.GetStagingTrueMarkCodesBySelfDeliveryDocumentItem(_unitOfWork, 0);

			foreach(var item in _selfDeliveryDocument.Items)
			{
				var itemGtins = item.Nomenclature.Gtins.Select(x => x.GtinNumber).ToList();
				var itemCodes = new List<StagingTrueMarkCode>();

				foreach(var code in allScannedCodes)
				{
					var codeGtin = code.AllIdentificationCodes.First().Gtin;
					if(itemGtins.Contains(codeGtin))
					{
						itemCodes.Add(code);
					}
				}

				result.Add(item, itemCodes);
			}

			return result;
		}

		private void StartUpdater()
		{
			_cancelationTokenSource = new CancellationTokenSource();

			Task.Run(async () => await DoUpdate(_cancelationTokenSource.Token), _cancelationTokenSource.Token);
		}

		private async Task DoUpdate(CancellationToken cancellationToken)
		{
			var scanningInProcess = true;

			while(scanningInProcess && !cancellationToken.IsCancellationRequested)
			{
				scanningInProcess = !IsAllCodesScanned;

				await CheckNewCodes(cancellationToken);

				await RecheckOldCodes(cancellationToken);

				_guiDispatcher.RunInGuiTread(CheckAndSetKeyBoardSettings);

				Thread.Sleep(100);
			}
		}

		private void CheckAndSetKeyBoardSettings()
		{
			if(!WinKeyboardWorkHelper.IsWindowsOs)
			{
				return;
			}

			if(!WinKeyboardWorkHelper.IsEnKeyboardLayot())
			{
				WinKeyboardWorkHelper.SwitchToEnglish();
			}

			if(WinKeyboardWorkHelper.IsCapsLockEnabled)
			{
				WinKeyboardWorkHelper.TurnOffCapsLock();
			}
		}

		private async Task RecheckOldCodes(CancellationToken cancellationToken)
		{
			var needCheckNewCode = false;

			while(!needCheckNewCode && _oldCodesToRechek.TryTake(out var code) && !cancellationToken.IsCancellationRequested)
			{
				var toTime = DateTime.UtcNow + TimeSpan.FromMinutes(1);

				while(DateTime.UtcNow < toTime && !cancellationToken.IsCancellationRequested)
				{
					if(_newCodesToCheck.Any())
					{
						needCheckNewCode = true;
						break;
					}

					Thread.Sleep(100);
				}

				lock(CodeScanRows)
				{
					if(CodeScanRows.All(x => !x.RawCode.Contains(code.RawCode) && !code.RawCode.Contains(x.RawCode)))
					{
						break;
					}
				}

				_logger.LogInformation("Запускам обработку кода c ошибкой {ToRecheckCode} из Updater-а", code.RawCode);

				await HandleCheckCodeAsync(code, cancellationToken);

				CurrentCodeInProcess = null;

				_logger.LogInformation("Завершена обработка кода с ошибкой {ToRecheckCode} из Updater-а", code.RawCode);
			}
		}

		private async Task CheckNewCodes(CancellationToken cancellationToken)
		{
			while(_newCodesToCheck.TryTake(out var code) && !cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("Запускам обработку нового кода {ToRecheckCode} из Updater-а", code.RawCode);

				await HandleCheckCodeAsync(code, cancellationToken);

				CurrentCodeInProcess = null;

				_logger.LogInformation("Завершена обработка нового кода {ToRecheckCode} из Updater-а", code.RawCode);

				Thread.Sleep(100);
			}
		}

		private void CloseScanning()
		{
			Close(false, CloseSource.ClosePage);
		}

		private void FillAlreadyScannedNomenclatures()
		{
			UpdateScannedStagingCodes().GetAwaiter().GetResult();

			foreach(var selfDeliveryDocumentItem in _selfDeliveryDocument.Items)
			{
				var nomenclature = selfDeliveryDocumentItem.Nomenclature.Name;

				var codes = _scannedStagingCodes.TryGetValue(selfDeliveryDocumentItem, out var itemCodes)
					? itemCodes
					: Enumerable.Empty<StagingTrueMarkCode>();

				foreach(var code in codes)
				{
					var codesScanViewModelNode = new CodeScanRow()
					{
						RowNumber = CodeScanRows.Count + 1,
						RawCode = code.RawCode,
						NomenclatureName = nomenclature,
						IsTrueMarkValid = true,
						HasInOrder = true,
						AdditionalInformation = code.IsIdentification ? string.Empty : "Агрегатный код"
					};

					CodeScanRows.Add(codesScanViewModelNode);
				}

				CodesScanProgressRows.Add(
					new CodesScanProgressRow
					{
						NomenclatureName = nomenclature,
						InSelfDelivery = (int)selfDeliveryDocumentItem.Amount,
						LeftToScan = (int)selfDeliveryDocumentItem.Amount - codes.Where(x => x.IsIdentification).Count(),
						Gtin = _allGtins.FirstOrDefault(x => x.NomenclatureName == nomenclature)?.GtinNumber
					});
			}
		}

		private async Task HandleCheckCodeAsync(CodeToCheck codeToCheck, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Обработка кода {RawCode}", codeToCheck.RawCode);

			var code = ParseRawCode(codeToCheck.RawCode, out var parsedCode);
			var gtin = parsedCode?.Gtin;

			CurrentCodeInProcess = code;

			UpdateCodeScanRows(code, gtin,
				additionalInformation: codeToCheck.NeedRecheck
					? new List<string> { "Повторная обработка кода после ошибки: {CheckError}", codeToCheck.Error }
					: new List<string> { $"Обработка кода" });

			try
			{
				Result<TrueMarkAnyCode> result;

				cancellationToken.ThrowIfCancellationRequested();

				_logger.LogInformation(
					"Отправляем запрос на обработку кода {RawCode} в {TrueMarkWaterCodeParser)}",
					codeToCheck.RawCode, nameof(_trueMarkWaterCodeParser));

				var additionalInformation = new List<string>();

				var createStagingTrueMarkCodeResult = await _codesProcessingService
					.CreateStagingTrueMarkCode(_unitOfWork, code, 0, cancellationToken);

				if(createStagingTrueMarkCodeResult.IsFailure)
				{
					if(createStagingTrueMarkCodeResult.Errors.Any(x => x == Errors.TrueMark.TrueMarkCodeErrors.StagingTrueMarkCodeDuplicate))
					{
						additionalInformation.Add("Повторное сканирование");
						UpdateCodeScanRows(code, gtin, additionalInformation: additionalInformation);
						return;
					}

					if(createStagingTrueMarkCodeResult.Errors.Any(x => x == Application.Errors.TrueMarkApi.ErrorResponse))
					{
						additionalInformation.AddRange(createStagingTrueMarkCodeResult.Errors.Select(x => x.Message));

						UpdateCodeScanRows(code, gtin, false, additionalInformation: additionalInformation);

						_logger.LogInformation(
							"Обработки кода {RawCode} в {TrueMarkWaterCodeParser} завершилась с ошибками: {Errors}",
							codeToCheck.RawCode, nameof(_trueMarkWaterCodeParser), string.Join(", ", additionalInformation));

						AddOldCodeToRecheck(codeToCheck.RawCode, string.Join(", ", additionalInformation));

						return;
					}

					var errorMessages = createStagingTrueMarkCodeResult.Errors.Select(x => x.Message).ToList();
					UpdateCodeScanRows(code, gtin, isDuplicate: true, additionalInformation: errorMessages);
					return;
				}

				var stagingCode = createStagingTrueMarkCodeResult.Value;

				//Если у созданного кода уже есть Id, значит либо этот код, либо его родительский код уже обрабатывались
				if(stagingCode.Id != 0)
				{
					return;
				}

				var isCodeCanBeAddedResult = await IsTrueMarkStagingCodeCanBeAdded(
					stagingCode,
					_selfDeliveryDocument.Items.First().Nomenclature.Id,
					cancellationToken);

				if(isCodeCanBeAddedResult.IsFailure)
				{
					var errorMessages = isCodeCanBeAddedResult.Errors.Select(x => x.Message).ToList();
					UpdateCodeScanRows(code, gtin, isDuplicate: true, additionalInformation: errorMessages);
					return;
				}

				var existingCodeScanRowsToRemove = GetExistingScanRowsToRemove(stagingCode);

				_unitOfWork.Save(stagingCode);

				RemoveCodeScanRows(existingCodeScanRowsToRemove);

				await UpdateCodeScanRowsByAnyCode(stagingCode, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					"Возникло исключение при обработке кода {RawCode} в {TrueMarkWaterCodeParser} : {Exception}",
					codeToCheck.RawCode, nameof(_trueMarkWaterCodeParser), ex);

				var additionalInformation = new List<string> { ex.Message };
				UpdateCodeScanRows(code, gtin, additionalInformation: additionalInformation);

				AddOldCodeToRecheck(codeToCheck.RawCode, ex.Message);
			}
		}

		private async Task<Result> IsTrueMarkStagingCodeCanBeAdded(
			StagingTrueMarkCode stagingCode,
			int nomenclatureId,
			CancellationToken cancellationToken)
		{
			var isCodeCanBeAddedToItemOfNomenclature = await _codesProcessingService.IsStagingTrueMarkCodeCanBeAddedToItemOfNomenclature(
				_unitOfWork,
				stagingCode,
				nomenclatureId,
				cancellationToken);

			if(isCodeCanBeAddedToItemOfNomenclature.IsFailure)
			{
				return isCodeCanBeAddedToItemOfNomenclature;
			}

			var isCodeAlreadyUsedInProductCodes = await _codesProcessingService.IsStagingTrueMarkCodeAlreadyUsedInProductCodes(
				_unitOfWork,
				stagingCode,
				cancellationToken);

			if(isCodeAlreadyUsedInProductCodes.IsFailure)
			{
				return isCodeAlreadyUsedInProductCodes;
			}

			return Result.Success();
		}

		private IEnumerable<CodeScanRow> GetExistingScanRowsToRemove(StagingTrueMarkCode stagingTrueMarkCode)
		{
			var rowsToRemove = new List<CodeScanRow>();

			var codesToRemove = stagingTrueMarkCode.AllCodes
				.Where(x => x.Id != 0);

			if(codesToRemove is null || !codesToRemove.Any())
			{
				return rowsToRemove;
			}

			foreach(var code in codesToRemove)
			{
				var row = GetCodeScanRowByRawCode(code.RawCode);

				if(row is null)
				{
					continue;
				}

				rowsToRemove.Add(row);
			}

			return rowsToRemove;
		}

		private void AddNewCodeToCheck(string rawCode, string error = null)
		{
			_newCodesToCheck.TryAdd(new CodeToCheck
			{
				RawCode = rawCode,
				NeedRecheck = false,
				Error = error
			});
		}

		private void AddOldCodeToRecheck(string rawCode, string error = null)
		{
			_oldCodesToRechek.TryAdd(new CodeToCheck
			{
				RawCode = rawCode,
				NeedRecheck = true,
				Error = error
			});
		}

		private string ParseRawCode(string rawCode, out TrueMarkWaterCode result)
		{
			_trueMarkWaterCodeParser.TryParse(rawCode, out var parsedCode);

			if(parsedCode == null)
			{
				parsedCode = _trueMarkWaterCodeParser.ParseCodeFromSelfDelivery(rawCode);
			}

			result = parsedCode;

			return parsedCode?.SourceCode ?? ReplaceCodeSpecSymbols(rawCode);
		}

		private void RefreshCodeScanRows()
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				lock(CodeScanRows)
				{
					CodeScanRows.Sort((a, b) => b.RowNumber.CompareTo(a.RowNumber));

					RefreshScanningNomenclaturesAction?.Invoke();

					UpdateScannedStagingCodes().GetAwaiter().GetResult();

					OnPropertyChanged(() => IsAllCodesScanned);
				}
			});
		}

		private bool? IsOrderContainsGtin(string gtin)
		{
			if(gtin is null)
			{
				return null;
			}

			return _gtinsInOrder.Contains(gtin);
		}

		private async Task AddCodeToSelfDeliveryDocumentItemAsync(
			SelfDeliveryDocumentItem selfDeliveryDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			CancellationToken cancellationToken)
		{
			await _unitOfWork.SaveAsync(trueMarkWaterIdentificationCode, cancellationToken: cancellationToken);
			
			var productCode = new SelfDeliveryDocumentItemTrueMarkProductCode
			{
				CreationTime = DateTime.Now,
				SourceCode = trueMarkWaterIdentificationCode,
				ResultCode = trueMarkWaterIdentificationCode,
				Problem = ProductCodeProblem.None,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				SelfDeliveryDocumentItem = selfDeliveryDocumentItem
			};

			selfDeliveryDocumentItem.TrueMarkProductCodes.Add(productCode);
		}

		private string ReplaceCodeSpecSymbols(string code) => code.Replace("", "\\u001d");

		private string GetNomenclatureNameByGtin(string gtin) =>
			_allGtins.FirstOrDefault(x => x.GtinNumber == gtin)?.NomenclatureName
			?? _allGroupGtins.FirstOrDefault(x => x.GtinNumber == gtin)?.NomenclatureName;

		private void UpdateCodeScanRows(string code, string gtin, bool? isValid = null, List<string> additionalInformation = null, bool isDuplicate = false)
		{
			if(additionalInformation is null)
			{
				additionalInformation = new List<string>();
			}

			var hasInOrder = IsOrderContainsGtin(gtin);
			var nomenclatureName = GetNomenclatureNameByGtin(gtin);

			lock(CodeScanRows)
			{
				var existsCodeScanRow = CodeScanRows.FirstOrDefault(x => x.RawCode.Contains(code) || code.Contains(x.RawCode));

				if(existsCodeScanRow is null)
				{
					existsCodeScanRow = CodeScanRows
						.SelectMany(x => x.Children)
						.FirstOrDefault(x => x.RawCode.Contains(code) || code.Contains(x.RawCode));
				}

				if(existsCodeScanRow is null)
				{
					var codeScanRow = new CodeScanRow
					{
						RowNumber = CodeScanRows.Count + 1,
						RawCode = code,
						IsTrueMarkValid = isValid,
						HasInOrder = hasInOrder,
						NomenclatureName = nomenclatureName,
						AdditionalInformation = string.Join(", ", additionalInformation),
						IsDuplicate = isDuplicate
					};

					CodeScanRows.Add(codeScanRow);
				}
				else
				{
					existsCodeScanRow.NomenclatureName = nomenclatureName;
					existsCodeScanRow.IsTrueMarkValid = isValid;
					existsCodeScanRow.HasInOrder = hasInOrder;
					existsCodeScanRow.AdditionalInformation = string.Join(", ", additionalInformation);
					existsCodeScanRow.IsDuplicate = isDuplicate;
				}
			}

			RefreshCodeScanRows();
		}

		private async Task UpdateCodeScanRowsByAnyCode(StagingTrueMarkCode stagingCode, CancellationToken cancellationToken)
		{
			var codeNomenclatureName =
				stagingCode.IsTransport
				? "Транспортный код"
				: stagingCode.IsGroup && string.IsNullOrWhiteSpace(GetNomenclatureNameByGtin(stagingCode.Gtin))
					? "Групповой код"
					: GetNomenclatureNameByGtin(stagingCode.Gtin);

			(List<StagingTrueMarkCode> childrenCodesList, string gtin, string rawCode,
						string identificationCode, string nomenclatureName, bool? hasInOrder) codeData =
						(stagingCode.IsIdentification ? null : stagingCode.AllIdentificationCodes.ToList(),
						stagingCode.Gtin,
						ReplaceCodeSpecSymbols(stagingCode.RawCode),
						stagingCode.IsTransport ? ReplaceCodeSpecSymbols(stagingCode.RawCode) : stagingCode.IdentificationCode,
						codeNomenclatureName,
						stagingCode.IsTransport ? null : IsOrderContainsGtin(stagingCode.Gtin));

			(var childrenUnitCodeList, var gtin, var rawCode, string identificationCode, var nomenclatureName, bool? hasInOrder) = codeData;

			var validationErrors = await GetValidationErrorsFromTrueMarkAsync(rawCode, identificationCode, gtin, cancellationToken);
			var isValid = validationErrors is null;

			var codesToDistribute = new List<StagingTrueMarkCode>();

			lock(CodeScanRows)
			{
				if(childrenUnitCodeList is null)
				{
					var childrenCodeScanRows = CodeScanRows.SelectMany(x => x.Children);

					var existsUnitCodeFromAggregateCode = childrenCodeScanRows.FirstOrDefault(x =>
						x.RawCode.Contains(rawCode)
						|| rawCode.Contains(x.RawCode));

					if(existsUnitCodeFromAggregateCode != null)
					{
						UpdateCodeScanRows(rawCode, gtin, false,
							new List<string>
							{
								$"Данный индивидуальный код уже содержится в отсканированном агрегатном коде: {existsUnitCodeFromAggregateCode.Parent?.RawCode}"
							});

						return;
					}

					if(isValid)
					{
						codesToDistribute.Add(stagingCode);
					}
				}
				else
				{
					var existsAggregateCodeForUnitCode =
						CodeScanRows.FirstOrDefault(sr => childrenUnitCodeList.Any(uc =>
							uc.RawCode.Contains(sr.RawCode)
							|| sr.RawCode.Contains(uc.RawCode)));

					if(existsAggregateCodeForUnitCode != null)
					{
						UpdateCodeScanRows(rawCode, gtin, false,
							new List<string>
							{
								$"Дочерний код данного агрегатного кода уже есть в самовывозе: {existsAggregateCodeForUnitCode.RawCode}"
							});

						return;
					}
					
					var rootNode = CodeScanRows.FirstOrDefault(x => x.RawCode.Contains(rawCode) || rawCode.Contains(x.RawCode));

					rootNode.Children.Clear();

					var rowNumber = 0;
					foreach(var trueMarkWaterIdentificationCode in childrenUnitCodeList)
					{
						hasInOrder = IsOrderContainsGtin(trueMarkWaterIdentificationCode.Gtin);

						var childNode = CodeScanRows.FirstOrDefault(x =>
							                x.RawCode.Contains(trueMarkWaterIdentificationCode.RawCode) || trueMarkWaterIdentificationCode.RawCode.Contains(x.RawCode))
						                ?? new CodeScanRow { RowNumber = CodeScanRows.Count + 1 };

						childNode.RawCode = trueMarkWaterIdentificationCode.RawCode;
						childNode.NomenclatureName = GetNomenclatureNameByGtin(trueMarkWaterIdentificationCode.Gtin);
						childNode.Parent = rootNode;
						childNode.HasInOrder = hasInOrder;
						childNode.IsTrueMarkValid = isValid;
						childNode.RowNumber = ++rowNumber;

						rootNode.Children.Add(childNode);

						if(isValid)
						{
							codesToDistribute.Add(trueMarkWaterIdentificationCode);
						}
						else
						{
							childNode.AdditionalInformation = string.Join(", ", validationErrors);
						}
					}
				}
			}

			if(!stagingCode.IsIdentification)
			{
				UpdateCodeScanRows(rawCode, gtin, additionalInformation: new List<string> { "Агрегатный код" });
			}

			await _unitOfWork.SaveAsync(stagingCode, cancellationToken: cancellationToken);

			await DistributeCodeOnNextSelfDeliveryItemAsync(codesToDistribute, cancellationToken);

			RefreshCodeScanRows();
		}

		private async Task<List<string>> GetValidationErrorsFromTrueMarkAsync(string rawCode, string identificationCode, string gtin,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Отправляем запрос на валидацию кода {Code} в {TrueMarkValidator)}", identificationCode, nameof(_trueMarkValidator));

			var trueMarkValidationResults =
				(await _trueMarkValidator.ValidateAsync(new List<string> { identificationCode }, _organizationInn, cancellationToken))
				.CodeResults
				.ToList();

			_logger.LogInformation(
				"Получили ответ по валидации кодов {Code} в {TrueMarkValidator)}", identificationCode, nameof(_trueMarkValidator));

			var errorMessages = new List<string>();

			if(!trueMarkValidationResults.Any())
			{
				errorMessages.Add("Не получен результат валидации в ЧЗ");

				UpdateCodeScanRows(rawCode, gtin, false, errorMessages);

				return errorMessages;
			}

			errorMessages.Clear();

			var validationResult = trueMarkValidationResults.FirstOrDefault(x => x.CodeString == identificationCode);

			if(validationResult is null)
			{
				errorMessages.Add("Не получен результат валидации в ЧЗ для кода");

				UpdateCodeScanRows(rawCode, gtin, false, errorMessages);

				AddOldCodeToRecheck(rawCode, string.Join(", ", errorMessages));

				return errorMessages;
			}

			if(!validationResult.IsIntroduced)
			{
				errorMessages.Add("Код не в обороте");
			}

			var isOurGtin = gtin == null || validationResult.IsOurGtin;

			if(!isOurGtin)
			{
				errorMessages.Add("Это не наш код");
			}

			if(!validationResult.IsOwnedByOurOrganization)
			{
				errorMessages.Add("Не мы являемся владельцем товара");
			}

			if(!validationResult.IsIntroduced || !isOurGtin || !validationResult.IsOwnedByOurOrganization)
			{
				UpdateCodeScanRows(rawCode, gtin, false, errorMessages);

				return errorMessages;
			}

			UpdateCodeScanRows(rawCode, gtin, true, errorMessages);

			return null;
		}

		private async Task DistributeCodeOnNextSelfDeliveryItemAsync(List<StagingTrueMarkCode> codes,
			CancellationToken cancellationToken)
		{
			foreach(var code in codes)
			{
				await DistributeCodeOnNextSelfDeliveryItemAsync(code, cancellationToken);
			}
		}

		private async Task DistributeCodeOnNextSelfDeliveryItemAsync(StagingTrueMarkCode code,
			CancellationToken cancellationToken)
		{
			SelfDeliveryDocumentItem nextSelfDeliveryItemToDistributeByGtin;

			nextSelfDeliveryItemToDistributeByGtin = GetNextNotScannedDocumentItem(code);

			if(nextSelfDeliveryItemToDistributeByGtin == null)
			{
				return;
			}

			//await AddCodeToSelfDeliveryDocumentItemAsync(nextSelfDeliveryItemToDistributeByGtin, code, cancellationToken);

			var nomenclatureName = nextSelfDeliveryItemToDistributeByGtin.Nomenclature.Name;

			CodesScanProgressRows.First(x => x.NomenclatureName == nomenclatureName && x.LeftToScan > 0).LeftToScan--;
		}

		private SelfDeliveryDocumentItem GetNextNotScannedDocumentItem(StagingTrueMarkCode code)
		{
			var documentItem = _selfDeliveryDocument.Items?
				.Where(x => x.Nomenclature.IsAccountableInTrueMark)
				.FirstOrDefault(s =>
					s.Nomenclature.Gtins.Select(g => g.GtinNumber).Contains(code.Gtin)
					&& s.TrueMarkProductCodes.Count < s.Amount
					&& s.TrueMarkProductCodes.All(c =>
						!c.SourceCode.RawCode.Contains(code.RawCode) && !code.RawCode.Contains(c.SourceCode.RawCode)));

			return documentItem;
		}

		public void CheckCode(string rawCode)
		{
			if(rawCode.Length < 20)
			{
				return;
			}

			var code = ParseRawCode(rawCode, out var parsedCode);
			var gtin = parsedCode?.Gtin;

			CodeScanRow alreadyScannedNode;

			lock(CodeScanRows)
			{
				alreadyScannedNode = CodeScanRows.FirstOrDefault(x => x.RawCode.Contains(code) || code.Contains(x.RawCode));

				//Поднятие вновь отсканированного кода наверх
				if(alreadyScannedNode != null)
				{
					for(var i = alreadyScannedNode.RowNumber + 1; i <= CodeScanRows.Count; i++)
					{
						CodeScanRows.First(x => x.RowNumber == i).RowNumber--;
					}

					alreadyScannedNode.RowNumber = CodeScanRows.Count;
				}
			}

			if(alreadyScannedNode != null)
			{
				RefreshCodeScanRows();

				return;
			}

			var inProcessMessage = new List<string> { "В очереди на обработку" };

			if(parsedCode != null)
			{
				_logger.LogInformation("Код {RawCode} успешно распарсен", rawCode);

				gtin = parsedCode.Gtin;

				var isGtinInOrder = _gtinsInOrder.Contains(gtin);

				var groupGtin = _allGroupGtins.FirstOrDefault(x => x.GtinNumber == gtin);

				if(groupGtin == null)
				{
					if(!isGtinInOrder)
					{
						var additionalInformation = new List<string>
							{ $"Номенклатура {GetNomenclatureNameByGtin(gtin)} с данным Gtin {gtin} отсутствует в заказе." };

						UpdateCodeScanRows(code, gtin, additionalInformation: additionalInformation);

						return;
					}

					UpdateCodeScanRows(code, gtin, additionalInformation: inProcessMessage);
				}
				else
				{
					UpdateCodeScanRows(code, gtin,
						additionalInformation: new List<string>
							{ $"В очереди на обработку: наша группа {groupGtin.CodesCount} шт.: {groupGtin.NomenclatureName}" });
				}
			}
			else
			{
				_logger.LogInformation("Код {RawCode} не удалось распарсить.", rawCode);

				UpdateCodeScanRows(code, null, additionalInformation: inProcessMessage);
			}

			AddNewCodeToCheck(rawCode);
		}

		public async Task<Result> AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes()
		{
			foreach(var item in _selfDeliveryDocument.Items)
			{
				var itemStagingCodes = _scannedStagingCodes.TryGetValue(item, out var itemCodes)
					? itemCodes
					: Enumerable.Empty<StagingTrueMarkCode>();

				if(!itemStagingCodes.Any())
				{
					continue;
				}

				var addingCodesResult = await _codesProcessingService.AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(
					_unitOfWork,
					item,
					itemStagingCodes);

				if(addingCodesResult.IsFailure)
				{
					return addingCodesResult;
				}
			}

			return Result.Success();
		}

		public Result IsAllTrueMarkProductCodesAdded()
		{
			foreach(var item in _selfDeliveryDocument.Items)
			{
				var checkResult =
					_codesProcessingService.IsAllSelfDeliveryDocumentItemTrueMarkProductCodesAdded(item);

				if(checkResult.IsFailure)
				{
					return checkResult;
				}
			}

			return Result.Success();
		}

		public OrderEdoRequest CreateEdoRequest(IUnitOfWork unitOfWork, Order order)
		{
			var codes = _selfDeliveryDocument.Items
				.SelectMany(x => x.TrueMarkProductCodes)
				.ToList();

			if(!codes.Any())
			{
				return null;
			}

			var edoRequest = new OrderEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Selfdelivery,
				Order = order
			};

			foreach(var code in codes)
			{
				edoRequest.ProductCodes.Add(code);
			}

			unitOfWork.Save(edoRequest);

			return edoRequest;
		}

		public async Task SendEdoRequestCreatedEvent(OrderEdoRequest orderEdoRequest)
		{
			await _messageBus.Publish(new EdoRequestCreatedEvent { Id = orderEdoRequest.Id });
		}

		public string GetCodesForClipboardCopy() =>
			string.Join(", ", CodeScanRows.OrderBy(x => x.RowNumber).Select(x => $"\"{x.RawCode}\""));

		public void Dispose()
		{
			_cancelationTokenSource?.Cancel();
		}
	}
}
