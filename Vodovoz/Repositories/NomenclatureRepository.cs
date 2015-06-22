using System;
using QSOrmProject;
using Vodovoz.Domain;

namespace Vodovoz.Repository
{
	public static class NomenclatureRepository
	{
		public static Nomenclature GetDefaultBottle(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature> (32);
		}
	}
}

