using Edo.Contracts.Messages.Events;
using Gamma.Binding.Core.RecursiveTreeConfig;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.TrueMark;
using Vodovoz.Models.TrueMark;
using VodovozBusiness.Domain.Client.Specifications;
using VodovozBusiness.Domain.Goods;
using VodovozBusiness.Services.TrueMark;
using VodovozInfrastructure.Keyboard;

namespace Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan
{
	public partial class CodesScanViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService _codesProcessingService;
		private readonly IGenericRepository<GroupGtin> _groupGtinrepository;
		private readonly IGenericRepository<Gtin> _gtinRepository;
		private readonly IBus _messageBus;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly ILogger<CodesScanViewModel> _logger;
		private List<GtinFromNomenclatureDto> _allGtins;
		private List<GtinFromNomenclatureDto> _allGroupGtins;
		private List<string> _gtinsInOrder;
		private readonly BlockingCollection<CodeToCheck> _newCodesToCheck = new BlockingCollection<CodeToCheck>();
		private readonly BlockingCollection<CodeToCheck> _oldCodesToRechek = new BlockingCollection<CodeToCheck>();
		private CancellationTokenSource _cancelationTokenSource;
		private CodeScanRow _selectedRow;

		private IUnitOfWork _unitOfWork;
		private SelfDeliveryDocument _selfDeliveryDocument;
		private IList<StagingTrueMarkCode> _allScannedStagingCodes;

