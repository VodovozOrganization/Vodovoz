using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Docflow;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using Notifications.Infrastructure;
using NSubstitute;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Xunit;

namespace EdoServices.Tests
{
	public class DocflowStatusUpdateHandlerTests
	{
		private const int _documentId = 204;
		private const int _taskId = 123;
		private const int _orderId = 30;

		private readonly IUnitOfWork _uow;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly INotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly DocflowStatusUpdateHandler _handler;

		public DocflowStatusUpdateHandlerTests()
		{
			_uow = Substitute.For<IUnitOfWork>();
			_notificationMessageFactory = Substitute.For<IEdoNotificationMessageFactory>();
			_notificationPublisher = Substitute.For<INotificationPublisher<EdoNotificationMessage>>();
			_publishEndpoint = Substitute.For<IPublishEndpoint>();

			var edoRequest = new PrimaryEdoRequest
			{
				Order = new OrderEntity { Id = _orderId }
			};
			var requestQuery = Substitute.For<IQueryOver<FormalEdoRequest, FormalEdoRequest>>();
			_uow.Session.QueryOver<FormalEdoRequest>().Returns(requestQuery);
			requestQuery.Where(Arg.Any<Expression<Func<FormalEdoRequest, bool>>>()).Returns(requestQuery);
			requestQuery.SingleOrDefaultAsync(Arg.Any<CancellationToken>()).Returns(edoRequest);

			_handler = new DocflowStatusUpdateHandler(
				Substitute.For<ILogger<DocflowStatusUpdateHandler>>(),
				_uow,
				_notificationMessageFactory,
				_notificationPublisher,
				_publishEndpoint);
		}

		[Fact]
		public async Task OurCancellation_WhenWaitingForCancellationReceived_DoesNotCreateDiscrepancy()
		{
			var document = SetupDocument(EdoDocumentStatus.Succeed, EdoDiscrepancyState.None, cancellationRequested: true);

			await HandleStatus(EdoDocFlowStatus.WaitingForCancellation);

			Assert.Equal(EdoDiscrepancyState.None, document.DiscrepancyState);
			Assert.Equal(EdoDocumentStatus.WaitingForCancellation, document.Status);
			await AssertNotificationNotPublished();
		}

		[Fact]
		public async Task OurCancellation_WhenCancelledReceived_CancelsDocumentWithoutDiscrepancy()
		{
			var document = SetupDocument(EdoDocumentStatus.WaitingForCancellation, EdoDiscrepancyState.None, cancellationRequested: true);

			await HandleStatus(EdoDocFlowStatus.Cancelled);

			Assert.Equal(EdoDiscrepancyState.None, document.DiscrepancyState);
			Assert.Equal(EdoDocumentStatus.Cancelled, document.Status);
			await _publishEndpoint.Received(1).Publish(Arg.Any<OrderDocumentCancelledEvent>(), Arg.Any<CancellationToken>());
			await AssertNotificationNotPublished();
		}

		[Fact]
		public async Task OurCancellation_WhenCancelledReceivedWithoutWaitingForCancellation_CancelsDocumentWithoutDiscrepancy()
		{
			var document = SetupDocument(EdoDocumentStatus.Succeed, EdoDiscrepancyState.None, cancellationRequested: true);

			await HandleStatus(EdoDocFlowStatus.Cancelled);

			Assert.Equal(EdoDiscrepancyState.None, document.DiscrepancyState);
			Assert.Equal(EdoDocumentStatus.Cancelled, document.Status);
			await AssertNotificationNotPublished();
		}

