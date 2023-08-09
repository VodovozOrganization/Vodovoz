using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExcludeEntityWithHierarchyFilter<TEntity> : IncludeExcludeFilter
	where TEntity : class, IDomainObject
	{
		public Func<TEntity, bool> SpecificationFunc { get; set; }

		public Action<IncludeExcludeEntityWithHierarchyFilter<TEntity>, TEntity> RefreshFunc { get; set; }

		public override void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			RefreshFunc?.Invoke(this, null);

			AfterRefreshFilteredElements();
		}
	}
}
