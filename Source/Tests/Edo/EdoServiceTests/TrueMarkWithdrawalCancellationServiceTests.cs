using TrueMark.Contracts;
using Edo.Transport;
using Edo.Withdrawal.Routine.Services;
using Microsoft.Extensions.Logging;
using NHibernate;
using NSubstitute;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.TrueMark;
using Xunit;

namespace EdoServices.Tests
{
	public class TrueMarkWithdrawalCancellationServiceTests
	{
		private readonly IUnitOfWorkFactory _uowFactory = Substitute.For<IUnitOfWorkFactory>();
		private readonly ITrueMarkApiClient _trueMarkApiClient = Substitute.For<ITrueMarkApiClient>();
		private readonly IEdoRequestCreatedEventPublisher _publisher = Substitute.For<IEdoRequestCreatedEventPublisher>();

		[Fact]
		public async Task SendCancellationDocuments_WhenRequestIsWaiting_CreatesCancellationDocument()
		{
			var withdrawalDocumentGuid = Guid.NewGuid();
			var cancellationDocumentGuid = Guid.NewGuid();
			var organization = new OrganizationEntity { INN = "1234567890" };
			var withdrawalTask = new WithdrawalEdoTask { Id = 15 };
			var request = CreateRequest(
				EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation,
				withdrawalDocumentGuid,
				organization,
				withdrawalTask);
			var uow = SetupUnitOfWork(request);
			_trueMarkApiClient.SendIndividualAccountingWithdrawalCancellationDocument(
				withdrawalDocumentGuid,
				organization.INN,
				Arg.Any<TrueMarkProductGroup>(),
				Arg.Any<CancellationToken>())
				.Returns(cancellationDocumentGuid.ToString());
			var service = CreateService();

			await service.SendCancellationDocuments(CancellationToken.None);

			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.CancellationSent, request.Status);
			Assert.Equal(1, request.CancellationAttemptsCount);
			Assert.Equal(cancellationDocumentGuid, request.CancellationDocument.Guid);
			Assert.Equal(TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation, request.CancellationDocument.Type);
			Assert.Same(request.Order, request.CancellationDocument.Order);
			Assert.Same(organization, request.CancellationDocument.Organization);
			Assert.Same(withdrawalTask, request.CancellationDocument.WithdrawalEdoTask);
			uow.Received().OpenTransaction();
			await uow.Session.Received().GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				request.Id,
				LockMode.Upgrade,
				Arg.Any<CancellationToken>());
			await uow.Received().SaveAsync(request.CancellationDocument, cancellationToken: Arg.Any<CancellationToken>());
			await uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task SendCancellationDocuments_WhenTrueMarkReturnsError_MarksRequestAsFailed()
		{
			var withdrawalDocumentGuid = Guid.NewGuid();
			var organization = new OrganizationEntity { INN = "1234567890" };
			var request = CreateRequest(
				EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation,
				withdrawalDocumentGuid,
				organization,
				new WithdrawalEdoTask { Id = 15 });
			var uow = SetupUnitOfWork(request);
			_trueMarkApiClient.SendIndividualAccountingWithdrawalCancellationDocument(
				withdrawalDocumentGuid,
				organization.INN,
				Arg.Any<TrueMarkProductGroup>(),
				Arg.Any<CancellationToken>())
				.Returns<Task<string>>(_ => throw new InvalidOperationException("Ошибка ЧЗ"));
			var service = CreateService();

			await service.SendCancellationDocuments(CancellationToken.None);

			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.CancellationFailed, request.Status);
			Assert.Equal(1, request.CancellationAttemptsCount);
			Assert.Equal("Ошибка ЧЗ", request.ErrorMessage);
			Assert.Null(request.CancellationDocument);
			await uow.Received().SaveAsync(request, cancellationToken: Arg.Any<CancellationToken>());
			await uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task PublishReadyResendRequests_WhenRequestIsReady_PublishesAndCompletesRequest()
		{
			var request = CreateRequest(
				EdoResendAfterTrueMarkCancellationStatus.ReadyToResend,
				Guid.NewGuid(),
				new OrganizationEntity { INN = "1234567890" },
				new WithdrawalEdoTask { Id = 15 });
			var uow = SetupUnitOfWork(request);
			var service = CreateService();

			await service.PublishReadyResendRequests(CancellationToken.None);

			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.Completed, request.Status);
			uow.Received().OpenTransaction();
			await uow.Session.Received().GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				request.Id,
				LockMode.Upgrade,
				Arg.Any<CancellationToken>());
			await _publisher.Received(1).Publish(
				uow,
				request.ResendEdoRequest.Id,
				Arg.Any<string>(),
				Arg.Any<CancellationToken>());
			await uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task SendCancellationDocuments_WhenProductGroupIsPetFood_SendsPetFoodGroupAndSetsProductGroup()
		{
			var withdrawalDocumentGuid = Guid.NewGuid();
			var cancellationDocumentGuid = Guid.NewGuid();
			var organization = new OrganizationEntity { INN = "1234567890" };
			var withdrawalTask = new WithdrawalEdoTask { Id = 15 };
			var request = CreateRequest(
				EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation,
				withdrawalDocumentGuid,
				organization,
				withdrawalTask);
			request.WithdrawalDocument.ProductGroup = TrueMarkProductGroup.PetFood;

			var uow = SetupUnitOfWork(request);
			_trueMarkApiClient.SendIndividualAccountingWithdrawalCancellationDocument(
				withdrawalDocumentGuid,
				organization.INN,
				TrueMarkProductGroup.PetFood,
				Arg.Any<CancellationToken>())
				.Returns(cancellationDocumentGuid.ToString());
			var service = CreateService();

			await service.SendCancellationDocuments(CancellationToken.None);

			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.CancellationSent, request.Status);
			Assert.NotNull(request.CancellationDocument);
			Assert.Equal(TrueMarkProductGroup.PetFood, request.CancellationDocument.ProductGroup);
			await _trueMarkApiClient.Received(1).SendIndividualAccountingWithdrawalCancellationDocument(
				withdrawalDocumentGuid,
				organization.INN,
				TrueMarkProductGroup.PetFood,
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task PublishReadyResendRequests_WhenOneOfTwoRequestsIsNotReady_DoesNotPublish()
		{
			var resendEdoRequestId = 50;
			var request1 = new EdoResendAfterTrueMarkCancellationRequest
			{
				Id = 1,
				ResendEdoRequest = new ManualEdoRequest { Id = resendEdoRequestId },
				Status = EdoResendAfterTrueMarkCancellationStatus.ReadyToResend
			};
			var request2 = new EdoResendAfterTrueMarkCancellationRequest
			{
				Id = 2,
				ResendEdoRequest = new ManualEdoRequest { Id = resendEdoRequestId },
				Status = EdoResendAfterTrueMarkCancellationStatus.CancellationSent
			};

			var uow = Substitute.For<IUnitOfWork>();
			_uowFactory.CreateWithoutRoot(Arg.Any<string>()).ReturnsForAnyArgs(uow);
			uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
				.Returns(new[] { request1, request2 }.AsQueryable());
			uow.Session.GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				1, LockMode.Upgrade, Arg.Any<CancellationToken>())
				.Returns(request1);
			uow.Session.GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				2, LockMode.Upgrade, Arg.Any<CancellationToken>())
				.Returns(request2);

			var service = CreateService();

			await service.PublishReadyResendRequests(CancellationToken.None);

			await _publisher.DidNotReceiveWithAnyArgs().Publish(default, default, default, default);
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.ReadyToResend, request1.Status);
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.CancellationSent, request2.Status);
		}

		[Fact]
		public async Task PublishReadyResendRequests_WhenBothRequestsAreReady_PublishesOnceAndCompletesBoth()
		{
			var resendEdoRequestId = 50;
			var request1 = new EdoResendAfterTrueMarkCancellationRequest
			{
				Id = 1,
				ResendEdoRequest = new ManualEdoRequest { Id = resendEdoRequestId },
				Status = EdoResendAfterTrueMarkCancellationStatus.ReadyToResend
			};
			var request2 = new EdoResendAfterTrueMarkCancellationRequest
			{
				Id = 2,
				ResendEdoRequest = new ManualEdoRequest { Id = resendEdoRequestId },
				Status = EdoResendAfterTrueMarkCancellationStatus.ReadyToResend
			};

			var uow = Substitute.For<IUnitOfWork>();
			_uowFactory.CreateWithoutRoot(Arg.Any<string>()).ReturnsForAnyArgs(uow);
			uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
				.Returns(new[] { request1, request2 }.AsQueryable());
			uow.Session.GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				1, LockMode.Upgrade, Arg.Any<CancellationToken>())
				.Returns(request1);
			uow.Session.GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				2, LockMode.Upgrade, Arg.Any<CancellationToken>())
				.Returns(request2);

			var service = CreateService();

			await service.PublishReadyResendRequests(CancellationToken.None);

			await _publisher.Received(1).Publish(
				uow,
				resendEdoRequestId,
				Arg.Any<string>(),
				Arg.Any<CancellationToken>());
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.Completed, request1.Status);
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.Completed, request2.Status);
			await uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		private TrueMarkWithdrawalCancellationService CreateService()
		{
			return new TrueMarkWithdrawalCancellationService(
				Substitute.For<ILogger<TrueMarkWithdrawalCancellationService>>(),
				_uowFactory,
				_trueMarkApiClient,
				_publisher);
		}

		private IUnitOfWork SetupUnitOfWork(EdoResendAfterTrueMarkCancellationRequest request)
		{
			var uow = Substitute.For<IUnitOfWork>();
			_uowFactory.CreateWithoutRoot(Arg.Any<string>()).ReturnsForAnyArgs(uow);
			uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
				.Returns(new[] { request }.AsQueryable());
			uow.Session.GetAsync<EdoResendAfterTrueMarkCancellationRequest>(
				request.Id,
				LockMode.Upgrade,
				Arg.Any<CancellationToken>())
				.Returns(request);
			return uow;
		}

		private static EdoResendAfterTrueMarkCancellationRequest CreateRequest(
			EdoResendAfterTrueMarkCancellationStatus status,
			Guid withdrawalDocumentGuid,
			OrganizationEntity organization,
			WithdrawalEdoTask withdrawalTask)
		{
			return new EdoResendAfterTrueMarkCancellationRequest
			{
				Id = 10,
				Order = new OrderEntity { Id = 20 },
				ResendEdoRequest = new ManualEdoRequest { Id = 30 },
				WithdrawalDocument = new TrueMarkDocument
				{
					Guid = withdrawalDocumentGuid,
					Organization = organization,
					WithdrawalEdoTask = withdrawalTask
				},
				Status = status
			};
		}
	}
}
