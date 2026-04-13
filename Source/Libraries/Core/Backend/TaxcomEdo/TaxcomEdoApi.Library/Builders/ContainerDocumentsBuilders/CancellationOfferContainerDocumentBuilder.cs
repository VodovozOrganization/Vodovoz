using Core.Infrastructure;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Documents;

namespace TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders
{
	public class CancellationOfferContainerDocumentBuilder : ContainerDocumentBuilder
	{
		public static CancellationOfferContainerDocumentBuilder Create() => new CancellationOfferContainerDocumentBuilder();
		
		public new CancellationOfferContainerDocumentBuilder MainFile(CancellationOfferDocument document)
		{
			var fileData = new FileData
			{
				Name = $"{document.FileIdentifier}.xml",
				Image = document.WrapperXml.SerializeObject()
			};
			
			base.MainFile(fileData);
			return this;
		}
	}
}
