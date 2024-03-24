using System;
using System.Collections;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class AutocompleteUpdatedEventArgs : EventArgs
	{
		public IList List;

		public AutocompleteUpdatedEventArgs(IList list)
		{
			List = list ?? throw new ArgumentNullException(nameof(list));
		}
	}
}
