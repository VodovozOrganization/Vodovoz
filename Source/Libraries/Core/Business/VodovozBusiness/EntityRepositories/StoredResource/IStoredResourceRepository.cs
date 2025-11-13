using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.StoredResources;

namespace Vodovoz.EntityRepositories.StoredResourceRepository
{
	public interface IStoredResourceRepository
	{
		IList<StoredResource> GetAllSignatures();
	}
}
