using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services.ReceiptFailedProblem;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Custom.Sources;
using Edo.Receipt.Sender;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Notifications.Infrastructure;
using MassTransit;
using TransactionalOutbox.MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModulKassa;
using ModulKassa.DTO;
using NHibernate;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
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
using Vodovoz.Core.Domain.Edo.NotificationSettings;
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
		private readonly IUnitOfWorkFactory _uowFactory = Substitute.For<IUnitOfWorkFactory>();
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
		private readonly EdoProblemRegistrar _edoProblemRegistrar;

		private readonly Guid _orgCashboxToken = Guid.NewGuid();
		private const string _orgInn = "7800000000";
		private const int _cashboxId = 7;
		private const int _docId = 940718;
		private const int _regDocId = 1;

		public ReceiptFailedReprocessingServiceTests()
		{
			_fiscalDocumentFactory = new FiscalDocumentFactory(_edoReceiptSettings);
			_uow.Session.Returns(_session);

			var transaction = Substitute.For<ITransaction>();
			_session.BeginTransaction().Returns(transaction);

			_uowFactory.CreateWithoutRoot(Arg.Any<string>()).ReturnsForAnyArgs(_uow);
			_uowFactory.CreateWithoutRoot().ReturnsForAnyArgs(_uow);

			_uow.When(x => x.SaveAsync(Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()))
				.Do(callInfo =>
				{
					if(callInfo.Arg<object>() is EdoTaskProblem prob && prob.EdoTask != null)
					{
						if(!prob.EdoTask.Problems.Contains(prob))
						{
							prob.EdoTask.Problems.Add(prob);
						}
					}
				});

			_options.CurrentValue.Returns(new ReceiptFailedReprocessingOptions
			{
				LookbackDays = 14,
				FailedOlderThanHours = 24,
				MaxReprocessingAttempts = 3,
				WorkerInterval = TimeSpan.FromMinutes(30),
				CriticalFiscalDriveErrors = new List<string> { "FnMemoryFull", "FnCrypt", "ResourceExhausted" },
				CriticalValidationErrors = new List<string> { "ValidationError", "InvalidPaymentDetails" }
			});

			_canProcessOrSendEdo = Substitute.For<CanProcessOrSendEdoByClosingAccountingDate>(_accountingSettings);
			_canProcessOrSendEdo.Check(Arg.Any<DateTime?>()).Returns(true);
			_canProcessOrSendEdo.GetCheckingDate().Returns(DateTime.Today.AddDays(-30));

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

			_cashboxClient.RequeueFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Queued,
						TimeStatusChangedString = DateTime.Now.ToString("o")
					}
				});

			var lockQuery = Substitute.For<ISQLQuery>();
			lockQuery.SetParameter(Arg.Any<string>(), Arg.Any<object>()).Returns(lockQuery);
			lockQuery.UniqueResultAsync<int?>(Arg.Any<CancellationToken>()).Returns(1);
			_session.CreateSQLQuery(Arg.Any<string>()).Returns(lockQuery);

			_codesPool = new ReceiptTrueMarkCodesPool(_uow);
			_edoProblemRegistrar = CreateEdoProblemRegistrarFixture();
		}

		private EdoProblemRegistrar CreateEdoProblemRegistrarFixture()
		{
			var customSources = new EdoTaskProblemCustomSource[]
			{
				new ReceiptFiscalizationFailed(),
				new ReceiptSendingFailed()
			};
			var customSourcesPersister = new EdoTaskCustomSourcesPersister(_uowFactory, customSources);
			var exceptionSourcesPersister = new EdoTaskExceptionSourcesPersister(_uowFactory, Enumerable.Empty<EdoTaskProblemExceptionSource>());
			return new EdoProblemRegistrar(_uow, _uowFactory, customSourcesPersister, exceptionSourcesPersister,
				Substitute.For<INotificationPublisher<EdoNotificationMessage>>());
		}

		private class TestDocAndTask
		{
			public EdoFiscalDocument Doc { get; set; }
			public ReceiptEdoTask Task { get; set; }
		}

		private TestDocAndTask CreateTestDocAndTask(
			string gtin = "04600000000001",
			string cis = "010460000000000121abc",
			DateTime? checkoutTime = null)
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
				FormalEdoRequest = formalRequest,
				Problems = new ObservableList<EdoTaskProblem>()
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
				CheckoutTime = checkoutTime ?? DateTime.Now.AddDays(-2),
				ClientInn = "7712345678"
			};
			doc.InventPositions.Add(inventPos);
			task.FiscalDocuments.Add(doc);

			_uow.GetById<EdoFiscalDocument>(_docId).Returns(doc);
			_uow.GetById<ReceiptEdoTask>(task.Id).Returns(task);
			_uow.GetById<EdoTask>(task.Id).Returns(task);

			return new TestDocAndTask { Doc = doc, Task = task };
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenDocumentAlreadyCompletedInModulKassa_MarksCompletedAndPublishesEvent()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

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
		public async Task ProcessFailedDocumentsAsync_WhenCodeCanSell_NeedsReplacementIsFalse_DoesNotOverwriteIndustryRequisiteAndRequeues()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

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
			Assert.Equal(0, result.ReplacedCodesCount);
			Assert.Equal(0, result.ErrorCount);

			await _poolCodeProvider.DidNotReceiveWithAnyArgs().TakeValidCodeAsync(
				default, default(GtinEntity), default, default, default);

			// При re-queue IndustryRequisiteData не перезаписывается, сохраняются исходные данные
			Assert.Equal("UUID=old&Time=123", doc.InventPositions[0].IndustryRequisiteData);
			Assert.Equal(FiscalDocumentStage.Sent, doc.Stage);
			Assert.Equal(EdoFiscalStatus.Queued, doc.Status);

			await _cashboxClient.Received(1).RequeueFiscalDocument(
				doc.DocumentGuid.ToString(),
				Arg.Any<CancellationToken>());
			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(default, default);
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenCodeCannotSell_NeedsReplacementIsTrue_TakesCodeFromPoolAndSends()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

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
							ExpireDate = DateTime.Now.AddDays(-1), // Просрочен
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			var newPoolCode = new TrueMarkWaterIdentificationCode
			{
				Id = 300,
				Gtin = "04600000000001",
				SerialNumber = "newserial2",
				CheckCode = "newcheck2",
				RawCode = "010460000000000121newcode99"
			};

			_poolCodeProvider.TakeValidCodeAsync(
				Arg.Any<ReceiptTrueMarkCodesPool>(),
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>())
				.Returns(newPoolCode);

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
			Assert.Equal(1, result.ReplacedCodesCount);
			Assert.Equal(0, result.ErrorCount);

			var updatedProductCode = doc.InventPositions[0].EdoTaskItem.ProductCode;
			Assert.Equal(newPoolCode.Id, updatedProductCode.ResultCode.Id);
			Assert.Equal(SourceProductCodeStatus.Changed, updatedProductCode.SourceCodeStatus);
			Assert.Equal("UUID=11111111-2222-3333-4444-555555555555&Time=222222", doc.InventPositions[0].IndustryRequisiteData);

			await _cashboxClient.Received(1).SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenClosedPeriodDateIsLaterThanLookback_UsesClosedPeriodDateAsStartDate()
		{
			// Arrange
			var closedDate = DateTime.Today.AddDays(-5);
			_canProcessOrSendEdo.GetCheckingDate().Returns(closedDate);

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Is<DateTime>(d => d == closedDate),
				Arg.Any<CancellationToken>())
				.Returns(new List<int>());

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(0, result.TotalFound);
			await _edoRepository.Received(1).GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Is<DateTime>(d => d == closedDate),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenPoolMissingCode_UpdatesDocumentFailureMessageAndDoesNotSend()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

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
							ExpireDate = DateTime.Now.AddDays(-1),
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			_poolCodeProvider.TakeValidCodeAsync(
				Arg.Any<ReceiptTrueMarkCodesPool>(),
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
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

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
		public async Task ProcessFailedDocumentsAsync_WhenFailureIsValidationError_BlocksReprocessingAndRegistersProblem()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "ValidationError",
							Message = "Некорректный ИНН покупателя"
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

			Assert.Contains("Некорректный ИНН покупателя", doc.FailureMessage);
			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(default, default);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenFailureIsTimeout24Hours_SendsAsSaleViaPost()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ожидание фискализации превысило 24 часа",
						FailureInfo = null,
						FnState = "NOT_READY"
					}
				});

			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

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
			Assert.Equal(FiscalDocumentType.Sale, doc.DocumentType);

			await _cashboxClient.Received(1).RequeueFiscalDocument(
				doc.DocumentGuid.ToString(),
				Arg.Any<CancellationToken>());
			await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenNonMarkedDocument_SendsAsSaleViaRequeue()
		{
			// Arrange - CheckoutTime был 3 часа назад, без маркированных позиций
			var testData = CreateTestDocAndTask(checkoutTime: DateTime.Now.AddHours(-3));
			var doc = testData.Doc;
			doc.InventPositions.Clear();
			doc.InventPositions.Add(new FiscalInventPosition
			{
				Id = 10,
				Name = "Помпа для воды",
				Quantity = 1,
				Price = 1000,
				Vat = FiscalVat.Vat20
			});

			var task = testData.Task;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ошибка связи",
						FailureInfo = null
					}
				});

			_cashboxClient.RequeueFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
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
			Assert.Equal(FiscalDocumentType.Sale, doc.DocumentType);

			await _cashboxClient.Received(1).RequeueFiscalDocument(
				doc.DocumentGuid.ToString(),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenDocumentIsInProcessInModulKassa_SkipsReprocessingToPreventDuplicate()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Pending
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);
			Assert.Equal(1, result.SkippedCount);

			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenModulKassaReturnsCommunicationError_RegistersProblemAndCountsAsError()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Error,
					ErrorMessage = "500 InternalServerError: connection timed out"
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(1, result.ErrorCount);
			Assert.Equal(0, result.SkippedCount);

			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenTaskHasPreviousProblems_ContinuesReprocessingWithoutMaxAttemptsLimit()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;

			// Задача имеет 5 предыдущих проблем
			for(int i = 0; i < 5; i++)
			{
				task.Problems.Add(new EdoTaskProblem { SourceName = nameof(ReceiptFiscalizationFailed) });
			}

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

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
							Realizable = true,
							Utilised = true
						}
					}
				});

			_cashboxClient.SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Queued
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert: не блокируется лимитом попыток, отправляет документ
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);

			await _cashboxClient.Received().RequeueFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenCashboxReturnsSuccessWithFailedStatus_HandlesAsError()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			var cis = doc.InventPositions.First().EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Error,
					ErrorMessage = "HTTP Code: 404 NotFound"
				});

			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 123456789,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddDays(30),
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
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "FiscalDriveFailure",
							Message = "Касса не смогла фискализировать чек"
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
			Assert.Equal(0, result.SkippedCount);
			await _publishEndpoint.DidNotReceiveWithAnyArgs().Publish(Arg.Any<ReceiptCompleteEvent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenDocumentIdContains404WithInternalServerError_DoesNotTreatAsNotFound()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			// Ошибка 500, но в тексте сообщения есть подстрока 404 в ID документа
			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Error,
					ErrorMessage = "Не удалось получить актуальный статус чека для документа №404a98-1234. HTTP Code: 500 InternalServerError"
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert: ошибка связи регистрирует проблему (Error), а не дублирует отправку (SendFiscalDocument не вызывается)
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(1, result.ErrorCount);
			Assert.Equal(0, result.SkippedCount);
			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenTaskHasMultipleDocumentsAndOnlyOneCompleted_DoesNotCompleteTaskOrPublishEvent()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc1 = testData.Doc;
			var task = testData.Task;

			// Второй документ в задаче ещё не фискализирован
			var doc2 = new EdoFiscalDocument
			{
				Id = _docId + 1,
				DocumentGuid = Guid.NewGuid(),
				CreationTime = DateTime.Now.AddDays(-2),
				CheckoutTime = DateTime.Now.AddDays(-2),
				ReceiptEdoTask = task,
				Status = EdoFiscalStatus.Failed,
				Stage = FiscalDocumentStage.Preparing
			};
			task.FiscalDocuments.Add(doc2);

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc1.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
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

			// Assert: документ 1 переведен в Completed, но общая задача ещё НЕ Completed, событие НЕ отправлено
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(EdoFiscalStatus.Completed, doc1.Status);
			Assert.NotEqual(EdoTaskStatus.Completed, task.Status);
			await _publishEndpoint.DidNotReceiveWithAnyArgs().Publish(Arg.Any<ReceiptCompleteEvent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenExpireDateIsDefault_DoesNotRejectCodeAsExpired()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			var cis = doc.InventPositions.First().EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Error,
					ErrorMessage = "HTTP Code: 404 NotFound"
				});

			// Код в ЧЗ с default датой срока годности
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 123456789,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = default,
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

			// Assert: код признан валидным, замена из пула НЕ производилась (ReplacedCodesCount == 0)
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ReplacedCodesCount);
			await _poolCodeProvider.DidNotReceiveWithAnyArgs().TakeValidCodeAsync(
				Arg.Any<ITrueMarkCodesPool>(),
				Arg.Any<GtinEntity>(),
				Arg.Any<string>(),
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenOrderInClosedPeriod_ButAlreadyCompletedInCashbox_SyncsAndCompletesSuccessfully()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;

			// Закрытый период включен
			_canProcessOrSendEdo.Check(Arg.Any<DateTime?>()).Returns(false);

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			// В МодульКассе чек уже фактически фискализирован со статусом Completed
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
							KktNumber = "0000000001",
							Date = DateTime.Now.ToString("o")
						}
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert: чек успешно синхронизирован в Completed, закрытый период не заблокировал факт сверки
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);

			Assert.Equal(EdoFiscalStatus.Completed, doc.Status);
			Assert.Equal(FiscalDocumentStage.Completed, doc.Stage);
			await _publishEndpoint.Received(1).Publish(
				Arg.Is<ReceiptCompleteEvent>(e => e.ReceiptEdoTaskId == task.Id),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_AssignsNewDocumentGuidAndSavesToDbBeforeSend()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var initialGuid = doc.DocumentGuid;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(initialGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "MarkingCheckViolated",
							Message = "Код маркировки не прошел проверку"
						}
					}
				});

			var cis = doc.InventPositions.First().EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			_tag1260Checker.CheckCodesForTag1260Async(Arg.Any<IEnumerable<string>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(callInfo =>
				{
					var requestedCises = callInfo.Arg<IEnumerable<string>>();
					return new CodeCheckResponse
					{
						Code = 0,
						ReqId = Guid.NewGuid(),
						ReqTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
						Codes = requestedCises.Select(c => new CodeCheckInfo
						{
							Cis = c,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false,
							ExpireDate = c == cis ? DateTime.Now.AddDays(-1) : DateTime.Now.AddYears(1)
						}).ToList()
					};
				});

			var replacementCode = new TrueMarkWaterIdentificationCode
			{
				Id = 555,
				Gtin = "04600000000001",
				SerialNumber = "sn123",
				CheckCode = "cc123",
				RawCode = "010460000000000121sn123"
			};
			_poolCodeProvider.TakeValidCodeAsync(
				Arg.Any<ReceiptTrueMarkCodesPool>(),
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>())
				.Returns(replacementCode);

			FiscalDocument capturedDocument = null;
			_cashboxClient.SendFiscalDocument(Arg.Do<FiscalDocument>(fd => capturedDocument = fd), Arg.Any<CancellationToken>())
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
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(1, result.ReplacedCodesCount);
			Assert.NotNull(capturedDocument);
			Assert.NotEqual(initialGuid, doc.DocumentGuid);
			Assert.Equal(doc.DocumentGuid.ToString(), capturedDocument.Id);
			Assert.Contains(doc.DocumentGuid.ToString(), capturedDocument.ResponseURL);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenMarkingCodesChecked_AssignsNewGuidAndSendsViaPost()
		{
			// Arrange
			var initialGuid = Guid.NewGuid();
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			doc.DocumentGuid = initialGuid;
			var task = testData.Task;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(initialGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "MarkingCheckViolated",
							Message = "Код маркировки не прошел проверку"
						}
					}
				});

			var cis = doc.InventPositions.First().EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			_tag1260Checker.CheckCodesForTag1260Async(Arg.Any<IEnumerable<string>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false,
							ExpireDate = DateTime.Now.AddDays(30)
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

			// Assert: если коды валидны и не заменялись, повторное проведение идет через re-queue со старым GUID
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ReplacedCodesCount);
			Assert.Equal(initialGuid, doc.DocumentGuid);
			await _cashboxClient.Received(1).RequeueFiscalDocument(initialGuid.ToString(), Arg.Any<CancellationToken>());
			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(default, default);
		}

		[Fact]
		public void FiscalDocumentFactory_CheckoutDateTime_HandlesUtcLocalAndUnspecified()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;

			// 1. Unspecified - в БД хранится московское время, форматируется с +00:00 для МодульКассы
			doc.CheckoutTime = new DateTime(2026, 11, 1, 15, 30, 0, DateTimeKind.Unspecified);
			var resUnspecified = _fiscalDocumentFactory.CreateFiscalDocument(doc);
			Assert.Equal("2026-11-01T15:30:00+00:00", resUnspecified.CheckoutDateTime);

			// 2. Local
			doc.CheckoutTime = new DateTime(2026, 11, 1, 15, 30, 0, DateTimeKind.Local);
			var resLocal = _fiscalDocumentFactory.CreateFiscalDocument(doc);
			Assert.Matches(@"^2026-11-01T\d{2}:30:00\+00:00$", resLocal.CheckoutDateTime);

			// 3. Utc - должен сконвертироваться в московское время (+3ч)
			var utcTime = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);
			doc.CheckoutTime = utcTime;
			var resUtc = _fiscalDocumentFactory.CreateFiscalDocument(doc);
			Assert.Equal("2026-11-01T15:00:00+00:00", resUtc.CheckoutDateTime);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenTag1260Returns401Unauthorized_RegistersProblemAndDoesNotSkip()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "MarkingCheckViolated",
							Message = "Ошибка проверки маркировки"
						}
					}
				});

			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.ThrowsAsync(new Tag1260ApiException(System.Net.HttpStatusCode.Unauthorized, "Unauthorized: bad organization token"));

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert - 401 НЕ считается временным сетевым сбоем, НЕ пропускается (Skipped == 0), фиксируется ошибка
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(0, result.SkippedCount);
			Assert.Equal(1, result.ErrorCount);
			Assert.Contains("Unauthorized", doc.FailureMessage);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenReqTimestampInMilliseconds_NormalizesToSeconds()
		{
			// Arrange - 13 цифр в миллисекундах: 1736179200000 -> должно стать 1736179200
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var code = (testData.Doc.InventPositions[0].EdoTaskItem.ProductCode as AutoTrueMarkProductCode).ResultCode;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				_uow,
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "MarkingCheckViolated",
							Message = "Ошибка проверки маркировки"
						}
					}
				});

			var newPoolCode = new TrueMarkWaterIdentificationCode
			{
				Id = 555,
				Gtin = "04601234567890",
				SerialNumber = "SN55555",
				CheckCode = "check55555"
			};

			_poolCodeProvider.TakeValidCodeAsync(
				Arg.Any<ReceiptTrueMarkCodesPool>(),
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>())
				.Returns(newPoolCode);

			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(code.FormatForCheck1260)),
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
							Cis = code.FormatForCheck1260,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddDays(-1), // Просрочен -> требует замены
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(newPoolCode.FormatForCheck1260)),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
					ReqTimestamp = 1736179200000L, // миллисекунды (13 цифр)
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = newPoolCode.FormatForCheck1260,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false,
							ExpireDate = DateTime.Now.AddYears(1)
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

			// Assert - ReqTimestamp нормализован до 10 цифр (секунды): 1736179200
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal("UUID=12345678-1234-1234-1234-123456789012&Time=1736179200", doc.InventPositions[0].IndustryRequisiteData);
		}

		[Theory]
		[InlineData("UUID=123&Time=1736179200000", "UUID=123&Time=1736179200")]
		[InlineData("UUID=abc&Time=1736179200", "UUID=abc&Time=1736179200")]
		[InlineData("Time=1736179200000", "Time=1736179200")]
		[InlineData("", "")]
		[InlineData(null, null)]
		public void FiscalDocumentFactory_NormalizeDocData_NormalizesCorrectly(string input, string expected)
		{
			// Act
			var actual = FiscalDocumentFactory.NormalizeDocData(input);

			// Assert
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(System.Net.HttpStatusCode.RequestTimeout, true)]
		[InlineData((System.Net.HttpStatusCode)429, true)]
		[InlineData(System.Net.HttpStatusCode.InternalServerError, true)]
		[InlineData(System.Net.HttpStatusCode.BadGateway, true)]
		[InlineData(System.Net.HttpStatusCode.ServiceUnavailable, true)]
		[InlineData(System.Net.HttpStatusCode.GatewayTimeout, true)]
		[InlineData(System.Net.HttpStatusCode.BadRequest, false)]
		[InlineData(System.Net.HttpStatusCode.Unauthorized, false)]
		[InlineData(System.Net.HttpStatusCode.Forbidden, false)]
		[InlineData(System.Net.HttpStatusCode.NotFound, false)]
		[InlineData(System.Net.HttpStatusCode.NotImplemented, false)]
		public void Tag1260ApiException_IsTransient_ClassifiesCorrectly(System.Net.HttpStatusCode code, bool expectedTransient)
		{
			// Arrange & Act
			var ex = new Tag1260ApiException(code, "test");

			// Assert
			Assert.Equal(expectedTransient, ex.IsTransient);
		}

		[Theory]
		[InlineData(ReasonForLeaving.Resale)]
		[InlineData(ReasonForLeaving.Tender)]
		public async Task ProcessFailedDocumentsAsync_WhenOrderReasonForLeavingIsResaleOrTender_AndCodesInvalid_DoesNotReplaceFromPoolAndRegistersProblem(ReasonForLeaving reason)
		{
			// Arrange
			var gtin = "04600000000001";
			var pair = CreateTestDocAndTask(gtin);
			var doc = pair.Doc;
			var task = pair.Task;
			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			((OrderEntityFixture)task.FormalEdoRequest.Order).SetClient(new CounterpartyEntity
			{
				ReasonForLeaving = reason
			});

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
				.Returns(new List<int> { doc.Id });

			_uow.GetById<EdoFiscalDocument>(doc.Id).Returns(doc);
			_uow.GetById<ReceiptEdoTask>(task.Id).Returns(task);

			_cashboxClient.CheckFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "MarkingCheckViolated",
							Message = "Ошибка проверки маркировки"
						}
					}
				});

			// Код не проходит проверку разрешительного режима
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
					ReqTimestamp = 123456789,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							Valid = false,
							ErrorCode = 1
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
			Assert.Equal(0, result.SkippedCount);

			// Из пула кодов для собственных нужд ничего НЕ бралось!
			await _poolCodeProvider.DidNotReceiveWithAnyArgs().TakeValidCodeAsync(
				default, default(GtinEntity), default, default, default);

			// Зафиксирована ошибка о запрете автозамены кодов из пула для перепродажи/госзакупок
			Assert.Contains("автозамена кодов маркировки из пула запрещена", doc.FailureMessage);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenOrderReasonForLeavingIsResale_AndCodesAreValid_ResendsWithoutCodeReplacement()
		{
			// Arrange
			var gtin = "04600000000001";
			var pair = CreateTestDocAndTask(gtin);
			var doc = pair.Doc;
			var task = pair.Task;
			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			((OrderEntityFixture)task.FormalEdoRequest.Order).SetClient(new CounterpartyEntity
			{
				ReasonForLeaving = ReasonForLeaving.Resale
			});

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
				.Returns(new List<int> { doc.Id });

			_uow.GetById<EdoFiscalDocument>(doc.Id).Returns(doc);
			_uow.GetById<ReceiptEdoTask>(task.Id).Returns(task);

			_cashboxClient.CheckFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "MarkingCheckViolated",
							Message = "Ошибка проверки маркировки"
						}
					}
				});

			// Код валиден в Честном Знаке
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
					ReqTimestamp = 123456789,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							Valid = true,
							Verified = true,
							Found = true,
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false,
							ErrorCode = 0,
							ExpireDate = DateTime.Now.AddDays(30)
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
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);

			// Из пула ничего не бралось
			await _poolCodeProvider.DidNotReceiveWithAnyArgs().TakeValidCodeAsync(
				default, default(GtinEntity), default, default, default);

			// Чек отправлен на фискализацию с обновленным реквизитом 1260
			Assert.Equal(FiscalDocumentStage.Sent, doc.Stage);
			Assert.Equal(EdoFiscalStatus.Queued, doc.Status);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenNetworkErrorOccurs_ImmediatelyRegistersProblemOnFirstFailure()
		{
			// Arrange
			var pair = CreateTestDocAndTask();
			var doc = pair.Doc;
			var task = pair.Task;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
				.Returns(new List<int> { doc.Id });

			_uow.GetById<EdoFiscalDocument>(doc.Id).Returns(doc);
			_uow.GetById<ReceiptEdoTask>(task.Id).Returns(task);

			_cashboxClient.CheckFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						FailureInfo = new FiscalFailureInfo
						{
							Type = "MarkingCheckViolated",
							Message = "Ошибка маркировки"
						}
					}
				});

			// Сбой сети при проверке кодов
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.ThrowsAsync(new Tag1260CdnUnavailableException("CDN недоступен"));

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert: сразу регистрирует проблему без 5 пропусков
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(0, result.SkippedCount);
			Assert.Equal(1, result.ErrorCount);
			Assert.Contains("Сбой связи с CDN Честного Знака", doc.FailureMessage);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenDocumentStatusInCashboxIsQueued_SkipsReprocessing()
		{
			// Arrange
			var pair = CreateTestDocAndTask();
			var doc = pair.Doc;
			var task = pair.Task;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
				.Returns(new List<int> { doc.Id });

			_uow.GetById<EdoFiscalDocument>(doc.Id).Returns(doc);
			_uow.GetById<ReceiptEdoTask>(task.Id).Returns(task);

			_cashboxClient.CheckFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Queued
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SkippedCount);
			Assert.Equal(0, result.ErrorCount);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenResendSucceeds_SetsSentStageAndDoesNotSolveProblemEarly()
		{
			// Arrange
			var pair = CreateTestDocAndTask();
			var doc = pair.Doc;
			var task = pair.Task;

			var activeProblem = new EdoTaskProblem
			{
				SourceName = "Custom.ReceiptFiscalizationFailed",
				State = TaskProblemState.Active
			};
			task.Problems.Add(activeProblem);
			task.Status = EdoTaskStatus.Problem;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
				.Returns(new List<int> { doc.Id });

			_uow.GetById<EdoFiscalDocument>(doc.Id).Returns(doc);
			_uow.GetById<ReceiptEdoTask>(task.Id).Returns(task);

			_cashboxClient.CheckFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

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
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			var fiscalizationSuccessResult = new FiscalizationResult
			{
				SendStatus = SendStatus.Success,
				FiscalDocumentInfo = new FiscalDocumentInfo
				{
					Status = ModulFiscalStatus.Queued,
					TimeStatusChangedString = DateTime.Now.ToString("o")
				}
			};

			_cashboxClient.SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>())
				.Returns(fiscalizationSuccessResult);
			_cashboxClient.RequeueFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(fiscalizationSuccessResult);

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);
			Assert.Equal(FiscalDocumentStage.Sent, doc.Stage);
			Assert.Equal(EdoFiscalStatus.Queued, doc.Status);
			Assert.Equal(TaskProblemState.Active, activeProblem.State);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenOnlyQueued_DoesNotSolveProblemEarly()
		{
			// Arrange
			var pair = CreateTestDocAndTask();
			var doc = pair.Doc;
			var task = pair.Task;

			var activeProblem = new EdoTaskProblem
			{
				SourceName = "Custom.ReceiptFiscalizationFailed",
				State = TaskProblemState.Active
			};
			task.Problems.Add(activeProblem);
			task.Status = EdoTaskStatus.Problem;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
				.Returns(new List<int> { doc.Id });

			_uow.GetById<EdoFiscalDocument>(doc.Id).Returns(doc);
			_uow.GetById<ReceiptEdoTask>(task.Id).Returns(task);

			_cashboxClient.CheckFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed
					}
				});

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
							Realizable = true,
							Utilised = true
						}
					}
				});

			var queuedResult = new FiscalizationResult
			{
				SendStatus = SendStatus.Success,
				FiscalDocumentInfo = new FiscalDocumentInfo
				{
					Status = ModulFiscalStatus.Queued,
					FiscalInfo = null
				}
			};

			_cashboxClient.SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>())
				.Returns(queuedResult);
			_cashboxClient.RequeueFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
				.Returns(queuedResult);

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);

			// Проблема НЕ должна закрываться, пока чек только в очереди кассы (не фискализирован)
			Assert.Equal(TaskProblemState.Active, activeProblem.State);
		}

		private readonly ILogger<ReceiptFailedDocumentReprocessingProcessor> _processorLogger = Substitute.For<ILogger<ReceiptFailedDocumentReprocessingProcessor>>();

		private ReceiptFailedReprocessingService CreateService()
		{
			var processor = new ReceiptFailedDocumentReprocessingProcessor(
				_processorLogger,
				_uow,
				_uowFactory,
				_options,
				_edoRepository,
				_cashboxClientProvider,
				_fiscalDocumentFactory,
				_poolCodeProvider,
				_tag1260Checker,
				_edoReceiptSettings,
				_publishEndpoint,
				_edoProblemRegistrar,
				_codesPool);

			var scopeFactory = Substitute.For<IServiceScopeFactory>();
			var scope = Substitute.For<IServiceScope>();
			var sp = Substitute.For<IServiceProvider>();

			scopeFactory.CreateScope().Returns(scope);
			scope.ServiceProvider.Returns(sp);
			sp.GetService(typeof(IUnitOfWork)).Returns(_uow);
			sp.GetService(typeof(IReceiptFailedDocumentReprocessingProcessor)).Returns(processor);

			return new ReceiptFailedReprocessingService(
				_logger,
				scopeFactory,
				_options,
				_edoRepository,
				_canProcessOrSendEdo);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenCashboxCompletedWithoutFiscalInfo_SkipsDocument()
		{
			// Arrange - касса вернула статус Printed, но без FiscalInfo
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;

			var activeProblem = new EdoTaskProblem
			{
				SourceName = "Custom.ReceiptFiscalizationFailed",
				State = TaskProblemState.Active
			};
			task.Problems.Add(activeProblem);
			task.Status = EdoTaskStatus.Problem;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Printed,
						FiscalInfo = null
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert: документ пропущен, фискализация не завершена, проблема остается Active
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);
			Assert.Equal(1, result.SkippedCount);
			Assert.Equal(TaskProblemState.Active, activeProblem.State);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenOneOfMultipleDocumentsStillFailed_DoesNotSolveProblemOnTask()
		{
			// Arrange - в задаче 2 документа: doc1 фискализирован в кассе, doc2 еще Failed
			var testData = CreateTestDocAndTask();
			var doc1 = testData.Doc;
			var task = testData.Task;

			var doc2 = new EdoFiscalDocument
			{
				Id = 99999,
				DocumentGuid = Guid.NewGuid(),
				Status = EdoFiscalStatus.Failed,
				Stage = FiscalDocumentStage.Sent,
				ReceiptEdoTask = task
			};
			task.FiscalDocuments.Add(doc2);

			var activeProblem = new EdoTaskProblem
			{
				SourceName = "Custom.ReceiptFiscalizationFailed",
				State = TaskProblemState.Active
			};
			task.Problems.Add(activeProblem);
			task.Status = EdoTaskStatus.Problem;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc1.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Completed,
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

			// Assert: поскольку doc2 все еще Failed, проблема НЕ должна закрываться на уровне задачи
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(TaskProblemState.Active, activeProblem.State);
			await _publishEndpoint.DidNotReceiveWithAnyArgs().Publish(Arg.Any<ReceiptCompleteEvent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenMaxReprocessingAttemptsExceeded_SkipsResending()
		{
			// Arrange - попытки исчерпаны (RetryCount = 3 при MaxReprocessingAttempts = 3)
			_options.CurrentValue.Returns(new ReceiptFailedReprocessingOptions
			{
				LookbackDays = 7,
				FailedOlderThanHours = 1,
				MaxReprocessingAttempts = 3,
				CriticalFiscalDriveErrors = new List<string> { "FnMemoryFull" },
				CriticalValidationErrors = new List<string> { "ValidationError" }
			});

			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			doc.FailureMessage = "MarkingCheckFlowViolated";
			var task = testData.Task;

			var problem = new EdoTaskProblem
			{
				SourceName = "Custom.ReceiptFiscalizationFailed",
				State = TaskProblemState.Active
			};
			problem.RoutineStates.Add(new EdoTaskProblemRoutineState
			{
				Problem = problem,
				RetryCount = 3,
				LastRetryTime = DateTime.Now.AddHours(-2)
			});
			task.Problems.Add(problem);

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			// Касса возвращает Failed
			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ошибка фискализации"
					}
				});

			var service = CreateService();

			// Act
			var result = await service.ProcessFailedDocumentsAsync(CancellationToken.None);

			// Assert: документ пропущен без повторной отправки из-за исчерпания попыток
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(0, result.SuccessCount);
			Assert.Equal(0, result.ErrorCount);
			Assert.Equal(1, result.SkippedCount);

			Assert.Contains("MarkingCheckFlowViolated", doc.FailureMessage);
			Assert.Contains("Исчерпан лимит", doc.FailureMessage);
			await _cashboxClient.DidNotReceiveWithAnyArgs().SendFiscalDocument(default, default);
			await _cashboxClient.DidNotReceiveWithAnyArgs().RequeueFiscalDocument(default, default);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenNewGuidAssigned_RecordsGuidHistoryInFailureMessage()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var oldGuid = doc.DocumentGuid;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ошибка маркировки"
					}
				});

			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(cis)),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 123456,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddDays(-1),
							Realizable = true,
							Utilised = true
						}
					}
				});

			var newPoolCode = new TrueMarkWaterIdentificationCode
			{
				Id = 999,
				Gtin = "04600000000001",
				SerialNumber = "newserial999",
				CheckCode = "newcheck999",
				RawCode = "010460000000000121newserial999\u001d93newcheck999"
			};

			_poolCodeProvider.TakeValidCodeAsync(
				Arg.Any<ReceiptTrueMarkCodesPool>(),
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>())
				.Returns(newPoolCode);

			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Is<IEnumerable<string>>(c => c.Contains(newPoolCode.FormatForCheck1260)),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 999999,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = newPoolCode.FormatForCheck1260,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
							ExpireDate = DateTime.Now.AddDays(30),
							Realizable = true,
							Utilised = true
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

			// Assert: старый и новый GUID различаются, отправка выполнена
			Assert.NotEqual(oldGuid, doc.DocumentGuid);
			await _cashboxClient.Received(1).SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenSendingDocument_IncrementsRoutineStateRetryCount()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			doc.FailureMessage = "MarkingCheckFlowViolated";
			var task = testData.Task;

			var problem = new EdoTaskProblem
			{
				SourceName = "Custom.ReceiptFiscalizationFailed",
				State = TaskProblemState.Active
			};
			var routineState = new EdoTaskProblemRoutineState
			{
				Problem = problem,
				RetryCount = 1,
				LastRetryTime = DateTime.Now.AddHours(-1)
			};
			problem.RoutineStates.Add(routineState);
			task.Problems.Add(problem);

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ошибка кассы"
					}
				});

			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 123456,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							Valid = true,
							Found = true,
							ErrorCode = 0,
							Verified = true,
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

			// Assert: RetryCount увеличен с 1 до 2 при отправке в кассу
			Assert.Equal(2, routineState.RetryCount);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenSendSucceeds_SetsSentStageAndQueuedStatus()
		{
			// Arrange
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;

			var activeProblem = new EdoTaskProblem
			{
				SourceName = "Custom.ReceiptFiscalizationFailed",
				State = TaskProblemState.Active
			};
			task.Problems.Add(activeProblem);
			task.Status = EdoTaskStatus.Problem;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ошибка кассы"
					}
				});

			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 123456,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							Valid = true,
							Found = true,
							ErrorCode = 0,
							Verified = true,
							Realizable = true,
							Utilised = true,
							IsBlocked = false,
							Sold = false
						}
					}
				});

			_cashboxClient.RequeueFiscalDocument(Arg.Any<string>(), Arg.Any<CancellationToken>())
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

			// Assert: документ переведен в Sent/Queued, ожидает завершения печати кассой
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(FiscalDocumentStage.Sent, doc.Stage);
			Assert.Equal(EdoFiscalStatus.Queued, doc.Status);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenNoActiveProblemExists_DoesNotCreateActiveProblemWhenSending()
		{
			// Arrange - нет активной проблемы на задаче
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;
			task.Problems.Clear();

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ошибка кассы"
					}
				});

			var cis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;
			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(new CodeCheckResponse
				{
					Code = 0,
					ReqId = Guid.NewGuid(),
					ReqTimestamp = 123456,
					Codes = new List<CodeCheckInfo>
					{
						new CodeCheckInfo
						{
							Cis = cis,
							ErrorCode = 0,
							Found = true,
							Valid = true,
							Verified = true,
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

			// Assert: при штатной переотправке ложная активная проблема в журнале операторов не создается
			var activeProblems = task.Problems.Where(p =>
				p.State == TaskProblemState.Active
				&& p.SourceName != null
				&& p.SourceName.IndexOf(nameof(ReceiptFiscalizationFailed), StringComparison.OrdinalIgnoreCase) >= 0)
				.ToList();

			Assert.Empty(activeProblems);
		}

		[Fact]
		public async Task ProcessFailedDocumentsAsync_WhenFirstPoolCodeFailsCdnCheck_RetriesAndSucceedsWithSecondCode()
		{
			// Arrange: начальный код не прошел проверку разрешительного режима
			var testData = CreateTestDocAndTask();
			var doc = testData.Doc;
			var task = testData.Task;

			_edoRepository.GetFailedFiscalDocumentIdsForReprocessing(
				Arg.Any<IUnitOfWork>(),
				Arg.Any<DateTime>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
				.Returns(new List<int> { _docId });

			_cashboxClient.CheckFiscalDocument(doc.DocumentGuid.ToString(), Arg.Any<CancellationToken>())
				.Returns(new FiscalizationResult
				{
					SendStatus = SendStatus.Success,
					FiscalDocumentInfo = new FiscalDocumentInfo
					{
						Status = ModulFiscalStatus.Failed,
						Message = "Ошибка кассы: ошибка 1260"
					}
				});

			var initialCis = doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode.FormatForCheck1260;

			var poolCode1 = new TrueMarkWaterIdentificationCode
			{
				Id = 881,
				Gtin = "04600000000001",
				SerialNumber = "serialFail1",
				CheckCode = "checkFail1",
				RawCode = "010460000000000121serialFail1\u001d93checkFail1"
			};

			var poolCode2 = new TrueMarkWaterIdentificationCode
			{
				Id = 882,
				Gtin = "04600000000001",
				SerialNumber = "serialOk2",
				CheckCode = "checkOk2",
				RawCode = "010460000000000121serialOk2\u001d93checkOk2"
			};

			_poolCodeProvider.TakeValidCodeAsync(
				Arg.Any<ReceiptTrueMarkCodesPool>(),
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>())
				.Returns(poolCode1, poolCode2);

			_tag1260Checker.CheckCodesForTag1260Async(
				Arg.Any<IEnumerable<string>>(),
				_orgCashboxToken,
				Arg.Any<CancellationToken>())
				.Returns(
					// 1. Проверка исходного кода документа - невалиден (Sold = true)
					callInfo => new CodeCheckResponse
					{
						Code = 0,
						ReqId = Guid.NewGuid(),
						ReqTimestamp = 111111,
						Codes = new List<CodeCheckInfo>
						{
							new CodeCheckInfo
							{
								Cis = initialCis,
								ErrorCode = 0,
								Found = true,
								Valid = true,
								Sold = true // Продан! Требуется замена
							}
						}
					},
					// 2. Проверка первого кода из пула (poolCode1) - тоже не прошел (Sold = true)
					callInfo => new CodeCheckResponse
					{
						Code = 0,
						ReqId = Guid.NewGuid(),
						ReqTimestamp = 222222,
						Codes = new List<CodeCheckInfo>
						{
							new CodeCheckInfo
							{
								Cis = poolCode1.FormatForCheck1260,
								ErrorCode = 0,
								Found = true,
								Valid = true,
								Sold = true // Не прошел в ЧЗ!
							}
						}
					},
					// 3. Проверка второго кода из пула (poolCode2) - успешно прошел
					callInfo => new CodeCheckResponse
					{
						Code = 0,
						ReqId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
						ReqTimestamp = 333333,
						Codes = new List<CodeCheckInfo>
						{
							new CodeCheckInfo
							{
								Cis = poolCode2.FormatForCheck1260,
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
					}
				);

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

			// Assert: успешно заменен со 2-й попытки на poolCode2
			Assert.Equal(1, result.TotalFound);
			Assert.Equal(1, result.SuccessCount);
			Assert.Equal(1, result.ReplacedCodesCount);
			Assert.Equal(0, result.ErrorCount);

			Assert.Equal(poolCode2, doc.InventPositions[0].EdoTaskItem.ProductCode.ResultCode);
			Assert.Equal(SourceProductCodeStatus.Changed, doc.InventPositions[0].EdoTaskItem.ProductCode.SourceCodeStatus);
			Assert.Equal("UUID=33333333-3333-3333-3333-333333333333&Time=333333", doc.InventPositions[0].IndustryRequisiteData);

			await _poolCodeProvider.Received(2).TakeValidCodeAsync(
				Arg.Any<ReceiptTrueMarkCodesPool>(),
				Arg.Any<GtinEntity>(),
				_orgInn,
				Arg.Any<CancellationToken>(),
				Arg.Any<DateTime?>());

			await _cashboxClient.Received(1).SendFiscalDocument(Arg.Any<FiscalDocument>(), Arg.Any<CancellationToken>());
		}
	}
}
