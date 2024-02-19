using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExcludeBoolParamsFilter : IncludeExcludeFilter
	{
		public Action<IncludeExcludeBoolParamsFilter> RefreshFunc { get; set; }

		private IEnumerable<IncludeExcludeElement<string, string>> IncludedEntityElements => IncludedElements.Cast<IncludeExcludeElement<string, string>>();

		private IEnumerable<IncludeExcludeElement<string, string>> ExcludedEntityElements => ExcludedElements.Cast<IncludeExcludeElement<string, string>>();

		public IEnumerable<string> GetIncluded()
		{
			return IncludedEntityElements.Select(x => x.Id);
		}

		public IEnumerable<string> GetExcluded()
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
