using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Containers.Interfaces
{
	public interface IContainerDocument
	{
		Card Card { get; set; }
		Guid? InternalId { get; }
		string ExternalIdentifier { get; }
		Guid? InternalDocumentGroupId { get; }
		string ExternalDocumentGroupIdentifier { get; }
		IFileData MainFile { get; set; }
		IList<IFileData> MainFileSignatures { get; }
		IFileData Attachment { get; set; }
		IList<IFileData> AttachmentSignatures { get; }
		string DirectoryInsideArchive { get; set; }
		string GetFilePath(string filename);
		string TransactionCode { get; set; }
		string ReglamentCode { get; set; }
		ContainerDescriptionDocFlowDocument ToWrapperXml(bool exportCardAsExternalFile);
		IFileData CreateFileDataFromCard();
		Guid? AdditionalDocId { get; set; }
		IList<IContainerWarrantCard> WarrantCards { get; set; }
	}
}
