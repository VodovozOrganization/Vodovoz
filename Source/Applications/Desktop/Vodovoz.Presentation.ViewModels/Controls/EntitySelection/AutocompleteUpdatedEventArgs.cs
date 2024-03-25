using System;
using System.Collections;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class AutocompleteUpdatedEventArgs : EventArgs
	{
		public AutocompleteUpdatedEventArgs(IList autocompleteItems)
		{
			AutocompleteItems = autocompleteItems ?? throw new ArgumentNullException(nameof(autocompleteItems));
		}

		public IList AutocompleteItems { get; set; }
	}
}
