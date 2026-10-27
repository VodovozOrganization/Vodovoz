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
	/// Статус Такском заменяется по транзакциям, только если он ещё «В процессе»,
	/// «Не начат» или «Получено оператором». Титул покупателя сам по себе не делает
	/// документ успешно завершённым: «Успешно завершен» и «Не принят» приходят от Такском.
	/// Порядок транзакций не учитывается: важнее наличие более высокого шага.
	/// Конечный шаг — принятое аннулирование.
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
			"FormalizedTradingCustomer",
			"FormalizedKC2Customer"
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
			EdoDocFlowStatus.NotStarted
		};

		public static TaxcomDocflowState Resolve(
			EdoDocFlowStatus? reportedStatus,
			IEnumerable<string> transactionCodes)
		{
			var codes = Normalize(transactionCodes);
			var status = ResolveStatus(reportedStatus, codes);

			return new TaxcomDocflowState(status, IsReceived(status, codes));
		}

		public static bool ShouldMarkAsSent(EdoDocFlowStatus? status, bool isReceived)
		{
			var statusCanStayStuck = !status.HasValue || _statusesThatCanStayStuck.Contains(status.Value);
			return statusCanStayStuck && isReceived;
		}

		private static EdoDocFlowStatus? ResolveStatus(EdoDocFlowStatus? reportedStatus, IReadOnlyList<string> codes)
		{
			if(!CanBeCorrectedByTransaction(reportedStatus))
			{
				return reportedStatus;
			}

			return DeriveStatusFromTransactions(codes) ?? reportedStatus;
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

		private static List<string> Normalize(IEnumerable<string> transactionCodes)
		{
			var codes = new List<string>();
			if(transactionCodes == null)
			{
				return codes;
			}

			foreach(var transactionCode in transactionCodes)
			{
				AddCode(codes, transactionCode);
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

			if(codes.Any(IsClarification))
			{
				derivedStatus = EdoDocFlowStatus.Warning;
			}

			if(codes.Any(IsCancellationOffer) && !codes.Any(IsCancellationRejected))
			{
				derivedStatus = EdoDocFlowStatus.WaitingForCancellation;
			}

			if(codes.Any(IsCancellationAccepted))
			{
				derivedStatus = EdoDocFlowStatus.Cancelled;
			}

			return derivedStatus;
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