		[Theory]
		[InlineData(EdoDocFlowStatus.Warning, EdoDocumentStatus.Warning)]
		[InlineData(EdoDocFlowStatus.CompletedWithDivergences, EdoDocumentStatus.CompletedWithDivergences)]
		[InlineData(EdoDocFlowStatus.NotAccepted, EdoDocumentStatus.NotAccepted)]
		[InlineData(EdoDocFlowStatus.WaitingForCancellation, EdoDocumentStatus.WaitingForCancellation)]
		[InlineData(EdoDocFlowStatus.Cancelled, EdoDocumentStatus.Cancelled)]
		public async Task Discrepancy_WhenDetected_ActivatesDiscrepancyAndNotifiesOnce(
			EdoDocFlowStatus incomeStatus,
			EdoDocumentStatus expectedDocumentStatus)
		{
			var document = SetupDocument(EdoDocumentStatus.Sent, EdoDiscrepancyState.None);

			await HandleStatus(incomeStatus);

			Assert.Equal(EdoDiscrepancyState.Active, document.DiscrepancyState);
			Assert.Equal(expectedDocumentStatus, document.Status);
			await _notificationPublisher.Received(1)
				.TryPublishAsync(_uow, Arg.Any<EdoNotificationMessage>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Discrepancy_WhenSameStatusReceivedAgain_DoesNotNotifyAgain()
		{
			var document = SetupDocument(EdoDocumentStatus.Sent, EdoDiscrepancyState.None);

			await HandleStatus(EdoDocFlowStatus.CompletedWithDivergences);
			await HandleStatus(EdoDocFlowStatus.CompletedWithDivergences);

			Assert.Equal(EdoDiscrepancyState.Active, document.DiscrepancyState);
			await _notificationPublisher.Received(1)
				.TryPublishAsync(_uow, Arg.Any<EdoNotificationMessage>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Discrepancy_WhenTypeChanges_UpdatesStatusWithoutSecondNotification()
		{
			var document = SetupDocument(EdoDocumentStatus.Warning, EdoDiscrepancyState.Active);

			await HandleStatus(EdoDocFlowStatus.WaitingForCancellation);

			Assert.Equal(EdoDiscrepancyState.Active, document.DiscrepancyState);
			Assert.Equal(EdoDocumentStatus.WaitingForCancellation, document.Status);
			await AssertNotificationNotPublished();
		}

		[Fact]
		public async Task ActiveDiscrepancy_WhenSucceedReceived_IgnoresStatus()
		{
			var document = SetupDocument(EdoDocumentStatus.CompletedWithDivergences, EdoDiscrepancyState.Active);

			await HandleStatus(EdoDocFlowStatus.Succeed);

			Assert.Equal(EdoDiscrepancyState.Active, document.DiscrepancyState);
			Assert.Equal(EdoDocumentStatus.CompletedWithDivergences, document.Status);
			Assert.Null(document.AcceptTime);
			await _publishEndpoint.DidNotReceive().Publish(Arg.Any<OrderDocumentAcceptedEvent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ResolvedDiscrepancy_WhenCancelledReceived_CancelsDocumentWithoutEventsAndDiscrepancy()
		{
			var document = SetupDocument(EdoDocumentStatus.WaitingForCancellation, EdoDiscrepancyState.Resolved, cancellationRequested: true);

			await HandleStatus(EdoDocFlowStatus.Cancelled);

			Assert.Equal(EdoDiscrepancyState.Resolved, document.DiscrepancyState);
			Assert.Equal(EdoDocumentStatus.Cancelled, document.Status);
			Assert.Empty(_publishEndpoint.ReceivedCalls());
			await AssertNotificationNotPublished();
		}

		[Theory]
		[InlineData(EdoDocFlowStatus.Succeed)]
		[InlineData(EdoDocFlowStatus.Warning)]
		[InlineData(EdoDocFlowStatus.CompletedWithDivergences)]
		[InlineData(EdoDocFlowStatus.NotAccepted)]
		[InlineData(EdoDocFlowStatus.WaitingForCancellation)]
		public async Task ResolvedDiscrepancy_WhenNewStatusReceived_KeepsDocumentUnchanged(EdoDocFlowStatus incomeStatus)
		{
			var document = SetupDocument(EdoDocumentStatus.CompletedWithDivergences, EdoDiscrepancyState.Resolved);

			await HandleStatus(incomeStatus);

			Assert.Equal(EdoDiscrepancyState.Resolved, document.DiscrepancyState);
			Assert.Equal(EdoDocumentStatus.CompletedWithDivergences, document.Status);
			Assert.Empty(_publishEndpoint.ReceivedCalls());
			await AssertNotificationNotPublished();
		}

		private OrderEdoDocument SetupDocument(
			EdoDocumentStatus status,
			EdoDiscrepancyState discrepancyState,
			bool cancellationRequested = false)
		{
			var document = new OrderEdoDocument
			{
				Id = _documentId,
				Type = OutgoingEdoDocumentType.Order,
				DocumentTaskId = _taskId,
				Status = status,
				DiscrepancyState = discrepancyState,
				CancellationRequestTime = cancellationRequested ? DateTime.Now : (DateTime?)null
			};

			_uow.Session.GetAsync<OutgoingEdoDocument>(_documentId, Arg.Any<CancellationToken>())
				.Returns(document);

			return document;
		}

		private Task HandleStatus(EdoDocFlowStatus status)
		{
			return _handler.HandleDocflowUpdatedEvent(
				new EdoDocflowUpdatedEvent
				{
					EdoDocumentId = _documentId,
					DocFlowStatus = status.ToString()
				},
				CancellationToken.None);
		}

		private async Task AssertNotificationNotPublished()
		{
			await _notificationPublisher.DidNotReceiveWithAnyArgs()
				.TryPublishAsync(default, default, default);
		}
	}
}
