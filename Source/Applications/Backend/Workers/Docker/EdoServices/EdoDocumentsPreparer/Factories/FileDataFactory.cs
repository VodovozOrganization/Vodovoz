using System;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsPreparer.Factories
{
	public class FileDataFactory : IFileDataFactory
	{
		public BillFileData CreateBillFileData(string billId, DateTime billDate, byte[] data) =>
			new BillFileData
			{
				BillNumber = billId,
				BillDate = billDate,
				Image = data
			};
	}
}
