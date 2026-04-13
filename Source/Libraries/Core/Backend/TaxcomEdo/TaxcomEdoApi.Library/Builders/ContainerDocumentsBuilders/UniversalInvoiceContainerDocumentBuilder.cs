using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Documents.UniversalInvoice;

namespace TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders
{
	public class UniversalInvoiceContainerDocumentBuilder : ContainerDocumentBuilder
	{
		public new UniversalInvoiceContainerDocumentBuilder MainFile(UniversalInvoiceDocument document)
		{
			ContainerDocument.DirectoryInsideArchive = UniversalInvoiceDocument.DefaultDirectoryInsideArchive;
			var fileName = $"{document.FileIdentifier}.xml";
			
			var fileData = new FileData
			{
				Name = fileName,
				Path = ContainerDocument.GetFilePath(fileName),
				Image = document.DocumentToByteArray()
			};
			
			base.MainFile(fileData);
			return this;
		}
		
		public new UniversalInvoiceContainerDocumentBuilder Attachment(UniversalInvoiceDocument document)
		{
			var fileData = document.AttachmentFile;
			
			base.Attachment(fileData);
			return this;
		}

		public static UniversalInvoiceContainerDocumentBuilder Create() =>
			new UniversalInvoiceContainerDocumentBuilder();
	}
}
