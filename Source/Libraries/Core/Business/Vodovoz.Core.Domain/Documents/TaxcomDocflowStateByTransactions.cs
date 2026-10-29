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
	/// «Не начат» или «Получено оператором». Исключение: титул покупателя с КодИтога = 2
	/// даёт «Завершен с расхождением», с КодИтога = 3 — «Не принят», пока нет аннулирования.
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

		/// <summary>
		/// Не код транзакции Такском. Вызывающий код добавляет его, если в титуле покупателя КодИтога = 2.
		/// </summary>
		public const string BuyerTitleAcceptedWithDivergences = "BuyerTitleAcceptedWithDivergences";

		/// <summary>
		/// Не код транзакции Такском. Вызывающий код добавляет его, если в титуле покупателя КодИтога = 3.
		/// </summary>
		public const string BuyerTitleNotAccepted = "BuyerTitleNotAccepted";

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

		public static bool ContainsBuyerTitle(IEnumerable<string> transactionCodes)
		{
			if(transactionCodes is null)
			{
				return false;
			}

			return transactionCodes.Any(code => !string.IsNullOrWhiteSpace(code) && _buyerTitleCodes.Contains(code));
		}

		public static IEnumerable<string> WithBuyerTitleResult(IEnumerable<string> transactionCodes, string resultCode)
		{
			var codes = transactionCodes ?? Enumerable.Empty<string>();
			if(resultCode == "3")
			{
				return codes.Concat(new[] { BuyerTitleNotAccepted });
			}

			if(resultCode == "2")
			{
				return codes.Concat(new[] { BuyerTitleAcceptedWithDivergences });
			}

			return codes;
		}

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
			if(CanBeCorrectedByTransaction(reportedStatus))
			{
				reportedStatus = DeriveStatusFromTransactions(codes) ?? reportedStatus;
			}

			if(!BuyerTitleResultCanReplaceStatus(reportedStatus, codes))
			{
				return reportedStatus;
			}

			if(BuyerTitleHasDivergences(codes))
			{
				reportedStatus = EdoDocFlowStatus.CompletedWithDivergences;
			}

			if(BuyerTitleIsNotAccepted(codes))
			{
				return EdoDocFlowStatus.NotAccepted;
			}

			return reportedStatus;
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

			if(codes.Any(IsBuyerTitle))
			{
				derivedStatus = EdoDocFlowStatus.Succeed;
			}

			if(BuyerTitleHasDivergences(codes))
			{
				derivedStatus = EdoDocFlowStatus.CompletedWithDivergences;
			}

			if(BuyerTitleIsNotAccepted(codes))
			{
				derivedStatus = EdoDocFlowStatus.NotAccepted;
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

		private static bool BuyerTitleHasDivergences(IReadOnlyList<string> codes)
		{
			return codes.Any(code => code == BuyerTitleAcceptedWithDivergences);
		}

		private static bool BuyerTitleIsNotAccepted(IReadOnlyList<string> codes)
		{
			return codes.Any(code => code == BuyerTitleNotAccepted);
		}

		private static bool BuyerTitleResultCanReplaceStatus(EdoDocFlowStatus? status, IReadOnlyList<string> codes)
		{
			return status != EdoDocFlowStatus.WaitingForCancellation
				&& status != EdoDocFlowStatus.Cancelled
				&& !codes.Any(IsCancellationOffer)
				&& !codes.Any(IsCancellationAccepted);
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
