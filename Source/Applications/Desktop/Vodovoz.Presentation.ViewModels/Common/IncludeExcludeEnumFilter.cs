using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class IncludeExcludeEnumFilter<TEnum> : IncludeExcludeFilter
		where TEnum : Enum
	{
		public List<Enum> HideElements { get; } = new List<Enum>();

		public Action<IncludeExcludeEnumFilter<TEnum>> RefreshFunc { get; set; }

		private IEnumerable<IncludeExcludeElement<TEnum, TEnum>> IncludedEnumeElements => IncludedElements.Cast<IncludeExcludeElement<TEnum, TEnum>>();

		private IEnumerable<IncludeExcludeElement<TEnum, TEnum>> ExcludedEnumeElements => ExcludedElements.Cast<IncludeExcludeElement<TEnum, TEnum>>();

		public IEnumerable<TEnum> GetIncluded()
		{
			return IncludedEnumeElements.Select(x => x.Id);
		}

		public IEnumerable<TEnum> GetExcluded()
		{
			return ExcludedEnumeElements.Select(x => x.Id);
		}

		public override void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			RefreshFunc?.Invoke(this);

			AfterRefreshFilteredElements();
		}
	}
}
