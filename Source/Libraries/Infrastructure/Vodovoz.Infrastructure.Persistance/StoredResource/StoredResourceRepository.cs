using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.StoredResources;
using Vodovoz.EntityRepositories.StoredResourceRepository;

namespace Vodovoz.Infrastructure.Persistance.StoredResource
{
	public class StoredResourceRepository : IStoredResourceRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public StoredResourceRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public IList<StoredResource> GetAllSignatures()
		{
			IList<StoredResource> result;
			using(var uow = _uowFactory.CreateWithoutRoot($"Получение подписей"))
			{
				result = uow.Session.QueryOver<StoredResource>()
				   .Where(sr => sr.ImageType == ImageType.Signature)
				   .List<StoredResource>();
			}
			return result;
		}
	}
}
