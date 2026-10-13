using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Статус документооборота и признак получения клиентом.
	/// Сначала берётся статус Такском. Если он ещё «В процессе» или «Получено оператором»,
	/// а в транзакциях уже есть более поздний шаг, статус заменяется на выведенный из транзакций.
	/// </summary>
	public static class TaxcomDocflowReceive
	{
		public const string ReceiveNoticeSendConfirmation = "ReceiveNoticeSendConfirmation";
		public const string ReceiveNotification = "ReceiveNotification";
		public const string UniversalMessageReceiveNotice = "UniversalMessageReceiveNotice";
		public const string CustomerInformation = "CustomerInformation";
		public const string PrimaryAccountingDocumentCustomer = "PrimaryAccountingDocumentCustomer";
		public const string CorrectionNotice = "CorrectionNotice";
		public const string UniversalMessageCorrectionNotice = "UniversalMessageCorrectionNotice";

		private static readonly string[] _buyerTitleCodes =
		{
			CustomerInformation,
			PrimaryAccountingDocumentCustomer,
			"CorPrimaryAccountingDocumentCustomer",
			"ExpInvoiceAndPrimaryAccountingDocumentCustomer",
			"CorExpInvoiceAndPrimaryAccountingDocumentCustomer",
			"FormalizedStatementCustomer",
			"FormalizedConsignmentCustomer",
			"FormalizedWorkResultCustomer",
			"FormalizedTradingCustomer"
		};

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

		private static readonly EdoDocFlowStatus[] _statusesThatCanBeCorrectedByTransaction =
		{
			EdoDocFlowStatus.InProgress,
			EdoDocFlowStatus.Sent,
			EdoDocFlowStatus.NotStarted,
			EdoDocFlowStatus.PreparingToSend,
			EdoDocFlowStatus.Unknown
		};

		public static EdoDocFlowStatus? ResolveStatus(EdoDocFlowStatus? reportedStatus, IEnumerable<string> transactionCodes)
		{
			var derivedStatus = DeriveStatusFromTransactions(transactionCodes);
			if(derivedStatus.HasValue && CanBeCorrectedByTransaction(reportedStatus))
			{
				return derivedStatus;
			}

			return reportedStatus;
		}

		public static bool IsReceived(EdoDocFlowStatus? status, IEnumerable<string> transactionCodes)
		{
			if(status.HasValue && _statusesThatMeanReceived.Contains(status.Value))
			{
				return true;
			}

			if(CanBeCorrectedByTransaction(status) && HasBuyerTitleOrClarification(transactionCodes))
			{
				return true;
			}

			if(!status.HasValue || _statusesThatCanStayStuck.Contains(status.Value))
			{
				return HasClientReceiveNotice(transactionCodes);
			}

			return false;
		}

		public static bool ShouldMarkAsSent(EdoDocFlowStatus? status, bool isReceived)
		{
			var statusCanStayStuck = !status.HasValue || _statusesThatCanStayStuck.Contains(status.Value);
			return statusCanStayStuck && isReceived;
		}

		private static EdoDocFlowStatus? DeriveStatusFromTransactions(IEnumerable<string> transactionCodes)
		{
			if(transactionCodes == null)
			{
				return null;
			}

			EdoDocFlowStatus? derivedStatus = null;
			foreach(var transactionCode in transactionCodes)
			{
				if(IsBuyerTitle(transactionCode))
				{
					derivedStatus = EdoDocFlowStatus.Succeed;
				}
				else if(IsClarification(transactionCode))
				{
					derivedStatus = EdoDocFlowStatus.Warning;
				}
			}

			return derivedStatus;
		}

		private static bool CanBeCorrectedByTransaction(EdoDocFlowStatus? status)
		{
			return !status.HasValue || _statusesThatCanBeCorrectedByTransaction.Contains(status.Value);
		}

		private static bool HasClientReceiveNotice(IEnumerable<string> transactionCodes)
		{
			return transactionCodes != null
				&& transactionCodes.Any(code =>
					code == ReceiveNoticeSendConfirmation
					|| code == ReceiveNotification
					|| (code != null && code.StartsWith(UniversalMessageReceiveNotice, StringComparison.Ordinal)));
		}

		private static bool HasBuyerTitleOrClarification(IEnumerable<string> transactionCodes)
		{
			return transactionCodes != null
				&& transactionCodes.Any(code => IsBuyerTitle(code) || IsClarification(code));
		}

		private static bool IsBuyerTitle(string transactionCode)
		{
			return transactionCode != null && _buyerTitleCodes.Contains(transactionCode);
		}

		private static bool IsClarification(string transactionCode)
		{
			return transactionCode == CorrectionNotice
				|| (transactionCode != null
					&& transactionCode.StartsWith(UniversalMessageCorrectionNotice, StringComparison.Ordinal));
		}
	}
}
