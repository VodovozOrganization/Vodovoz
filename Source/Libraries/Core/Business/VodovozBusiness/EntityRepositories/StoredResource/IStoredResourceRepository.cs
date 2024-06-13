using System.Collections.Generic;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.EntityRepositories.StoredResourceRepository
{
	public interface IStoredResourceRepository
	{
		IList<StoredResource> GetAllSignatures();
	}
}
