using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Gamma.Binding.Core.RecursiveTreeConfig;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;
using Vodovoz.Models.TrueMark;
using VodovozBusiness.Domain.Goods;
using VodovozBusiness.Services.TrueMark;

namespace Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan
{
	public partial class CodesScanViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly SelfDeliveryDocument _selfDeliveryDocument;
		private readonly IUnitOfWork _unitOfWork;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly IGenericRepository<GroupGtin> _groupGtinrepository;
		private readonly IGenericRepository<Gtin> _gtinRepository;
		private readonly IBus _messageBus;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly ILogger<CodesScanViewModel> _logger;
		private string _organizationInn;
		private List<GtinFromNomenclatureDto> _allGtins;
		private List<GtinFromNomenclatureDto> _allGroupGtins;
		private List<string> _gtinsInOrder;
		private HashSet<CancellationTokenSource> _cancellationTokenSources = new HashSet<CancellationTokenSource>();
		private List<string> _codesToRecheck = new List<string>();

		public CodesScanViewModel(
			INavigationManager navigationManager,
			SelfDeliveryDocument selfDeliveryDocument,
			IUnitOfWork unitOfWork,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			IGenericRepository<GroupGtin> groupGtinrepository,
			IGenericRepository<Gtin> gtinRepository,
			IBus messageBus,
			IGuiDispatcher guiDispatcher,
			ILogger<CodesScanViewModel> logger
		)
			: base(navigationManager)
		{
			_selfDeliveryDocument = selfDeliveryDocument ?? throw new ArgumentNullException(nameof(selfDeliveryDocument));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_groupGtinrepository = groupGtinrepository ?? throw new ArgumentNullException(nameof(groupGtinrepository));
			_gtinRepository = gtinRepository ?? throw new ArgumentNullException(nameof(gtinRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			WindowPosition = WindowGravity.None;

			Initialize();
		}

		public List<CodeScanRow> CodeScanRows { get; } = new List<CodeScanRow>();

		public List<CodesScanProgressRow> CodesScanProgressRows { get; } = new List<CodesScanProgressRow>();

		public DelegateCommand CloseCommand { get; set; }

		public IRecursiveConfig RecursiveConfig { get; set; }

		public Action RefreshScanningNomenclaturesAction { get; set; }

		public bool IsAllCodesScanned
		{
			get
			{
				lock(_selfDeliveryDocument)
				{
					return _selfDeliveryDocument.Items?
						.Where(x => x.Nomenclature.IsAccountableInTrueMark)
						.All(x => x.Amount == x.TrueMarkProductCodes.Count) ?? false;
				}
			}
		}

		private void Initialize()
		{
			CloseCommand = new DelegateCommand(CloseScanning, () => IsAllCodesScanned);
			CloseCommand.CanExecuteChangedWith(this, vm => vm.IsAllCodesScanned);

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

			lock(CodeScanRows)
			{
				FillAlreadyScannedNomenclatures();
			}

			StartUpdater();
		}

		private void StartUpdater()
		{
			var cancelationTokenSource = new CancellationTokenSource();

			lock(_cancellationTokenSources)
			{
				_cancellationTokenSources.Add(cancelationTokenSource);
			}

			Task.Run(
				async () =>
				{
					LogInformationWithThreadId($"Запускам поток слежения за процессом сканирования");

					Update(cancelationTokenSource.Token);

					LogInformationWithThreadId($"Завершен поток слежения за процессом сканирования");
				}
			);
		}

		private void Update(CancellationToken cancellationToken)
		{
			bool scanningInProcess = true;

			while(scanningInProcess && !cancellationToken.IsCancellationRequested)
			{
				scanningInProcess = !IsAllCodesScanned;

				var toCheck = new List<string>();

				lock(_codesToRecheck)
				{
					toCheck = _codesToRecheck.ToList();
				}

				foreach(var recheckCode in toCheck)
				{
					LogInformationWithThreadId($"Запускам повторную обработку кода {recheckCode} из Updater-а");

					HandleCheckCode(recheckCode, cancellationToken, true);

					LogInformationWithThreadId($"Завершена повторная обработка кода {recheckCode} из Updater-а");


					lock(_codesToRecheck)
					{
						_codesToRecheck.Remove(recheckCode);
					}
				}

				Thread.Sleep(5000);
			}
		}

		private void LogInformationWithThreadId(string message, LogLevel logLevel = LogLevel.Information)
		{
			var resultMessage = $"Id потока: {Task.CurrentId} : {message}";

			_logger.Log(logLevel, resultMessage);
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

				foreach(var code in selfDeliveryDocumentItem.TrueMarkProductCodes)
				{
					var codesScanViewModelNode = new CodeScanRow()
					{
						RowNumber = CodeScanRows.Count + 1,
						CodeNumber = code.SourceCode.RawCode,
						NomenclatureName = nomenclature,
						IsTrueMarkValid = true,
						HasInOrder = true
					};

					CodeScanRows.Add(codesScanViewModelNode);
				}

				CodesScanProgressRows.Add(
					new CodesScanProgressRow
					{
						NomenclatureName = nomenclature,
						InSelfDelivery = (int)selfDeliveryDocumentItem.Amount,
						LeftToScan = (int)selfDeliveryDocumentItem.Amount - selfDeliveryDocumentItem.TrueMarkProductCodes.Count,
						Gtin = _allGtins.FirstOrDefault(x => x.NomenclatureName == nomenclature)?.GtinNumber
					});
			}
		}

		private void HandleCheckCode(string rawCode, CancellationToken cancellationToken, bool forceCheck = false)
		{
			LogInformationWithThreadId($"Обработка кода {rawCode}");

			var code = ParseRawCode(rawCode, out var parsedCode);

			if(!forceCheck)
			{
				lock(CodeScanRows)
				{
					var alreadyScannedNode = CodeScanRows.FirstOrDefault(x => x?.CodeNumber == code);

					//Поднятие вновь отсканированного кода наверх
					if(alreadyScannedNode != null)
					{
						for(var i = alreadyScannedNode.RowNumber + 1; i <= CodeScanRows.Count; i++)
						{
							CodeScanRows.First(x => x.RowNumber == i).RowNumber--;
						}

						alreadyScannedNode.RowNumber = CodeScanRows.Count;

						RefreshCodeScanRows();

						return;
					}
				}
			}

			string gtin = null;

			var inProcessMessage = new List<string> { "В обработке" };

			if(parsedCode != null)
			{
				LogInformationWithThreadId($"Код {rawCode} успешно распарсен");

				gtin = parsedCode.GTIN;

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
							{ $"В обработке: наша группа {groupGtin.CodesCount} шт.: {groupGtin.NomenclatureName}" });
				}
			}
			else
			{
				LogInformationWithThreadId($"Код {rawCode} не удалось распарсить.");

				UpdateCodeScanRows(code, gtin, additionalInformation: inProcessMessage);
			}

