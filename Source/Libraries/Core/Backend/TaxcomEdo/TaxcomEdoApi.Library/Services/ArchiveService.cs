using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Core.Infrastructure;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Builders;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Containers;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	public class ArchiveService
	{
		private readonly Container _container;

		protected ArchiveService(Container container)
		{
			_container = container ?? throw new ArgumentNullException(nameof(container));
		}
		
		public byte[] Archive()
		{
			using var zipStream = new MemoryStream();
			using(var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
			{
				var containerDescriptionDocFlows = new List<ContainerDescriptionDocFlow>();

				var metaBuilder = MetaBuilder.Create()
					.IsLast(_container.IsLast)
					.RequestDateTime(DateTime.Now)
					.LastRecordDateTime(_container.LastRecordDateTime);

				foreach(var docflow in _container.Docflows)
				{
					containerDescriptionDocFlows.Add(docflow.ToWrapperXml(_container.ExportCardsAsExternalFiles));
					ExportDocflow(zipArchive, docflow);
				}

				metaBuilder.Docflows(containerDescriptionDocFlows.ToArray());

				var meta = metaBuilder.Build();
				Export(zipArchive, MetaConstants.XmlName, meta.SerializeObject());

				ExportWarrantData(zipArchive, _container.Warrant);
			}

			return zipStream.ToArray();
		}
		

		protected virtual void ExportDocflow(ZipArchive zipArchive, IContainerDocflow docflow)
		{
			foreach(var document in docflow.Documents)
			{
				if(document.MainFile != null)
				{
					Export(zipArchive, document.MainFile.Path, document.MainFile.Image);
				}

				if(document.MainFileSignatures != null)
				{
					foreach(var mainImageSignature in document.MainFileSignatures)
					{
						Export(zipArchive, mainImageSignature.Path, mainImageSignature.Image);
					}
				}

				if(document.Attachment != null)
				{
					Export(zipArchive, document.Attachment.Path, document.Attachment.Image);
				}

				if(document.AttachmentSignatures != null)
				{
					foreach(var dataImageSignature in document.AttachmentSignatures)
					{
						Export(zipArchive, dataImageSignature.Path, dataImageSignature.Image);
					}
				}

				if(_container.ExportCardsAsExternalFiles)
				{
					var cardFile = document.CreateFileDataFromCard();
					Export(zipArchive, cardFile.Path, cardFile.Image);
				}
			}
		}

		private void ExportWarrantData(
			ZipArchive zipArchive,
			IContainerWarrant warrant)
		{
			Export(zipArchive, WarrantConstants.XmlName, warrant.RawWarrantImage);

			foreach(var warrantCard in warrant.WarrantCards)
			{
				if(warrantCard.WarrantImage != null)
				{
					Export(zipArchive, warrantCard.WarrantImage.Path, warrantCard.WarrantImage.Image);
				}

				if(warrantCard.WarrantSignatures == null)
				{
					continue;
				}

				foreach(var warrantSignature in warrantCard.WarrantSignatures)
				{
					Export(zipArchive, warrantSignature.Path, warrantSignature.Image);
				}
			}
		}

		private static void Export(ZipArchive zipArchive, string filePath, byte[] content)
		{
			var dataEntry = zipArchive.CreateEntry(filePath);
					
			using var streamWriter = new BinaryWriter(dataEntry.Open());
			streamWriter.Write(content);
		}

		public static ArchiveService Create(Container container) => new ArchiveService(container);
	}
}
