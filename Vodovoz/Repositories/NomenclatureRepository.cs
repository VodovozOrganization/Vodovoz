using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Criterion;

namespace Vodovoz.Repository
{
	public static class NomenclatureRepository
	{
		public static Nomenclature GetDefaultBottle (IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature> (32);
		}

		public static QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery ()
		{
			return QueryOver.Of<Nomenclature> ()
				.Where (n => n.Category.IsIn (Nomenclature.GetCategoriesForProductMaterial ()));
		}
	}
}

