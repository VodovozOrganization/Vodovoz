using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public interface ICashboxClient : IDisposable
	{
		int CashboxId { get; }

		Task<bool> CanFiscalizeAsync(CancellationToken cancellationToken);

		Task<FiscalizationResult> SendFiscalDocument(FiscalDocument doc, CancellationToken cancellationToken);

		Task<FiscalizationResult> CheckFiscalDocument(FiscalDocument doc, CancellationToken cancellationToken);
		Task<FiscalizationResult> CheckFiscalDocument(string fiscalDocumentId, CancellationToken cancellationToken);
		Task<FiscalizationResult> RequeueFiscalDocument(string fiscalDocumentId, CancellationToken cancellationToken);
	}
}
