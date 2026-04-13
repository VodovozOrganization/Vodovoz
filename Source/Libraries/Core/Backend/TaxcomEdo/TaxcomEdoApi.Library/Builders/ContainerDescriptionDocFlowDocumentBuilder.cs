using System;
using System.Collections.Generic;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Builders
{
	public class ContainerDescriptionDocFlowDocumentBuilder
	{
		private ContainerDescriptionDocFlowDocument _docflowDocument = new ContainerDescriptionDocFlowDocument();

		public ContainerDescriptionDocFlowDocumentBuilder ReglamentCode(string reglamentCode)
		{
			if(!string.IsNullOrWhiteSpace(reglamentCode))
			{
				_docflowDocument.ReglamentCode = reglamentCode;
			}

			return this;
		}
		
		public ContainerDescriptionDocFlowDocumentBuilder TransactionCode(string transactionCode)
		{
			if(!string.IsNullOrWhiteSpace(transactionCode))
			{
				_docflowDocument.TransactionCode = transactionCode;
			}

			return this;
		}
		
		public ContainerDescriptionDocFlowDocumentBuilder MainFile(IFileData mainImage)
		{
			if(mainImage != null)
			{
				InitializeFilesIfNull();
				_docflowDocument.Files.MainImage = CreateFileData(mainImage);
			}

			return this;
		}
		
		public ContainerDescriptionDocFlowDocumentBuilder MainFileSignatures(IEnumerable<IFileData> mainImageSignatures)
		{
			if(mainImageSignatures != null && mainImageSignatures.Any())
			{
				InitializeFilesIfNull();
				_docflowDocument.Files.MainImageSignature = mainImageSignatures.Select(CreateFileData).ToArray();
			}

			return this;
		}
		
		public ContainerDescriptionDocFlowDocumentBuilder Attachment(IFileData dataImage)
		{
			if(dataImage != null)
			{
				InitializeFilesIfNull();
				_docflowDocument.Files.DataImage = CreateFileData(dataImage);
			}

			return this;
		}
		
		public ContainerDescriptionDocFlowDocumentBuilder AttachmentSignatures(IEnumerable<IFileData> dataImageSignatures)
		{
			if(dataImageSignatures != null && dataImageSignatures.Any())
			{
				InitializeFilesIfNull();
				_docflowDocument.Files.DataImageSignature = dataImageSignatures.Select(CreateFileData).ToArray();
			}

			return this;
		}
		
		public ContainerDescriptionDocFlowDocumentBuilder Card(string filePath, Card card, bool exportCardAsExternalFile)
		{
			if(card != null && exportCardAsExternalFile)
			{
				InitializeFilesIfNull();
				_docflowDocument.Files.ExternalCard = FileInfo.Create(filePath);
			}

			return this;
		}

		private static FileInfo CreateFileData(IFileData mainImage)
		{
			return new FileInfo
			{
				Name = mainImage.Name,
				Path = mainImage.Path,
			};
		}

		private void InitializeFilesIfNull()
		{
			_docflowDocument.Files ??= new ContainerDescriptionDocFlowDocumentFiles();
		}
		
		public ContainerDescriptionDocFlowDocument Build()
		{
			var docflowDocument = _docflowDocument;
			_docflowDocument = new ContainerDescriptionDocFlowDocument();
			
			return docflowDocument;
		}

		public static ContainerDescriptionDocFlowDocumentBuilder Create() => new ContainerDescriptionDocFlowDocumentBuilder();
	}
}
