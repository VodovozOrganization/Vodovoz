using System.Collections.Generic;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Interfaces;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services.SignProcessors
{
	/// <inheritdoc/>
	public class NullSignProcessor : ISignProcessor
	{
		/// <inheritdoc/>
		public IList<IFileData> Sign(IContainerDocument containerDocument, IDocument document)
		{
			return new List<IFileData>();
		}
	}
}
