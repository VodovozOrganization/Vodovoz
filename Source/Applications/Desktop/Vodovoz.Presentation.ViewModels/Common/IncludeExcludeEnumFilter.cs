using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class IncludeExcludeEnumFilter<TEnum> : IncludeExcludeFilter
		where TEnum : Enum
	{
		public List<Enum> HideElements { get; } = new List<Enum>();

		public Action<IncludeExcludeEnumFilter<TEnum>> RefreshFunc { get; set; }

		public override void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			RefreshFunc?.Invoke(this);

			AfterRefreshFilteredElements();
		}
	}
}
