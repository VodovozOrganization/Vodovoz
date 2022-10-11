using System;
using System.Collections;

namespace Vodovoz.Journal
{
	public interface INodeAutocompleteSelector : INodeSelector
	{
		IList Items { get; }
		void SearchValues(params string[] values);
		event EventHandler ListUpdated;
	}
}
