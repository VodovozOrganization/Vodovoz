using System;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrant;
using TaxcomEdoApi.Library.Builders;
using TaxcomEdoApi.Library.Factories;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Containers;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Documents;

namespace TaxcomEdoApi.Library.Services
{
	public class ContainerService
	{
		private readonly DocumentService _documentService;
		private readonly ISignProcessorFactory _signProcessorFactory;

		public ContainerService(
			DocumentService documentService,
			ISignProcessorFactory signProcessorFactory)
		{
			_documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
			_signProcessorFactory = signProcessorFactory ?? throw new ArgumentNullException(nameof(signProcessorFactory));
		}

		public byte[] ExportNewToZip(NewContainer newContainer)
		{
			var container = new Container();
			
			foreach(var document in newContainer.Documents)
			{
				var containerDocflow = new ContainerDocflow();
				var newContainerDocument = _documentService.CreateContainerDocument(document);
				var signatures = _signProcessorFactory.CreateSignProcessor(newContainer.SignMode)
					.Sign(newContainerDocument, document);
				
				if(signatures.Any() && newContainerDocument is ContainerDocument containerDocument)
				{
					containerDocument.MainFileSignatures = signatures;
					SetWarrantCardDocSigns(newContainerDocument, newContainer.ContainerWarrant);
				}
				
				if(newContainer.ContainerWarrant != null)
				{
					var parameter = new DescriptionAdditionalParameter
					{
						Name = WarrantConstants.DefaultXmlPath,
						Value = WarrantConstants.XmlName
					};
						
					newContainerDocument.Card.Description.AddAdditionalParameter(parameter);
				}

				containerDocflow.AddDocument(newContainerDocument);
				container.AddDocflow(containerDocflow);
			}

			if(newContainer.ContainerWarrant != null)
			{
				container.Warrant = ContainerWarrantBuilder
					.Create()
					.FromWarrant(newContainer.ContainerWarrant.Warrant)
					.Build();
			}

			return ArchiveService
				.Create(container)
				.Archive();
		}

		private static void SetWarrantCardDocSigns(
			IContainerDocument containerDocument,
			NewContainerWarrant newContainerWarrant)
		{
			if(newContainerWarrant is null
				|| !containerDocument.MainFileSignatures.Any())
			{
				return;
			}

			var firstCard = newContainerWarrant.Warrant.WarrantCards[0];
			firstCard.ToSign = containerDocument.MainFileSignatures
				.Select(signature => new WarrantCardDocSign { File = signature.Path })
				.ToArray();
		}
	}
}