			try
			{
				Result<TrueMarkAnyCode> result;

				lock(_unitOfWork)
				{
					cancellationToken.ThrowIfCancellationRequested();

					LogInformationWithThreadId($"Отправляем запрос на обработку кода {rawCode} в {nameof(_trueMarkWaterCodeParser)}");

					result = _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_unitOfWork, code, _organizationInn, cancellationToken)
						.GetAwaiter()
						.GetResult();

					LogInformationWithThreadId($"Получили результат обработки кода {rawCode} в {nameof(_trueMarkWaterCodeParser)}");
				}

				var additionalInformation = new List<string>();

				if(result.Errors.Any(x => !string.IsNullOrEmpty(x.Message)))
				{
					additionalInformation.AddRange(result.Errors.Select(x => x.Message));

					UpdateCodeScanRows(code, gtin, false, additionalInformation: additionalInformation);

					LogInformationWithThreadId(
						$"Обработки кода {rawCode} в {nameof(_trueMarkWaterCodeParser)} завершилась с ошибками: {string.Join(", ", additionalInformation)}");

					AddCodeToRecheck(rawCode);

					return;
				}

				UpdateCodeScanRowsByAnyCode(result.Value);
			}
			catch(Exception ex)
			{
				LogInformationWithThreadId($"Возникло исключение при обработке кода {rawCode} в {nameof(_trueMarkWaterCodeParser)} : {ex}",
					LogLevel.Error);

				var additionalInformation = new List<string> { ex.ToString() };
				UpdateCodeScanRows(code, gtin, additionalInformation: additionalInformation);

				AddCodeToRecheck(rawCode);
			}
		}

		private void AddCodeToRecheck(string rawCode)
		{
			lock(_codesToRecheck)
			{
				_codesToRecheck.Add(rawCode);
			}
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

		private void AddCodeToSelfDeliveryDocumentItem(
			SelfDeliveryDocumentItem selfDeliveryDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode)
		{
			lock(_unitOfWork)
			{
				var productCode = new SelfDeliveryDocumentItemTrueMarkProductCode
				{
					CreationTime = DateTime.Now,
					SourceCode = trueMarkWaterIdentificationCode,
					Problem = ProductCodeProblem.None,
					SourceCodeStatus = SourceProductCodeStatus.Accepted,
					SelfDeliveryDocumentItem = selfDeliveryDocumentItem
				};

				_unitOfWork.Save(productCode);

				selfDeliveryDocumentItem.TrueMarkProductCodes.Add(productCode);
			}
		}

		private string ReplaceCodeSpecSymbols(string code) => code.Replace("", "\\u001d");

		private string GetNomenclatureNameByGtin(string gtin) =>
			_allGtins.FirstOrDefault(x => x.GtinNumber == gtin)?.NomenclatureName
			?? _allGroupGtins.FirstOrDefault(x => x.GtinNumber == gtin)?.NomenclatureName;

		private void UpdateCodeScanRows(string code, string gtin, bool? isValid = null, List<string> additionalInformation = null)
		{
			if(additionalInformation is null)
			{
				additionalInformation = new List<string>();
			}

			var hasInOrder = IsOrderContainsGtin(gtin);
			var nomenclatureName = GetNomenclatureNameByGtin(gtin);

			lock(CodeScanRows)
			{
				var existsCodeScanRow = CodeScanRows.FirstOrDefault(x => x.CodeNumber == code);

				if(existsCodeScanRow is null)
				{
					existsCodeScanRow = CodeScanRows
						.SelectMany(x => x.Children)
						.FirstOrDefault(x => x.CodeNumber == code);
				}

				if(existsCodeScanRow is null)
				{
					var codeScanRow = new CodeScanRow
					{
						RowNumber = CodeScanRows.Count + 1,
						CodeNumber = code,
						IsTrueMarkValid = isValid,
						HasInOrder = hasInOrder,
						NomenclatureName = nomenclatureName,
						AdditionalInformation = string.Join(", ", additionalInformation),
					};

					CodeScanRows.Add(codeScanRow);
				}
				else
				{
					existsCodeScanRow.NomenclatureName = nomenclatureName;
					existsCodeScanRow.IsTrueMarkValid = isValid;
					existsCodeScanRow.HasInOrder = hasInOrder;
					existsCodeScanRow.AdditionalInformation = string.Join(", ", additionalInformation);
				}

				RefreshCodeScanRows();
			}
		}

		private void UpdateCodeScanRowsByAnyCode(TrueMarkAnyCode anyCode)
		{
			var codeInfo =
				anyCode
					.Match<(List<TrueMarkWaterIdentificationCode> childrenCodesList, string rawCode, string nomenclatureName, bool?
						hasInOrder)>(
						transportCode =>
						{
							return
								(transportCode.GetAllCodes()
										.Where(x => x.IsTrueMarkWaterIdentificationCode)
										.Select(x => x.TrueMarkWaterIdentificationCode)
										.ToList(),
									ReplaceCodeSpecSymbols(transportCode.RawCode),
									"Транспортный код",
									null);
						},
						groupCode =>
						{
							var groupNomenclatureName = GetNomenclatureNameByGtin(groupCode.GTIN);

							return
								(groupCode.GetAllCodes()
										.Where(x => x.IsTrueMarkWaterIdentificationCode)
										.Select(x => x.TrueMarkWaterIdentificationCode)
										.ToList(),
									ReplaceCodeSpecSymbols(groupCode.RawCode),
									string.IsNullOrWhiteSpace(groupNomenclatureName) ? "Групповой код" : groupNomenclatureName,
									null);
						},
						waterCode => (
							null,
							ReplaceCodeSpecSymbols(waterCode.FullCode),
							GetNomenclatureNameByGtin(waterCode.GTIN),
							IsOrderContainsGtin(waterCode.GTIN)));

			(var childrenCodesList, var rawCode, var nomenclatureName, bool? hasInOrder) = codeInfo;

			lock(CodeScanRows)
			{
				var rootNode = CodeScanRows.FirstOrDefault(x => x.CodeNumber == rawCode);

				if(rootNode is null)
				{
					rootNode = new CodeScanRow
					{
						RowNumber = CodeScanRows.Count + 1,
						CodeNumber = rawCode,
						NomenclatureName = nomenclatureName,
						HasInOrder = hasInOrder
					};

					CodeScanRows.Add(rootNode);
				}
				else
				{
					var rootAdditionalInformations = new List<string>();
					bool? rootIsTrueMarkValid = null;
					string rootGtin = null;

					if(anyCode.IsTrueMarkWaterIdentificationCode)
					{
						rootAdditionalInformations =
							GetAdditionalInformation(anyCode.TrueMarkWaterIdentificationCode.TrueMarkCodeValidationResult);
						rootIsTrueMarkValid =
							IsValidByTrueMarkCodeValidationResult(anyCode.TrueMarkWaterIdentificationCode.TrueMarkCodeValidationResult);
						rootGtin = anyCode.TrueMarkWaterIdentificationCode.GTIN;
					}

					rootNode.CodeNumber = rawCode;
					rootNode.NomenclatureName = nomenclatureName;
					rootNode.HasInOrder = hasInOrder;
					rootNode.AdditionalInformation = string.Join(", ", rootAdditionalInformations);
					rootNode.IsTrueMarkValid = rootIsTrueMarkValid;

					UpdateCodeScanRows(rawCode, rootGtin, rootIsTrueMarkValid, rootAdditionalInformations);
				}

				if(childrenCodesList != null)
				{
					rootNode.Children.Clear();

					foreach(var trueMarkWaterIdentificationCode in childrenCodesList)
					{
						hasInOrder = IsOrderContainsGtin(trueMarkWaterIdentificationCode.GTIN);

						var childNode = CodeScanRows.FirstOrDefault(x =>
							                x.CodeNumber == trueMarkWaterIdentificationCode.RawCode)
						                ?? new CodeScanRow { RowNumber = CodeScanRows.Count + 1 };

						var additionalInformations = GetAdditionalInformation(trueMarkWaterIdentificationCode.TrueMarkCodeValidationResult);

						childNode.CodeNumber = trueMarkWaterIdentificationCode.RawCode;
						childNode.NomenclatureName = GetNomenclatureNameByGtin(trueMarkWaterIdentificationCode.GTIN);
						childNode.Parent = rootNode;
						childNode.HasInOrder = hasInOrder;
						childNode.IsTrueMarkValid =
							IsValidByTrueMarkCodeValidationResult(trueMarkWaterIdentificationCode.TrueMarkCodeValidationResult);
						childNode.AdditionalInformation = string.Join(", ", additionalInformations);

						if(childNode.IsTrueMarkValid ?? false)
						{
							DistributeCodeOnNextSelfDeliveryItem(trueMarkWaterIdentificationCode);
						}

						rootNode.Children.Add(childNode);

						UpdateCodeScanRows(trueMarkWaterIdentificationCode.RawCode, trueMarkWaterIdentificationCode.GTIN,
							childNode.IsTrueMarkValid, new List<string> { childNode.AdditionalInformation });
					}
				}
				else
				{
					if(rootNode.IsTrueMarkValid ?? false)
					{
						DistributeCodeOnNextSelfDeliveryItem(anyCode.TrueMarkWaterIdentificationCode);
					}
				}

				RefreshCodeScanRows();
			}
		}

		private void DistributeCodeOnNextSelfDeliveryItem(TrueMarkWaterIdentificationCode code)
		{
			lock(_selfDeliveryDocument)
			{
				var nextSelfDeliveryItemToDistributeByGtin = GetNextNotScannedDocumentItem(code);

				if(nextSelfDeliveryItemToDistributeByGtin == null)
				{
					return;
				}

				AddCodeToSelfDeliveryDocumentItem(nextSelfDeliveryItemToDistributeByGtin, code);

				var nomenclatureName = nextSelfDeliveryItemToDistributeByGtin.Nomenclature.Name;

				CodesScanProgressRows.First(x => x.NomenclatureName == nomenclatureName && x.LeftToScan > 0).LeftToScan--;
			}
		}

		private List<string> GetAdditionalInformation(TrueMarkCodeValidationResult trueMarkCodeValidationResult)
		{
			var additionalInformation = new List<string>();

			if(trueMarkCodeValidationResult == null)
			{
				additionalInformation.Add("Не получен результат валидации в ЧЗ");

				return additionalInformation;
			}

			if(!trueMarkCodeValidationResult.IsIntroduced)
			{
				additionalInformation.Add("Код не в обороте");
			}

			if(!trueMarkCodeValidationResult.IsOurGtin)
			{
				additionalInformation.Add("Это не наш код");
			}

			if(!trueMarkCodeValidationResult.IsOwnedByOurOrganization)
			{
				additionalInformation.Add("Не мы являемся владельцем товара");
			}

			return additionalInformation;
		}

		private bool? IsValidByTrueMarkCodeValidationResult(TrueMarkCodeValidationResult trueMarkCodeValidationResult)
		{
			if(trueMarkCodeValidationResult is null)
			{
				return null;
			}

			return trueMarkCodeValidationResult.IsIntroduced
			       && trueMarkCodeValidationResult.IsOurGtin
			       && trueMarkCodeValidationResult.IsOwnedByOurOrganization;
		}

		private SelfDeliveryDocumentItem GetNextNotScannedDocumentItem(TrueMarkWaterIdentificationCode code)
		{
			var documentItem = _selfDeliveryDocument.Items?
				.Where(x => x.Nomenclature.IsAccountableInTrueMark)
				.FirstOrDefault(s =>
					s.Nomenclature.Gtins.Select(g => g.GtinNumber).Contains(code.GTIN)
					&& s.TrueMarkProductCodes.Count < s.Amount
					&& s.TrueMarkProductCodes.All(c => c.SourceCode.Id != code.Id));

			return documentItem;
		}

		public void CheckCode(string code)
		{
			var cancellationTokenSource = new CancellationTokenSource();

			lock(_cancellationTokenSources)
			{
				_cancellationTokenSources.Add(cancellationTokenSource);
			}

			Task.Run(
					async () =>
					{
						LogInformationWithThreadId($"Запускам поток обработки кода {code} со сканера");

						HandleCheckCode(code, cancellationTokenSource.Token);

						LogInformationWithThreadId($"Завершён поток обработки кода {code} со сканера");
					}
				);
		}

		private void OnTaskComplete(Task task, string code)
		{
			LogInformationWithThreadId($"Поток с id {task.Id} для обработки кода {code} со сканера завершён.");
		}

		private void OnUpdaterComplete(Task task)
		{
			LogInformationWithThreadId($"Поток с id {task.Id} для слежения за сканированием завершён");
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

		public void Dispose()
		{
			lock(_cancellationTokenSources)
			{
				foreach(var cancellationTokenSource in _cancellationTokenSources)
				{
					cancellationTokenSource?.Cancel();
				}
			}
		}
	}
}
