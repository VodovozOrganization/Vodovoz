using System;
using Microsoft.Extensions.DependencyInjection;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Models.Documents.CustomerInvoice;
using TaxcomEdoApi.Library.Models.Documents.UniversalInvoice;
using TaxcomEdoApi.Library.Models.Interfaces;
using TaxcomEdoApi.Library.Services.ContainerDocumentsServices;

namespace TaxcomEdoApi.Library.Services
{
	public class DocumentService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public DocumentService(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}
		
		public IContainerDocument CreateContainerDocument(IDocument document)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			
			var containerDocument = document.Type switch
			{
				DocumentType.ExpInvoiceAndPrimaryAccountingDocumentVendor =>
					scope
						.ServiceProvider
						.GetRequiredService<UniversalInvoiceContainerDocumentService>()
						.CreateContainerDocument(document as UniversalInvoiceDocument),
				DocumentType.ExpInvoiceAndPrimaryAccountingDocumentCustomer =>
					scope
						.ServiceProvider
						.GetRequiredService<CustomerUniversalInvoiceContainerDocumentService>()
						.CreateContainerDocument(document as CustomerUniversalInvoiceDocument),
				DocumentType.CancellationOffer =>
					scope
						.ServiceProvider
						.GetRequiredService<CancellationOfferContainerDocumentService>()
						.CreateContainerDocument(document as CancellationOfferDocument),
				DocumentType.CancellationOfferResign =>
					scope
						.ServiceProvider
						.GetRequiredService<CancellationOfferResignContainerDocumentService>()
						.CreateContainerDocument(document as CancellationOfferResign),
				_ => scope
					.ServiceProvider
					.GetRequiredService<NonformalizedContainerDocumentService>()
					.CreateContainerDocument(document as NonformalizedDocument)
			};
				
			return containerDocument;
		}
	}
}
