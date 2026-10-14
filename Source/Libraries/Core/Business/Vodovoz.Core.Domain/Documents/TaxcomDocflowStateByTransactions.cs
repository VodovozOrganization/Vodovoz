using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Статус документооборота и признак получения, выведенные из транзакций Такском.
	/// </summary>
	public sealed class TaxcomDocflowState
	{
		public TaxcomDocflowState(EdoDocFlowStatus? status, bool isReceived)
		{
			Status = status;
			IsReceived = isReceived;
		}

		public EdoDocFlowStatus? Status { get; }

		public bool IsReceived { get; }
	}

	/// <summary>
	/// Сначала берётся статус Такском. Если он ещё «В процессе» или «Получено оператором»,
	/// а в транзакциях уже есть титул покупателя или уточнение, статус заменяется.
	/// Предложение об аннулировании и его принятие заменяют статус, даже если Такском ещё присылает прежний.
	/// </summary>
	public static class TaxcomDocflowStateByTransactions
	{
		public const string ReceiveNoticeSendConfirmation = "ReceiveNoticeSendConfirmation";
		public const string ReceiveNotification = "ReceiveNotification";
		public const string UniversalMessageReceiveNotice = "UniversalMessageReceiveNotice";
		public const string CustomerInformation = "CustomerInformation";
		public const string PrimaryAccountingDocumentCustomer = "PrimaryAccountingDocumentCustomer";
		public const string CorrectionNotice = "CorrectionNotice";
		public const string UniversalMessageCorrectionNotice = "UniversalMessageCorrectionNotice";
		public const string CancellationOffer = "CancellationOffer";
		public const string CancellationOfferResign = "CancellationOfferResign";
		public const string AcceptCancellationOffer = "AcceptCancellationOffer";
		public const string RejectCancellationOffer = "RejectCancellationOffer";
		public const string UniversalMessageRejectionNoticeCancellationOffer = "UniversalMessageRejectionNoticeCancellationOffer";

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

		public static TaxcomDocflowState Resolve<TDocument>(
			EdoDocFlowStatus? reportedStatus,
			IEnumerable<TDocument> documents,
			Func<TDocument, string> transactionCode,
			Func<TDocument, string> reglamentCode)
		{
			var codes = ReadCodes(documents, transactionCode, reglamentCode);
			var status = ResolveStatus(reportedStatus, codes);
			status = ApplyCancellation(status, DeriveCancellation(codes));

			return new TaxcomDocflowState(status, IsReceived(status, codes));
		}

		public static bool ShouldMarkAsSent(EdoDocFlowStatus? status, bool isReceived)
		{
			var statusCanStayStuck = !status.HasValue || _statusesThatCanStayStuck.Contains(status.Value);
			return statusCanStayStuck && isReceived;
		}

		private static EdoDocFlowStatus? ResolveStatus(EdoDocFlowStatus? reportedStatus, IReadOnlyList<string> codes)
		{
			var derivedStatus = DeriveStatusFromTransactions(codes);
			if(derivedStatus.HasValue && CanBeCorrectedByTransaction(reportedStatus))
			{
				return derivedStatus;
			}

			return reportedStatus;
		}

		private static EdoDocFlowStatus? ApplyCancellation(
			EdoDocFlowStatus? status,
			EdoDocFlowStatus? cancellationStatus)
		{
			if(!cancellationStatus.HasValue || status == EdoDocFlowStatus.Cancelled)
			{
				return status;
			}

			return cancellationStatus;
		}

		private static bool IsReceived(EdoDocFlowStatus? status, IReadOnlyList<string> codes)
		{
			if(status.HasValue && _statusesThatMeanReceived.Contains(status.Value))
			{
				return true;
			}

			if(CanBeCorrectedByTransaction(status) && HasBuyerTitleOrClarification(codes))
			{
				return true;
			}

			if(!status.HasValue || _statusesThatCanStayStuck.Contains(status.Value))
			{
				return HasClientReceiveNotice(codes);
			}

			return false;
		}

		private static List<string> ReadCodes<TDocument>(
			IEnumerable<TDocument> documents,
			Func<TDocument, string> transactionCode,
			Func<TDocument, string> reglamentCode)
		{
			var codes = new List<string>();
			if(documents == null || transactionCode == null || reglamentCode == null)
			{
				return codes;
			}

			foreach(var document in documents)
			{
				AddCode(codes, transactionCode(document));
				AddCode(codes, reglamentCode(document));
			}

			return codes;
		}

		private static void AddCode(List<string> codes, string code)
		{
			if(!string.IsNullOrWhiteSpace(code))
			{
				codes.Add(code);
			}
		}

		private static EdoDocFlowStatus? DeriveStatusFromTransactions(IReadOnlyList<string> codes)
		{
			EdoDocFlowStatus? derivedStatus = null;
			foreach(var code in codes)
			{
				if(IsBuyerTitle(code))
				{
					derivedStatus = EdoDocFlowStatus.Succeed;
				}
				else if(IsClarification(code))
				{
					derivedStatus = EdoDocFlowStatus.Warning;
				}
			}

			return derivedStatus;
		}

		private static EdoDocFlowStatus? DeriveCancellation(IReadOnlyList<string> codes)
		{
			EdoDocFlowStatus? cancellationStatus = null;
			foreach(var code in codes)
			{
				if(IsCancellationRejected(code))
				{
					cancellationStatus = null;
				}
				else if(IsCancellationAccepted(code))
				{
					cancellationStatus = EdoDocFlowStatus.Cancelled;
				}
				else if(IsCancellationOffer(code))
				{
					cancellationStatus = EdoDocFlowStatus.WaitingForCancellation;
				}
			}

			return cancellationStatus;
		}

		private static bool CanBeCorrectedByTransaction(EdoDocFlowStatus? status)
		{
			return !status.HasValue || _statusesThatCanBeCorrectedByTransaction.Contains(status.Value);
		}

		private static bool HasClientReceiveNotice(IReadOnlyList<string> codes)
		{
			return codes.Any(code =>
				code == ReceiveNoticeSendConfirmation
				|| code == ReceiveNotification
				|| code.StartsWith(UniversalMessageReceiveNotice, StringComparison.Ordinal));
		}

		private static bool HasBuyerTitleOrClarification(IReadOnlyList<string> codes)
		{
			return codes.Any(code => IsBuyerTitle(code) || IsClarification(code));
		}

		private static bool IsBuyerTitle(string code)
		{
			return _buyerTitleCodes.Contains(code);
		}

		private static bool IsClarification(string code)
		{
			return code == CorrectionNotice
				|| code.StartsWith(UniversalMessageCorrectionNotice, StringComparison.Ordinal);
		}

		private static bool IsCancellationOffer(string code)
		{
			return code == CancellationOffer;
		}

		private static bool IsCancellationAccepted(string code)
		{
			return code == CancellationOfferResign || code == AcceptCancellationOffer;
		}

		private static bool IsCancellationRejected(string code)
		{
			return code == RejectCancellationOffer
				|| code.StartsWith(UniversalMessageRejectionNoticeCancellationOffer, StringComparison.Ordinal);
		}
	}
}
