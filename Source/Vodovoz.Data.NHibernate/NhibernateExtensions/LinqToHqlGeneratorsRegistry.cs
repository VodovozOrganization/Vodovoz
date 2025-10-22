using NHibernate.Linq.Functions;
using NHibernate.Spatial.Linq.Functions;

namespace Vodovoz.Data.NHibernate.NhibernateExtensions
{
	public class LinqToHqlGeneratorsRegistry : SpatialLinqToHqlGeneratorsRegistry
	{
		public LinqToHqlGeneratorsRegistry() : base()
		{
			this.Merge(new CustomOrderItemActualSumGenerator());
			this.Merge(new CustomAddDaysMethodGenerator());
		}
	}
}
