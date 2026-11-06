using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services.ReceiptFailedProblem;
using Edo.Problems;
using Edo.Receipt.Sender;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModulKassa;
using ModulKassa.DTO;
using NHibernate;
using NSubstitute;
using QS.DomainModel.UoW;
using Receipt.Dispatcher.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;
using TrueMark.Library;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Rules.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Accounting;
using Vodovoz.Settings.Edo;
using Xunit;
using EdoFiscalStatus = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus;
using ModulFiscalStatus = ModulKassa.DTO.FiscalDocumentStatus;

namespace Receipt.Dispatcher.Tests
{
	public class ReceiptFailedReprocessingServiceTests
	{
		private readonly ILogger<ReceiptFailedReprocessingService> _logger = Substitute.For<ILogger<ReceiptFailedReprocessingService>>();
		private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
		private readonly ISession _session = Substitute.For<ISession>();
		private readonly IOptionsMonitor<ReceiptFailedReprocessingOptions> _options = Substitute.For<IOptionsMonitor<ReceiptFailedReprocessingOptions>>();
		private readonly IEdoRepository _edoRepository = Substitute.For<IEdoRepository>();
		private readonly CashboxClientProvider _cashboxClientProvider = Substitute.For<CashboxClientProvider>();
		private readonly CashboxClient _cashboxClient = Substitute.For<CashboxClient>();
		private readonly FiscalDocumentFactory _fiscalDocumentFactory;
		private readonly ReceiptTrueMarkCodesPool _codesPool;
		private readonly ITrueMarkCodesPoolCodeProvider _poolCodeProvider = Substitute.For<ITrueMarkCodesPoolCodeProvider>();
		private readonly Tag1260Checker _tag1260Checker = Substitute.For<Tag1260Checker>();
		private readonly IEdoReceiptSettings _edoReceiptSettings = Substitute.For<IEdoReceiptSettings>();
		private readonly IAccountingSettings _accountingSettings = Substitute.For<IAccountingSettings>();
		private readonly CanProcessOrSendEdoByClosingAccountingDate _canProcessOrSendEdo;
		private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();

		private readonly Guid _orgCashboxToken = Guid.NewGuid();
		private const string _orgInn = "7800000000";
		private const int _cashboxId = 7;
		private const int _docId = 940718;
		private const int _regDocId = 1;

		public ReceiptFailedReprocessingServiceTests()
		{
			_fiscalDocumentFactory = new FiscalDocumentFactory(_edoReceiptSettings);
			_uow.Session.Returns(_session);

			_options.CurrentValue.Returns(new ReceiptFailedReprocessingOptions
			{
				LookbackDays = 7,
				FailedOlderThanHours = 24
			});

			_canProcessOrSendEdo = Substitute.For<CanProcessOrSendEdoByClosingAccountingDate>(_accountingSettings);
			_canProcessOrSendEdo.Check(Arg.Any<DateTime?>()).Returns(true);

			_edoReceiptSettings.EdoReceiptApiUrl.Returns("http://localhost:5000");
			_edoReceiptSettings.IndustryRequisiteRegulatoryDocumentId.Returns(_regDocId);
			_uow.GetById<FiscalIndustryRequisiteRegulatoryDocument>(_regDocId).Returns(new FiscalIndustryRequisiteRegulatoryDocument
			{
				Id = _regDocId,
				FoivId = "030",
				DocNumber = "1234",
				DocDateTime = DateTime.Today.ToString("dd.MM.yyyy")
			});

			_cashboxClientProvider.GetCashboxAsync(_cashboxId, Arg.Any<CancellationToken>())
				.Returns(_cashboxClient);

			// Setup MySQL GET_LOCK and RELEASE_LOCK mock
			var lockQuery = Substitute.For<ISQLQuery>();
			lockQuery.SetParameter(Arg.Any<string>(), Arg.Any<object>()).Returns(lockQuery);
			lockQuery.UniqueResultAsync<int?>(Arg.Any<CancellationToken>()).Returns(1);
			_session.CreateSQLQuery(Arg.Any<string>()).Returns(lockQuery);

			_codesPool = new ReceiptTrueMarkCodesPool(_uow);
		}

