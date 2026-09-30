using Edo.Withdrawal.Routine.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Xunit;

namespace EdoServices.Tests
{
	public class TrueMarkDocumentsStatusUpdateServiceTests
	{
		private readonly IUnitOfWorkFactory _uowFactory = Substitute.For<IUnitOfWorkFactory>();
		private readonly IGenericRepository<TrueMarkDocument> _repository = Substitute.For<IGenericRepository<TrueMarkDocument>>();
		private readonly ITrueMarkApiClient _trueMarkApiClient = Substitute.For<ITrueMarkApiClient>();

		[Fact]
		public async Task UpdateTrueMarkDocuments_WhenCancellationIsAccepted_MarksRequestReadyToResend()
		{
			var (document, request, uow) = SetupCancellationDocument();
			_trueMarkApiClient.GetDocumentInfo(document.Guid.Value, document.Organization.INN, Arg.Any<CancellationToken>())
				.Returns(new TrueMarkDocumentInfo { Status = TrueMarkDocumentStatus.Ok });
			var service = CreateService();

			await service.UpdateTrueMarkDocuments(CancellationToken.None);

			Assert.True(document.IsSuccess);
			Assert.Null(document.ErrorMessage);
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.ReadyToResend, request.Status);
			await uow.Received().SaveAsync(request, cancellationToken: Arg.Any<CancellationToken>());
			await uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task UpdateTrueMarkDocuments_WhenCancellationIsRejected_MarksRequestFailed()
		{
			var (document, request, uow) = SetupCancellationDocument();
			_trueMarkApiClient.GetDocumentInfo(document.Guid.Value, document.Organization.INN, Arg.Any<CancellationToken>())
				.Returns(new TrueMarkDocumentInfo
				{
					Status = TrueMarkDocumentStatus.Error,
					ErrorMessage = "Документ отклонён"
				});
			var service = CreateService();

			await service.UpdateTrueMarkDocuments(CancellationToken.None);

			Assert.False(document.IsSuccess);
			Assert.Equal("Документ отклонён", document.ErrorMessage);
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.CancellationFailed, request.Status);
			Assert.Equal("Документ отклонён", request.ErrorMessage);
			await uow.Received().SaveAsync(request, cancellationToken: Arg.Any<CancellationToken>());
			await uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task UpdateTrueMarkDocuments_WhenCancellationIsPending_KeepsRequestWaitingForResult()
		{
			var (document, request, uow) = SetupCancellationDocument();
			_trueMarkApiClient.GetDocumentInfo(document.Guid.Value, document.Organization.INN, Arg.Any<CancellationToken>())
				.Returns(new TrueMarkDocumentInfo { Status = TrueMarkDocumentStatus.Pending });
			var service = CreateService();

			await service.UpdateTrueMarkDocuments(CancellationToken.None);

			Assert.False(document.IsSuccess);
			Assert.Null(document.ErrorMessage);
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.CancellationSent, request.Status);
			await uow.DidNotReceive().SaveAsync(request, cancellationToken: Arg.Any<CancellationToken>());
			await uow.Received().CommitAsync(Arg.Any<CancellationToken>());
		}

		private TrueMarkDocumentsStatusUpdateService CreateService()
		{
			return new TrueMarkDocumentsStatusUpdateService(
				Substitute.For<ILogger<TrueMarkDocumentsStatusUpdateService>>(),
				_uowFactory,
				_repository,
				_trueMarkApiClient);
		}

		private (TrueMarkDocument Document, EdoResendAfterTrueMarkCancellationRequest Request, IUnitOfWork Uow)
			SetupCancellationDocument()
		{
			var document = new TrueMarkDocument
			{
				Id = 10,
				Guid = Guid.NewGuid(),
				Organization = new OrganizationEntity { INN = "1234567890" },
				Type = TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation
			};
			var request = new EdoResendAfterTrueMarkCancellationRequest
			{
				CancellationDocument = document,
				Status = EdoResendAfterTrueMarkCancellationStatus.CancellationSent
			};
			var uow = Substitute.For<IUnitOfWork>();
			_uowFactory.CreateWithoutRoot(Arg.Any<string>()).ReturnsForAnyArgs(uow);
			uow.GetAll<EdoResendAfterTrueMarkCancellationRequest>()
				.Returns(new[] { request }.AsQueryable());
			_repository.GetAsync(
				uow,
				Arg.Any<Expression<Func<TrueMarkDocument, bool>>>(),
				Arg.Any<int>(),
				Arg.Any<CancellationToken>())
				.Returns(Result.Success<IEnumerable<TrueMarkDocument>>(new[] { document }));
			return (document, request, uow);
		}
	}
}
