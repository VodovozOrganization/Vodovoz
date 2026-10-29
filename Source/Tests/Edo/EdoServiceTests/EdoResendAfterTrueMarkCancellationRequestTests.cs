using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Xunit;

namespace EdoServices.Tests
{
	public class EdoResendAfterTrueMarkCancellationRequestTests
	{
		[Fact]
		public void RetryCancellation_WhenRequestFailed_ClearsErrorAndCancellationDocument()
		{
			var request = new EdoResendAfterTrueMarkCancellationRequest
			{
				Status = EdoResendAfterTrueMarkCancellationStatus.CancellationFailed,
				CancellationDocument = new TrueMarkDocument(),
				ErrorMessage = "Ошибка"
			};

			request.RetryCancellation();

			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation, request.Status);
			Assert.Null(request.CancellationDocument);
			Assert.Null(request.ErrorMessage);
		}

		[Fact]
		public void RetryCancellation_WhenRequestHasNoError_ThrowsException()
		{
			var request = new EdoResendAfterTrueMarkCancellationRequest
			{
				Status = EdoResendAfterTrueMarkCancellationStatus.CancellationSent
			};

			Assert.Throws<InvalidOperationException>(() => request.RetryCancellation());
		}

		[Fact]
		public void SuccessfulCancellationAndPublish_ChangesRequestToCompleted()
		{
			var request = new EdoResendAfterTrueMarkCancellationRequest
			{
				Status = EdoResendAfterTrueMarkCancellationStatus.CancellationSent
			};

			request.MarkReadyToResend();
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.ReadyToResend, request.Status);

			request.MarkCompleted();
			Assert.Equal(EdoResendAfterTrueMarkCancellationStatus.Completed, request.Status);
		}
	}
}
