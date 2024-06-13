using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.StoredResources;
using Vodovoz.EntityRepositories.StoredResourceRepository;

namespace Vodovoz.Infrastructure.Persistance.StoredResources
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