		public CodesScanViewModel(
			ILogger<CodesScanViewModel> logger,
			INavigationManager navigationManager,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService codesProcessingService,
			IGenericRepository<GroupGtin> groupGtinrepository,
			IGenericRepository<Gtin> gtinRepository,
			IBus messageBus,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService)
			: base(navigationManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_codesProcessingService = codesProcessingService ?? throw new ArgumentNullException(nameof(codesProcessingService));
			_groupGtinrepository = groupGtinrepository ?? throw new ArgumentNullException(nameof(groupGtinrepository));
			_gtinRepository = gtinRepository ?? throw new ArgumentNullException(nameof(gtinRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			WindowPosition = WindowGravity.None;

			CloseCommand = new DelegateCommand(CloseScanning, () => IsAllCodesScanned);
			CloseCommand.CanExecuteChangedWith(this, vm => vm.IsAllCodesScanned);

			DeleteCodeCommand = new DelegateCommand(DeleteCode);
		}

		public List<CodeScanRow> CodeScanRows { get; } = new List<CodeScanRow>();

		public IObservableList<CodesScanProgressRow> CodesScanProgressRows { get; } = new ObservableList<CodesScanProgressRow>();

		public DelegateCommand CloseCommand { get; private set; }
		public DelegateCommand DeleteCodeCommand { get; private set; }

		public IRecursiveConfig RecursiveConfig { get; set; }

		public Action RefreshScanningNomenclaturesAction { get; set; }

		public string CurrentCodeInProcess { get; private set; }

		public bool IsAllCodesScanned =>
			!_documentItemsScannedStagingCodes.Any(x => x.Key.Amount > x.Value.SelectMany(c => c.AllIdentificationCodes).Count());

		public CodeScanRow SelectedRow
		{
			get => _selectedRow;
			set => SetField(ref _selectedRow, value);
		}

		private IDictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>> _documentItemsScannedStagingCodes =>
			GetSelfDeliveryDocumentItemStagingTrueMarkCodes();

		public void Initialize(IUnitOfWork uow, SelfDeliveryDocument selfDeliveryDocument, IList<StagingTrueMarkCode> allScannedStagingCodes)
		{
			_unitOfWork = uow ?? throw new ArgumentNullException(nameof(uow));
			_selfDeliveryDocument = selfDeliveryDocument ?? throw new ArgumentNullException(nameof(selfDeliveryDocument));
			_allScannedStagingCodes = allScannedStagingCodes ?? new List<StagingTrueMarkCode>();

			RecursiveConfig = new RecursiveConfig<CodeScanRow>(x => x.Parent, x => x.Children);

			UpdateGtinsData();
			FillAlreadyScannedNomenclatures();
			CheckAllCodeScanned();
			StartUpdater();
		}

		private void UpdateGtinsData()
		{
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

					var hasEdoRequest = _unitOfWork
						.GetAll<FormalEdoRequest>()
						.Any(x => x.Order.Id == _selfDeliveryDocument.Order.Id);

					if(hasEdoRequest)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error, $"По данному заказу уже создана заявка");

						return;
					}

					if(stagingCodeToDelete != null
						&& _allScannedStagingCodes.Contains(stagingCodeToDelete))
					{
						_allScannedStagingCodes.Remove(stagingCodeToDelete);
					}

					RemoveCodeFromScanRows(rawCode);

					RefreshCodeScanRows();
					UpdateCodesScanProgressRows();
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

			var stagingCode = _documentItemsScannedStagingCodes
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

		private IDictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>> GetSelfDeliveryDocumentItemStagingTrueMarkCodes()
		{
			var result = new Dictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>>();

			if(_selfDeliveryDocument is null)
			{
				return result;
			}

			foreach(var item in _selfDeliveryDocument.Items)
			{
				var itemGtins = item.Nomenclature.Gtins.Select(x => x.GtinNumber).ToList();
				var itemCodes = new List<StagingTrueMarkCode>();

				foreach(var code in _allScannedStagingCodes)
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
			foreach(var selfDeliveryDocumentItem in _selfDeliveryDocument.Items)
			{
				var nomenclature = selfDeliveryDocumentItem.Nomenclature.Name;

				var codes = _documentItemsScannedStagingCodes.TryGetValue(selfDeliveryDocumentItem, out var itemCodes)
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

				var alreadyScannedItemCodesCount = codes.SelectMany(x => x.AllIdentificationCodes).Count();

				CodesScanProgressRows.Add(
					new CodesScanProgressRow
					{
						NomenclatureName = nomenclature,
						InSelfDelivery = (int)selfDeliveryDocumentItem.Amount,
						LeftToScan = (int)selfDeliveryDocumentItem.Amount - alreadyScannedItemCodesCount,
						Gtin = _allGtins.FirstOrDefault(x => x.NomenclatureName == nomenclature)?.GtinNumber
					});
			}
			UpdateCodeScanRowsByStagingCodes(_allScannedStagingCodes);
		}

		private async Task HandleCheckCodeAsync(CodeToCheck codeToCheck, CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Обработка кода {RawCode}. Заказ {OrderId}",
				codeToCheck.RawCode,
				_selfDeliveryDocument.Order.Id);

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

						_logger.LogInformation(
							"Обработки кода {RawCode} в {TrueMarkWaterCodeParser} завершилась с ошибками: {Errors}",
							codeToCheck.RawCode, nameof(_trueMarkWaterCodeParser), string.Join(", ", additionalInformation));

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

				var isCodeCanBeAddedResult = await IsTrueMarkStagingCodeCanBeAdded(
					stagingCode,
					_selfDeliveryDocument.Items.First().Nomenclature.Id,
					cancellationToken);

				if(isCodeCanBeAddedResult.IsFailure)
				{
					var errorMessages = isCodeCanBeAddedResult.Errors.Select(x => x.Message).ToList();
					UpdateCodeScanRows(code, gtin, isDuplicate: true, additionalInformation: errorMessages);

					_logger.LogInformation(
						"Обработки кода {RawCode} в {TrueMarkWaterCodeParser} завершилась с ошибками: {Errors}",
						codeToCheck.RawCode, nameof(_trueMarkWaterCodeParser), string.Join(", ", additionalInformation));

					return;
				}

				var addingCodeResult = AddStagingTrueMarkCode(stagingCode);

				if(addingCodeResult.IsFailure)
				{
					if(addingCodeResult.Errors.Any(x => x == Errors.TrueMark.TrueMarkCodeErrors.StagingTrueMarkCodeDuplicate))
					{
						additionalInformation.Add("Повторное сканирование");
						UpdateCodeScanRows(code, gtin, true, additionalInformation: additionalInformation);

						_logger.LogInformation(
							"Обработки кода {RawCode} в {TrueMarkWaterCodeParser} завершилась с ошибками: {Errors}",
							codeToCheck.RawCode, nameof(_trueMarkWaterCodeParser), string.Join(", ", additionalInformation));

						return;
					}

					if(addingCodeResult.Errors.Any(x => x == Application.Errors.TrueMarkApi.ErrorResponse))
					{
						additionalInformation.AddRange(addingCodeResult.Errors.Select(x => x.Message));
						UpdateCodeScanRows(code, gtin, false, additionalInformation: additionalInformation);

						_logger.LogInformation(
							"Обработки кода {RawCode} в {TrueMarkWaterCodeParser} завершилась с ошибками: {Errors}",
							codeToCheck.RawCode, nameof(_trueMarkWaterCodeParser), string.Join(", ", additionalInformation));

						return;
					}
				}

				var existingCodeScanRowsToRemove = GetExistingScanRowsToRemove(addingCodeResult.Value);

				lock(CodeScanRows)
				{
					RemoveCodeScanRows(existingCodeScanRowsToRemove);
				}

				UpdateCodeScanRowsByStagingCode(stagingCode);
				UpdateCodesScanProgressRows();
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

		private Result<IEnumerable<StagingTrueMarkCode>> AddStagingTrueMarkCode(StagingTrueMarkCode stagingTrueMarkCode)
		{
			var alreadyAddedRootCode =
				GetStagingTrueMarkCodesAddedDuplicates(new[] { stagingTrueMarkCode });

			if(alreadyAddedRootCode.Any())
			{
				return Result.Failure<IEnumerable<StagingTrueMarkCode>>(TrueMarkCodeErrors.StagingTrueMarkCodeDuplicate);
			}

			var alreadyAddedInnerCodes =
				GetStagingTrueMarkCodesAddedDuplicates(stagingTrueMarkCode.AllCodes);

			lock(_allScannedStagingCodes)
			{
				foreach(var code in alreadyAddedInnerCodes)
				{
					if(!_allScannedStagingCodes.Contains(code))
					{
						continue;
					}

					_allScannedStagingCodes.Remove(code);
				}

				_allScannedStagingCodes.Add(stagingTrueMarkCode);
			}

			return Result.Success(alreadyAddedInnerCodes);
		}

		private IEnumerable<StagingTrueMarkCode> GetStagingTrueMarkCodesAddedDuplicates(
			IEnumerable<StagingTrueMarkCode> newCodes)
		{
			var existingCodes = new List<StagingTrueMarkCode>();
			var allScannedCodes = _allScannedStagingCodes.SelectMany(x => x.AllCodes).ToList();

			foreach(var code in newCodes)
			{
				var existingCodesPredicate = StagingTrueMarkCodeSpecification.CreateForEqualStagingCodes(code).Expression.Compile();

				existingCodes.AddRange(allScannedCodes
					.Where(existingCodesPredicate)
					.ToList());
			}

			return existingCodes;
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

		private IEnumerable<CodeScanRow> GetExistingScanRowsToRemove(IEnumerable<StagingTrueMarkCode> stagingTrueMarkCodes)
		{
			var rowsToRemove = new List<CodeScanRow>();

			foreach(var stagingTrueMarkCode in stagingTrueMarkCodes)
			{
				var row = GetCodeScanRowByRawCode(stagingTrueMarkCode.RawCode);

				if(row is null)
				{
					continue;
				}

				if(rowsToRemove.Contains(row))
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
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
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

		private void UpdateCodeScanRowsByStagingCodes(IEnumerable<StagingTrueMarkCode> stagingCodes)
		{
			foreach(var staingCode in stagingCodes)
			{
				UpdateCodeScanRowsByStagingCode(staingCode);
			}
		}

		private void UpdateCodeScanRowsByStagingCode(StagingTrueMarkCode stagingCode)
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

					UpdateCodeScanRows(rawCode, gtin, true, new List<string>());
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
						childNode.IsTrueMarkValid = true;
						childNode.RowNumber = ++rowNumber;

						rootNode.Children.Add(childNode);
					}

					UpdateCodeScanRows(rawCode, gtin, additionalInformation: new List<string> { "Агрегатный код" });
				}
			}

			RefreshCodeScanRows();
		}

		private void UpdateCodesScanProgressRows()
		{
			var documentItemsCodes = _documentItemsScannedStagingCodes;

			lock(CodesScanProgressRows)
			{
				foreach(var documentItemCodes in documentItemsCodes)
				{
					var nomenclatureName = documentItemCodes.Key.Nomenclature.Name;

					var documentItemIdentificationCodesCount =
						documentItemCodes.Value
						.SelectMany(x => x.AllIdentificationCodes)
						.Count();

					var codesLeftToScan = (int)(documentItemCodes.Key.Amount - documentItemIdentificationCodesCount);

					CodesScanProgressRows.First(x => x.NomenclatureName == nomenclatureName).LeftToScan
						= codesLeftToScan;
				}
			}
			OnPropertyChanged(() => IsAllCodesScanned);
		}

		private async Task DistributeCodeOnNextSelfDeliveryItemAsync(List<TrueMarkWaterIdentificationCode> codes,
			CancellationToken cancellationToken)
		{
			foreach(var code in codes)
			{
				await DistributeCodeOnNextSelfDeliveryItemAsync(code, cancellationToken);
			}
		}

		private async Task DistributeCodeOnNextSelfDeliveryItemAsync(TrueMarkWaterIdentificationCode code,
			CancellationToken cancellationToken)
		{
			SelfDeliveryDocumentItemEntity nextSelfDeliveryItemToDistributeByGtin;

			nextSelfDeliveryItemToDistributeByGtin = GetNextNotScannedDocumentItem(code);

			if(nextSelfDeliveryItemToDistributeByGtin == null)
			{
				return;
			}

			await AddCodeToSelfDeliveryDocumentItemAsync(nextSelfDeliveryItemToDistributeByGtin, code, cancellationToken);

			var nomenclatureName = nextSelfDeliveryItemToDistributeByGtin.Nomenclature.Name;

			CodesScanProgressRows.First(x => x.NomenclatureName == nomenclatureName && x.LeftToScan > 0).LeftToScan--;
		}

		private SelfDeliveryDocumentItemEntity GetNextNotScannedDocumentItem(TrueMarkWaterIdentificationCode code)
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

		public async Task<Result> AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(bool isCheckAllCodesScanned = false)
		{
			if(isCheckAllCodesScanned && !IsAllCodesScanned)
			{
				return Result.Failure(TrueMarkCodeErrors.NotAllCodesAdded);
			}

			foreach(var item in _selfDeliveryDocument.Items)
			{
				var itemStagingCodes = _documentItemsScannedStagingCodes.TryGetValue(item, out var itemCodes)
					? itemCodes
					: Enumerable.Empty<StagingTrueMarkCode>();

				if(!itemStagingCodes.Any())
				{
					continue;
				}

				var addingCodesResult = await _codesProcessingService.AddProductCodesToSelfDeliveryDocumentItem(
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

		public PrimaryEdoRequest CreateEdoRequest(IUnitOfWork unitOfWork, Order order)
		{
			var codes = _selfDeliveryDocument.Items
				.SelectMany(x => x.TrueMarkProductCodes)
				.ToList();

			if(!codes.Any())
			{
				return null;
			}

			var edoRequest = new PrimaryEdoRequest
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

		public async Task SendEdoRequestCreatedEvent(PrimaryEdoRequest orderEdoRequest)
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
