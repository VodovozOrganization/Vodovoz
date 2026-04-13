using System;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Documents;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Builders.ContainerDocumentsBuilders
{
	public abstract class ContainerDocumentBuilder
	{
		protected ContainerDocument ContainerDocument = new ContainerDocument();
		
		public ContainerDocumentBuilder Card(Card card)
		{
			ContainerDocument.Card = card;
			return this;
		}
		
		public virtual ContainerDocumentBuilder MainFile(IFileData fileData)
		{
			if(fileData is null)
			{
				throw new InvalidOperationException($"{nameof(fileData)} should not be null.");
			}
			
			ContainerDocument.MainFile = fileData;
			
			return this;
		}
		
		public virtual ContainerDocumentBuilder Attachment(IFileData fileData)
		{
			ContainerDocument.Attachment = fileData;
			return this;
		}
		
		public ContainerDocumentBuilder TransactionCode(string transactionCode)
		{
			ContainerDocument.TransactionCode = transactionCode;
			return this;
		}

		public ContainerDocument Build()
		{
			var containerDocument = ContainerDocument;
			ContainerDocument = new ContainerDocument();
			return containerDocument;
		}
	}
}
