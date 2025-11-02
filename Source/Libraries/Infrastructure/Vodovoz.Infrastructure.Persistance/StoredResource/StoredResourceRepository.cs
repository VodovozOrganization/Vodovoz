using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.StoredResources;
using Vodovoz.EntityRepositories.StoredResourceRepository;
using VodovozStoredResource = Vodovoz.Core.Domain.StoredResources.StoredResource;

namespace Vodovoz.Infrastructure.Persistance.StoredResource
{
	internal sealed class StoredResourceRepository : IStoredResourceRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public StoredResourceRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public IList<VodovozStoredResource> GetAllSignatures()
		{
			IList<VodovozStoredResource> result;
			using(var uow = _uowFactory.CreateWithoutRoot($"Получение подписей"))
			{
				result = uow.Session.QueryOver<VodovozStoredResource>()
				   .Where(sr => sr.ImageType == ImageType.Signature)
				   .List<VodovozStoredResource>();
			}
			return result;
		}
	}
}
