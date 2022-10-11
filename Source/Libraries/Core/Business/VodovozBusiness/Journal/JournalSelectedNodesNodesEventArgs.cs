using System;
using QS.Project.Journal;

namespace Vodovoz.Journal
{
	public class JournalSelectedNodesNodesEventArgs : EventArgs
	{
		public JournalNodeBase[] SelectedNodes { get; }

		public JournalSelectedNodesNodesEventArgs(JournalNodeBase[] selectedNodes)
		{
			SelectedNodes = selectedNodes;
		}
	}
}
