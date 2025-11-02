using System;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsPreparer.Factories
{
	public interface IFileDataFactory
	{
		BillFileData CreateBillFileData(string billId, DateTime billDate, byte[] data);
	}
}
