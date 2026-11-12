using NHibernate.Linq.Functions;
using NHibernate.Spatial.Linq.Functions;
using Vodovoz.Data.NHibernate.NhibernateExtensions.HqlGenerators;

namespace Vodovoz.Data.NHibernate.NhibernateExtensions
{
	public class LinqToHqlGeneratorsRegistry : SpatialLinqToHqlGeneratorsRegistry
	{
		public LinqToHqlGeneratorsRegistry() : base()
		{
			this.Merge(new CustomOrderItemActualSumGenerator());
			this.Merge(new CustomOrderDepositItemActualSumGenerator());
			this.Merge(new CustomAddDaysMethodGenerator());
			this.Merge(new CustomOrderTemplateCardSortingPriorityGenerator());
		}
	}
}