		private (EdoFiscalDocument doc, ReceiptEdoTask task) CreateTestDocAndTask(string gtin = "04600000000001", string cis = "010460000000000121abc")
		{
			var org = new OrganizationEntity
			{
				Id = 1,
				INN = _orgInn,
				CashBoxTokenFromTrueMark = _orgCashboxToken
			};

			var contract = new CounterpartyContractEntity
			{
				Organization = org
			};

			var order = new OrderEntityFixture
			{
				Id = 1001,
				Contract = contract
			};
			order.SetDeliveryDate(DateTime.Today.AddDays(-2));

			var formalRequest = new PrimaryEdoRequest
			{
				Order = order
			};

			var task = new ReceiptEdoTask
			{
				Id = 1731230,
				CashboxId = _cashboxId,
				ReceiptStatus = EdoReceiptStatus.Sending,
				Status = EdoTaskStatus.InProgress,
				FormalEdoRequest = formalRequest
			};

			var productCode = new AutoTrueMarkProductCode
			{
				Id = 500,
				SourceCode = new TrueMarkWaterIdentificationCode
				{
					Id = 200,
					Gtin = gtin,
					SerialNumber = "oldserial1",
					CheckCode = "oldcheck1",
					RawCode = cis
				},
				ResultCode = new TrueMarkWaterIdentificationCode
				{
					Id = 200,
					Gtin = gtin,
					SerialNumber = "oldserial1",
					CheckCode = "oldcheck1",
					RawCode = cis
				},
				SourceCodeStatus = SourceProductCodeStatus.Accepted
			};

			var taskItem = new EdoTaskItem
			{
				ProductCode = productCode
			};

			var inventPos = new FiscalInventPosition
			{
				Id = 10,
				Name = "Вода 19л",
				Quantity = 1,
				Price = 250,
				Vat = FiscalVat.VatFree,
				EdoTaskItem = taskItem,
				IndustryRequisiteData = "UUID=old&Time=123"
			};

			var doc = new EdoFiscalDocument
			{
				Id = _docId,
				DocumentGuid = Guid.NewGuid(),
				CreationTime = DateTime.Now.AddDays(-2),
				Status = EdoFiscalStatus.Failed,
				ReceiptEdoTask = task,
				CheckoutTime = DateTime.Now.AddDays(-2),
				ClientInn = "7712345678"
			};
			doc.InventPositions.Add(inventPos);
			task.FiscalDocuments.Add(doc);

			return (doc, task);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenDocumentAlreadyCompletedInModulKassa_MarksCompletedAndPublishesEvent()
		{
			// Arrange
			var (doc, task) = CreateTestDocAndTask();
			_edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<EdoFiscalDocument> { doc });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Completed,
						TimeStatusChangedString = DateTime.Now.ToString("o"),
						FiscalInfo = new FiscalInfo
						{
							FnDocNumber = 12345,
							FnDocMark = 67890,
							EcrRegistrationNumber = "0000000001",
							Date = DateTime.Now.ToString("o")
						}
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ReplacedCodesCount);
			Assert.Equal(0, result.ErrorCount);

			Assert.Equal(EdoFiscalStatus.Completed, doc.Status);
			Assert.Equal(FiscalDocumentStage.Completed, doc.Stage);
			Assert.Equal(EdoReceiptStatus.Completed, task.ReceiptStatus);
			Assert.Equal(EdoTaskStatus.Completed, task.Status);

			await _publishEndpoint.Received(1).Publish(
				Arg.Is<ReceiptCompleteEvent>(e => e.ReceiptEdoTaskId == task.Id),
				Arg.Any<CancellationToken>());

			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenCodeCanSell_NeedsReplacementIsFalse_UpdatesIndustryRequisiteAndSends()
		{
			// Arrange
			var (doc, task) = CreateTestDocAndTask();
			_edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<EdoFiscalDocument> { doc });

			// ModulKassa check returns FAILED (needs resending)
			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

			// Tag 1260 check says can sell!
			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
					ReqTimestamp = 987654321,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddYears(1),
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			// Send to ModulKassa succeeds
			_cashboxClient.SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Queued,
						TimeStatusChangedString = DateTime.Now.ToString("o")
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ReplacedCodesCount); // NOT replaced!
			Assert.Equal(0, result.ErrorCount);

			// Pool was NOT touched
			await _poolCodeProvider.DidNotReceiveWithAnyArgs().TakeValidCodeAsync(
				default, default(GtinEntity), default, default, default);

			// IndustryRequisiteData was updated
			Assert.Equal("UUID=12345678-1234-1234-1234-123456789012&Time=987654321", doc.InventPositions[0].IndustryRequisiteData);

			// Document stage and status updated
			Assert.Equal(FiscalDocumentStage.Sent, doc.Stage);
			Assert.Equal(EdoFiscalStatus.Queued, doc.Status);

			await _cashboxClient.Received(1).SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenCodeCannotSell_NeedsReplacementIsTrue_TakesCodeFromPoolAndSends()
		{
			// Arrange
			var (doc, task) = CreateTestDocAndTask();
			_edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<EdoFiscalDocument> { doc });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

