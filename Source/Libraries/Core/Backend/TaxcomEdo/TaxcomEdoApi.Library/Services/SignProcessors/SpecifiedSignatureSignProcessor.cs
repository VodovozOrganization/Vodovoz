using System;
using System.Collections.Generic;
using System.Linq;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Interfaces;
using TaxcomEdoApi.Library.Providers;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.SignProcessors
{
	/// <inheritdoc/>
	public class SpecifiedSignatureSignProcessor : ISignProcessor
	{
		private readonly ISignFilenameProvider _signFileNameProvider;

		public SpecifiedSignatureSignProcessor(ISignFilenameProvider signFilenameProvider)
		{
			_signFileNameProvider = signFilenameProvider ?? throw new ArgumentNullException(nameof(signFilenameProvider));
		}

		/// <inheritdoc/>
		public IList<IFileData> Sign(IContainerDocument containerDocument, IDocument document)
		{
			if(!document.Signatures.Any())
			{
				throw new InvalidOperationException("Отсутствует подпись документа. См раздел документации \"Подпись документов в контейнере\"");
			}

			var signatures = new List<IFileData>();
			
			if(containerDocument.MainFile != null && document.Image != null)
			{
				var imageDocument = new FileData
				{
					Encoding = containerDocument.MainFile.Encoding,
					Name = containerDocument.MainFile.Name,
					Image = document.Image
				};
				
				containerDocument.MainFile = imageDocument;
			}
			
			foreach (var signature in document.Signatures)
			{
				var data = new FileData
				{
					Name = _signFileNameProvider.GetSignFilename(),
					Image = signature
				};
				signatures.Add(data);
			}
			
			return signatures;
		}
	}
}
