using QS.DomainModel.Entity;
using System;
using System.Linq.Expressions;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExcludeEntityFilter<TEntity> : IncludeExcludeFilter
		where TEntity : class, IDomainObject
	{
		public Expression<Func<TEntity, bool>> Specification { get; set; }

		public Action<IncludeExcludeEntityFilter<TEntity>> RefreshFunc { get; set; }
		
		public override void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			RefreshFunc?.Invoke(this);

			AfterRefreshFilteredElements();
		}
	}
}
