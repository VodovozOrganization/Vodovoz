using System;
using QS.Tdi;

namespace Vodovoz.Journal
{
	public interface INodeSelector : ITdiTab, IDisposable
	{
		event EventHandler<JournalSelectedNodesNodesEventArgs> OnEntitySelectedResult;
	}
}
