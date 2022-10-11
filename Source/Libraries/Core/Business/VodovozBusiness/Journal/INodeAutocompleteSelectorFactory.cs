using System;
namespace Vodovoz.Journal
{
	public interface INodeAutocompleteSelectorFactory : INodeSelectorFactory
	{
		INodeAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false);
	}
}
