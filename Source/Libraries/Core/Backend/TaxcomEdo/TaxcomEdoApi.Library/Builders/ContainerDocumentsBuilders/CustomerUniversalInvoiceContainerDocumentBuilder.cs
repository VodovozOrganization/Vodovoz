using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Documents.CustomerInvoice;

namespace TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders
{
	public class CustomerUniversalInvoiceContainerDocumentBuilder : ContainerDocumentBuilder
	{
		public new CustomerUniversalInvoiceContainerDocumentBuilder MainFile(CustomerUniversalInvoiceDocument document)
		{
			var fileData = new FileData
			{
				Name = $"{document.FileIdentifier}.xml",
				Image = document.DocumentToByteArray()
			};
			
			base.MainFile(fileData);
			return this;
		}
		
		public static CustomerUniversalInvoiceContainerDocumentBuilder Create() => new CustomerUniversalInvoiceContainerDocumentBuilder();
	}
}
