using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public interface IFiscalChangeDetector
	{
		FiscalChangeSet DetectChanges(FiscalOrderSnapshot previous, FiscalOrderSnapshot current, bool isFullCancellation);
	}
}
