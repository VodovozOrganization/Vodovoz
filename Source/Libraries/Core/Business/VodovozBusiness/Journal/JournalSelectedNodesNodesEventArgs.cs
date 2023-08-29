using System;
using QS.Project.Journal;

namespace Vodovoz.Journal
{
	public class JournalSelectedNodesNodesEventArgs : EventArgs
	{
		public IJournalNode[] SelectedNodes { get; }

		public JournalSelectedNodesNodesEventArgs(IJournalNode[] selectedNodes)
		{
			SelectedNodes = selectedNodes;
		}
	}
}
