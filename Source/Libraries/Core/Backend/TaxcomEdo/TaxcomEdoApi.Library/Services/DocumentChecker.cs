using System;
using System.IO;
using System.Xml;
using Core.Infrastructure;
using Edo.Contracts.Xml.FormalizedDocuments;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	public class DocumentChecker : IDocumentChecker
	{
		private const string _rootName = "Файл";
		private const string _formatAttribute = "ВерсФорм";
		
		public Format? RecognizeVersion(byte[] data)
		{
			using var input = new MemoryStream(data);
			using var xmlTextReader = new XmlTextReader(input);
			
			if(xmlTextReader.ReadToFollowing(_rootName))
			{
				var attribute = xmlTextReader.GetAttribute(_formatAttribute);
				var version = attribute.TryParseAsEnum<Format>();

				return version switch
				{
					Format.Format5_03 => version,
					_ => throw new ArgumentException("Формат не поддерживается " + attribute)
				};
			}
			
			return null;
		}
	}
}
