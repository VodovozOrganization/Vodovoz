﻿using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.EntityRepositories.StoredResourceRepository
{
	public class StoredResourceRepository : IStoredResourceRepository
	{
		public IList<StoredResource> GetAllSignatures()
		{
			IList<StoredResource> result;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"Получение подписей"))
			{
				result = uow.Session.QueryOver<StoredResource>()
				   .Where(sr => sr.ImageType == ImageType.Signature)
				   .List<StoredResource>();
			}
			return result;
		}
	}
}
