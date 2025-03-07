using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Edo.Contracts.Messages.Events;
using Gamma.Binding.Core.RecursiveTreeConfig;
using MassTransit;
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
		private readonly ITrueMarkCodesValidator _trueMarkValidator;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly IGenericRepository<GroupGtin> _groupGtinrepository;
		private readonly IGenericRepository<Gtin> _gtinRepository;
		private readonly IBus _messageBus;
		private readonly IGuiDispatcher _guiDispatcher;
		private string _organizationInn;
		private List<GtinFromNomenclatureDto> _allGtins;
		private List<GtinFromNomenclatureDto> _allGroupGtins;
		private List<string> _nomenclatureGtinsInOrder;
		private HashSet<CancellationTokenSource> _cancellationTokenSource = new HashSet<CancellationTokenSource>();

		public CodesScanViewModel(
			INavigationManager navigationManager,
			SelfDeliveryDocument selfDeliveryDocument,
			IUnitOfWork unitOfWork,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			ITrueMarkCodesValidator trueMarkValidator,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			IGenericRepository<GroupGtin> groupGtinrepository,
			IGenericRepository<Gtin> gtinRepository,
			IBus messageBus,
			IGuiDispatcher guiDispatcher)
			: base(navigationManager)
		{
			_selfDeliveryDocument = selfDeliveryDocument ?? throw new ArgumentNullException(nameof(selfDeliveryDocument));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_trueMarkValidator = trueMarkValidator ?? throw new ArgumentNullException(nameof(trueMarkValidator));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_groupGtinrepository = groupGtinrepository ?? throw new ArgumentNullException(nameof(groupGtinrepository));
			_gtinRepository = gtinRepository ?? throw new ArgumentNullException(nameof(gtinRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			WindowPosition = WindowGravity.None;

			Initialize();
		}

		public List<CodeScanRow> CodeScanRows { get; set; } = new List<CodeScanRow>();

		public List<CodesScanProgressRow> CodesScanProgressRows { get; set; } = new List<CodesScanProgressRow>();

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

			_nomenclatureGtinsInOrder = gtinsInOrder.Union(groupGtinsInOrder).ToList();

			RecursiveConfig = new RecursiveConfig<CodeScanRow>(x => x.Parent, x => x.Children);

			FillAlreadyScannedNomenclatures();
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

		private void ProcessCheckCodes(string rawCode, CancellationToken cancellationToken)
		{
			var code = ReplaceCodeSpecSymbols(rawCode);

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

			var isParsed = _trueMarkWaterCodeParser.TryParse(code, out var parseCode);

			string gtin = null;

			if(isParsed)
			{
				gtin = parseCode.GTIN;

				var gtinHasInOrder = _nomenclatureGtinsInOrder.Contains(gtin);

				var groupGtin = _allGroupGtins.FirstOrDefault(x => x.GtinNumber == gtin);

				if(groupGtin == null)
				{
					if(!gtinHasInOrder)
					{
						var additionalInformation = new List<string>
							{ $"Номенклатура {GetNomenclatureNameByGtin(gtin)} с данным Gtin {gtin} отсутствует в заказе" };

						UpdateCodeScanRows(code, gtin, additionalInformation: additionalInformation);

						return;
					}

					UpdateCodeScanRows(code, gtin);
				}
				else
				{
					UpdateCodeScanRows(code, gtin,
						additionalInformation: new List<string>
							{ $"Наша группа {groupGtin.CodesCount} шт.: {groupGtin.NomenclatureName}" });
				}
			}
			else
			{
				UpdateCodeScanRows(code, "В обработке");
			}

			try
			{
				Result<TrueMarkAnyCode> result = null;

				lock(_unitOfWork)
				{
					result = _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(_unitOfWork, code, cancellationToken)
						.GetAwaiter()
						.GetResult();
				}

				var additionalInformation = new List<string>();

				if(result.Errors.Any(x => !string.IsNullOrEmpty(x.Message)))
				{
					additionalInformation.AddRange(result.Errors.Select(x => x.Message));

					UpdateCodeScanRows(code, gtin, false, additionalInformation: additionalInformation);

					return;
				}

				List<TrueMarkWaterIdentificationCode> unitCodes;

				unitCodes = GetAllIdentificationCodes(result.Value);

				UpdateCodeScanRowsByAnyCode(result.Value);

				ValidateInTrueMark(unitCodes, cancellationToken);
			}
			finally
			{
				lock(CodeScanRows)
				{
					RefreshCodeScanRows();
				}
			}
		}

		private void RefreshCodeScanRows()
		{
			lock(CodeScanRows)
			{
				CodeScanRows.Sort((a, b) => b.RowNumber.CompareTo(a.RowNumber));
			}

			_guiDispatcher.RunInGuiTread(() =>
				RefreshScanningNomenclaturesAction?.Invoke()
			);
		}

		private bool? GetGtinHasInOrder(string gtin)
		{
			if(gtin is null)
			{
				return null;
			}

			return _nomenclatureGtinsInOrder.Contains(gtin);
		}

		private List<TrueMarkWaterIdentificationCode> GetAllIdentificationCodes(TrueMarkAnyCode anyCode) =>
			anyCode.Match(
				transportCode => transportCode.GetAllCodes()
					.Where(x => x.IsTrueMarkWaterIdentificationCode)
					.Select(x => x.TrueMarkWaterIdentificationCode)
					.ToList(),
				groupCode => groupCode.GetAllCodes()
					.Where(x => x.IsTrueMarkWaterIdentificationCode)
					.Select(x => x.TrueMarkWaterIdentificationCode)
					.ToList(),
				waterCode => new List<TrueMarkWaterIdentificationCode> { waterCode }
			);
		
		private void ValidateInTrueMark(List<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken)
		{
			var trueMarkValidationResults = _trueMarkValidator
				.ValidateAsync(codes, _organizationInn, cancellationToken)
				.GetAwaiter()
				.GetResult()
				.CodeResults
				.ToList();

			var additionalInformation = new List<string>();

			if(!trueMarkValidationResults.Any())
			{
				additionalInformation.Add("Не получен результат валидации в ЧЗ");

				UpdateCodeScanRows(string.Join(", ", codes.Select(x => x.RawCode)), null, false, additionalInformation);

				return;
			}

			foreach(var code in codes)
			{
				additionalInformation.Clear();

				var validationResult =
					trueMarkValidationResults.FirstOrDefault(x => (x.Code as TrueMarkWaterIdentificationCode)?.RawCode == code.RawCode);

				if(validationResult is null)
				{
					additionalInformation.Add("Не получен результат валидации в ЧЗ для кода");

					UpdateCodeScanRows(code.RawCode, code.GTIN, false, additionalInformation);

					continue;
				}

				if(!validationResult.IsIntroduced)
				{
					additionalInformation.Add("Код не в обороте");
				}

				if(!validationResult.IsOurGtin)
				{
					additionalInformation.Add("Это не наш код");
				}

				if(!validationResult.IsOwnedByOurOrganization)
				{
					additionalInformation.Add("Не мы являемся владельцем товара");
				}

				if(!validationResult.IsIntroduced || !validationResult.IsOurGtin || !validationResult.IsOwnedByOurOrganization)
				{
					UpdateCodeScanRows(code.RawCode, code.GTIN, false, additionalInformation);

					continue;
				}

				UpdateCodeScanRows(code.RawCode, code.GTIN, true, additionalInformation);

				var nextSelfDeliveryItemToDistributeByGtin = GetNextNotScannedItem(code.GTIN);
				if(nextSelfDeliveryItemToDistributeByGtin != null)
				{
					AddCodeToSelfDeliveryDocumentItem(nextSelfDeliveryItemToDistributeByGtin, code);

					lock(CodesScanProgressRows)
					{
						CodesScanProgressRows.First(x => x.Gtin == code.GTIN && x.LeftToScan > 0).LeftToScan--;
					}
				}
			}
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

			var hasInOrder = GetGtinHasInOrder(gtin);
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
						AdditionalInformation = string.Join(", ", additionalInformation)
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
			List<TrueMarkAnyCode> childrenCodesList = null;
			string rawCode = null;
			string nomenclatureName = null;
			bool? hasInOrder = null;

			if(anyCode.IsTrueMarkTransportCode)
			{
				childrenCodesList = anyCode.TrueMarkTransportCode.GetAllCodes().Where(x => x.IsTrueMarkWaterIdentificationCode).ToList();
				rawCode = anyCode.TrueMarkTransportCode.RawCode;
				nomenclatureName = "Транспортный код";
			}

			if(anyCode.IsTrueMarkWaterGroupCode)
			{
				childrenCodesList = anyCode.TrueMarkWaterGroupCode.GetAllCodes().Where(x => x.IsTrueMarkWaterIdentificationCode).ToList();
				rawCode = anyCode.TrueMarkWaterGroupCode.RawCode;
				var groupNomenclatureName = GetNomenclatureNameByGtin(anyCode.TrueMarkWaterGroupCode.GTIN);

				nomenclatureName = string.IsNullOrWhiteSpace(groupNomenclatureName) ? "Групповой код" : groupNomenclatureName;
			}

			if(anyCode.IsTrueMarkWaterIdentificationCode)
			{
				rawCode = anyCode.TrueMarkWaterIdentificationCode.RawCode;
				nomenclatureName = GetNomenclatureNameByGtin(anyCode.TrueMarkWaterIdentificationCode.GTIN);
				hasInOrder = GetGtinHasInOrder(anyCode.TrueMarkWaterIdentificationCode.GTIN);
			}

			lock(CodeScanRows)
			{
				var parentNode = CodeScanRows.FirstOrDefault(x => ReplaceCodeSpecSymbols(x.CodeNumber) == rawCode);

				if(parentNode is null)
				{
					parentNode = new CodeScanRow
					{
						RowNumber = CodeScanRows.Count + 1,
						CodeNumber = rawCode,
						NomenclatureName = nomenclatureName,
						HasInOrder = hasInOrder
					};

					CodeScanRows.Add(parentNode);
				}
				else
				{
					parentNode.CodeNumber = rawCode;
					parentNode.NomenclatureName = nomenclatureName;
					parentNode.HasInOrder = hasInOrder;
				}

				if(childrenCodesList != null)
				{
					parentNode.Children.Clear();

					foreach(var childrenCode in childrenCodesList)
					{
						hasInOrder = GetGtinHasInOrder(childrenCode.TrueMarkWaterIdentificationCode.GTIN);

						var childNode = CodeScanRows.FirstOrDefault(x => 
							x.CodeNumber == childrenCode.TrueMarkWaterIdentificationCode.RawCode) 
						    ?? new CodeScanRow { RowNumber = CodeScanRows.Count + 1 };

						childNode.CodeNumber = childrenCode.TrueMarkWaterIdentificationCode.RawCode;
						childNode.NomenclatureName = GetNomenclatureNameByGtin(childrenCode.TrueMarkWaterIdentificationCode.GTIN);
						childNode.Parent = parentNode;
						childNode.HasInOrder = hasInOrder;

						parentNode.Children.Add(childNode);
					}
				}

				RefreshCodeScanRows();
			}
		}

		private SelfDeliveryDocumentItem GetNextNotScannedItem(string parsedCodeGtin)
		{
			lock(_selfDeliveryDocument)
			{
				var documentItem = _selfDeliveryDocument.Items?
					.Where(x => x.Nomenclature.IsAccountableInTrueMark)
					.FirstOrDefault(s =>
						s.Nomenclature.Gtins.Select(g => g.GtinNumber).Contains(parsedCodeGtin)
						&& s.TrueMarkProductCodes.Count < s.Amount);

				return documentItem;
			}
		}

		public void CheckCode(string code)
		{
			var cancellationTokenSource = new CancellationTokenSource();

			lock(_cancellationTokenSource)
			{
				_cancellationTokenSource.Add(cancellationTokenSource);
			}

			Task.Run(() => ProcessCheckCodes(code, cancellationTokenSource.Token), cancellationTokenSource.Token);
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
				DocumentType = EdoDocumentType.Bill,
				Order = order
			};

			foreach(var code in codes)
			{
				edoRequest.ProductCodes.Add(code);
			}

			unitOfWork.Save(edoRequest);

			return edoRequest;
		}

		public void SendEdoRequestCreatedEvent(OrderEdoRequest orderEdoRequest)
		{
			_messageBus.Publish(new EdoRequestCreatedEvent { Id = orderEdoRequest.Id });
		}

		public void Dispose()
		{
			lock(_cancellationTokenSource)
			{
				foreach(var cancellationTokenSource in _cancellationTokenSource)
				{
					cancellationTokenSource.Cancel();
				}
			}
		}
	}
}
