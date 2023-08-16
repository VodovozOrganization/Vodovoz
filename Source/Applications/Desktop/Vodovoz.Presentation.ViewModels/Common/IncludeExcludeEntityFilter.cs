using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExcludeEntityFilter<TEntity> : IncludeExcludeFilter
		where TEntity : class, IDomainObject
	{
		public Expression<Func<TEntity, bool>> Specification { get; set; }

		public Action<IncludeExcludeEntityFilter<TEntity>> RefreshFunc { get; set; }

		private IEnumerable<IncludeExcludeElement<int?, TEntity>> IncludedEntityElements => IncludedElements.Cast<IncludeExcludeElement<int?, TEntity>>();

		private IEnumerable<IncludeExcludeElement<int?, TEntity>> ExcludedEntityElements => ExcludedElements.Cast<IncludeExcludeElement<int?, TEntity>>();

		public IEnumerable<int?> GetIncluded()
		{
			return IncludedEntityElements.Select(x => x.Id);
		}

		public IEnumerable<int?> GetExcluded()
		{
			return ExcludedEntityElements.Select(x => x.Id);
		}

		public override void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			RefreshFunc?.Invoke(this);

			AfterRefreshFilteredElements();
		}
	}
}
