using System;
namespace Vodovoz.Journal
{
	public interface INodeSelectorFactory
	{
		Type EntityType { get; }
		INodeSelector CreateSelector(bool multipleSelect = false);
	}
}
