using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Признак получения документа клиентом.
	/// Сначала учитывается статус документооборота, затем транзакция извещения о получении:
	/// старое подтверждение отправки извещения или универсальное сообщение.
	/// </summary>
	public static class TaxcomDocflowReceipt
	{
		public const string ReceiveNoticeSendConfirmation = "ReceiveNoticeSendConfirmation";
		public const string UniversalMessageReceiveNotice = "UniversalMessageReceiveNotice";

		private static readonly EdoDocFlowStatus[] _statusesThatMeanReceived =
		{
			EdoDocFlowStatus.Succeed,
			EdoDocFlowStatus.Warning,
			EdoDocFlowStatus.CompletedWithDivergences,
			EdoDocFlowStatus.NotAccepted,
			EdoDocFlowStatus.WaitingForCancellation,
			EdoDocFlowStatus.Cancelled
		};

		private static readonly EdoDocFlowStatus[] _statusesThatCanStayStuck =
		{
			EdoDocFlowStatus.InProgress,
			EdoDocFlowStatus.NotStarted,
			EdoDocFlowStatus.PreparingToSend,
			EdoDocFlowStatus.Unknown
		};

		public static bool IsReceived(EdoDocFlowStatus? status, IEnumerable<string> transactionCodes)
		{
			if(status.HasValue && _statusesThatMeanReceived.Contains(status.Value))
			{
				return true;
			}

			if(!status.HasValue || _statusesThatCanStayStuck.Contains(status.Value))
			{
				return HasClientReceiptTransaction(transactionCodes);
			}

			return false;
		}

		public static bool ShouldMarkAsSent(EdoDocFlowStatus? status, bool isReceived)
		{
			var statusCanStayStuck = !status.HasValue || _statusesThatCanStayStuck.Contains(status.Value);
			return statusCanStayStuck && isReceived;
		}

		public static bool HasClientReceiptTransaction(IEnumerable<string> transactionCodes)
		{
			return transactionCodes != null
				&& transactionCodes.Any(code =>
					code == ReceiveNoticeSendConfirmation
					|| code == UniversalMessageReceiveNotice);
		}
	}
}