			var oldCis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			// Old code has expired! (canSell = false => needsReplacement = true)
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(oldCis)),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 111111,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = oldCis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddDays(-1), // EXPIRED!
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			// Pool returns a new code
			var newPoolCode = new TrueMarkWaterIdentificationCode
			{
				Id = 300,
				Gtin = "04600000000001",
				SerialNumber = "newserial2",
				CheckCode = "newcheck2",
				RawCode = "010460000000000121newcode99"
			};

			_poolCodeProvider.TakeValidCodeAsync(
				_codesPool,
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>())
				.Returns(newPoolCode);

			// Tag 1260 check for new code
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(newPoolCode.FormatForCheck1260)),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
					ReqTimestamp = 222222,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = newPoolCode.FormatForCheck1260,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddYears(1),
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			_cashboxClient.SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Queued,
						TimeStatusChangedString = DateTime.Now.ToString("o")
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(1, result.ReplacedCodesCount); // Replaced 1 code!
			Assert.Equal(0, result.ErrorCount);

			// Code was replaced in productCode
			var updatedProductCode = doc.InventPositions[0].EdoTaskItem.ProductCode;
			Assert.Equal(newPoolCode.Id, updatedProductCode.ResultCode.Id);
			Assert.Equal(SourceProductCodeStatus.Changed, updatedProductCode.SourceCodeStatus);

			// IndustryRequisiteData has new UUID and Time
			Assert.Equal("UUID=11111111-2222-3333-4444-555555555555&Time=222222", doc.InventPositions[0].IndustryRequisiteData);

			await _cashboxClient.Received(1).SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenOrderInClosedPeriod_SkipsReprocessing()
		{
			// Arrange
			var (doc, task) = CreateTestDocAndTask();

			_canProcessOrSendEdo.Check(Arg.Any<DateTime?>()).Returns(false);

			_edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<EdoFiscalDocument> { doc });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(1, result.SkippedClosedPeriodCount);
			Assert.Equal(0, result.ErrorCount);

			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(default, default);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenPoolMissingCode_UpdatesDocumentFailureMessageAndDoesNotSend()
		{
			// Arrange
			var (doc, task) = CreateTestDocAndTask();
			_edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<EdoFiscalDocument> { doc });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

			var oldCis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			// Code expired
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(oldCis)),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 111111,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = oldCis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddDays(-1), // EXPIRED
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			// Pool throws EdoCodePoolMissingCodeException
			_poolCodeProvider.TakeValidCodeAsync(
				_codesPool,
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				addingTimeNotAfter: doc.CreationTime)
				.Returns<Task<TrueMarkWaterIdentificationCode>>(_ => throw new EdoCodePoolMissingCodeException("В пуле не найден валидный код для GTIN"));

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(0, result.ReplacedCodesCount);
			Assert.Equal(1, result.ErrorCount);

			Assert.Equal("В пуле не найден валидный код для GTIN", doc.FailureMessage);
			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(default, default);
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenFailureIsUnrecoverable_BlocksReprocessingAndRegistersProblem()
		{
			// Arrange
			var (doc, task) = CreateTestDocAndTask();
			_edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<EdoFiscalDocument> { doc });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "FnMemoryFull",
							Message = "Память ФН исчерпана"
						}
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(1, result.ErrorCount);

			Assert.Contains("Память ФН исчерпана", doc.FailureMessage);
			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(default, default);
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenFailureIsTimeout24Hours_EligibilityAllowed_ConvertsToCorrectionAndSends()
		{
			// Arrange
			var (doc, task) = CreateTestDocAndTask();
			_edoRepository.GetFailedFiscalDocumentsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<EdoFiscalDocument> { doc });

			// ModulKassa returned timeout 24 hours (failureInfo is null, message has timeout text)
			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ожидание фискализации превысило 24 часа",
						FailureInfo = null,
						FnState = "NOT_READY" // historical cashbox state must not block reprocessing!
					}
				});

			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			// Code is valid
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(cis)),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 333333,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddYears(1),
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			_cashboxClient.SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Queued,
						TimeStatusChangedString = DateTime.Now.ToString("o")
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);
			Assert.Equal(FiscalDocumentType.SaleCorrection, doc.DocumentType);

			await _cashboxClient.Received(1).SendFiscalDocument(
				Arg.Is<FiscalDocument>(fd => fd.DocType == "SALE_CORRECTION"),
				Arg.Any<CancellationToken>());
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		private ReceiptFailedReprocessingService CreateService(EdoProblemRegistrar edoProblemRegistrar = null)
		{
			return new ReceiptFailedReprocessingService(
				_logger,
				_uow,
				_options,
				_edoRepository,
				_cashboxClientProvider,
				_fiscalDocumentFactory,
				_codesPool,
				_poolCodeProvider,
				_tag1260Checker,
				_edoReceiptSettings,
				_canProcessOrSendEdo,
				_publishEndpoint,
				edoProblemRegistrar);
		}
	}
}
