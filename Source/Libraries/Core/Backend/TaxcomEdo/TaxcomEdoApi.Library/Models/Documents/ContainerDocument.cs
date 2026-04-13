using System;
using System.Collections.Generic;
using System.IO;
using Core.Infrastructure;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Builders;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents
{
	public class ContainerDocument : IContainerDocument
	{
		public Card Card { get; set; }
		
		public string DirectoryInsideArchive { get; set; }
		
		public Guid? InternalId
		{
			get
			{
				Guid result;
				return Card != null && Card.Identifiers != null && Guid.TryParse(Card.Identifiers.InternalId, out result)
					? new Guid?(result)
					: new Guid?();
			}
		}

		public string ExternalIdentifier =>
			Card?.Identifiers == null
				? string.Empty
				: Card.Identifiers.ExternalIdentifier;

		public Guid? InternalDocumentGroupId
		{
			get
			{
				Guid result;
				return Card != null && Card.Identifiers != null && Guid.TryParse(Card.Identifiers.InternalDocumentGroupId, out result)
					? new Guid?(result)
					: new Guid?();
			}
		}

		public string ExternalDocumentGroupIdentifier =>
			Card?.Identifiers == null
				? string.Empty
				: Card.Identifiers.ExternalDocumentGroupIdentifier;

		public IFileData MainFile { get; set; }
		public IList<IFileData> MainFileSignatures { get; set; } = new List<IFileData>();
		public IFileData Attachment { get; set; }
		public IList<IFileData> AttachmentSignatures { get; set; } = new List<IFileData>();

		public string GetFilePath(string filename)
		{
			return string.IsNullOrWhiteSpace(DirectoryInsideArchive)
				? filename
				: Path.Combine(DirectoryInsideArchive, filename);
		}

		public string TransactionCode { get; set; }

		public string ReglamentCode { get; set; }

		public Guid? AdditionalDocId { get; set; }

		public IList<IContainerWarrantCard> WarrantCards { get; set; }

		public DateTime? DateTime { get; set; }

		public ContainerDescriptionDocFlowDocument ToWrapperXml(bool exportCardAsExternalFile)
		{
			var builder =
				ContainerDescriptionDocFlowDocumentBuilder
					.Create()
					.ReglamentCode(ReglamentCode)
					.TransactionCode(TransactionCode)
					.MainFile(MainFile)
					.MainFileSignatures(MainFileSignatures)
					.Attachment(Attachment)
					.AttachmentSignatures(AttachmentSignatures)
					.Card(GetFilePath(Card.FileName), Card, exportCardAsExternalFile)
					;

			return builder.Build();
		}

		public IFileData CreateFileDataFromCard()
		{
			if(Card is null)
			{
				throw new InvalidOperationException("Card.xml не заполнен!");
			}

			var cardBytes = Card.SerializeObject(XmlExtensions.Win1251Encoding);
			var fileName = Card.FileName;
			
			return FileData.Create(fileName, GetFilePath(fileName), cardBytes, XmlExtensions.Win1251Encoding);
		}
	}
}
