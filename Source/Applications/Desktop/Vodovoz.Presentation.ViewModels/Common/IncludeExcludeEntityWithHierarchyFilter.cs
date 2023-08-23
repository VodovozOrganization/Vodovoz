using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExcludeEntityWithHierarchyFilter<TEntity> : IncludeExcludeFilter
		where TEntity : class, IDomainObject
	{
		public Expression<Func<TEntity, bool>> Specification { get; set; }

		public Action<IncludeExcludeEntityWithHierarchyFilter<TEntity>, TEntity> RefreshFunc { get; set; }

		private IEnumerable<IncludeExcludeElement<int?, TEntity>> IncludedHirerarchicalElements => IncludedElements.Cast<IncludeExcludeElement<int?, TEntity>>();

		private IEnumerable<IncludeExcludeElement<int?, TEntity>> ExcludedHirerarchicalElements => ExcludedElements.Cast<IncludeExcludeElement<int?, TEntity>>();

		public IEnumerable<int?> GetIncluded()
		{
			return IncludedHirerarchicalElements.Select(x => x.Id);
		}

		public IEnumerable<int?> GetExcluded()
		{
			return ExcludedHirerarchicalElements.Select(x => x.Id);
		}

		public override void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			RefreshFunc?.Invoke(this, null);

			AfterRefreshFilteredElements();
		}
	}
}
