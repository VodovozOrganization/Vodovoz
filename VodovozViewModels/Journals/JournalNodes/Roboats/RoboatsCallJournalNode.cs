using Gamma.Utilities;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader.Hierarchy;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Roboats
{
	public class RoboatsCallJournalNode : JournalNodeBase, IHierarchicalNode<RoboatsCallJournalNode>
	{
		private string _callStatus = null;
		private string _callResult = null;
		public override string Title => $"{Phone} {Time:dd.MM.yyyy HH:mm:ss}";

		public RoboatsCallNodeType NodeType { get; set; }

		public int Id { get; set; }

		public string EntityId => Id == 0 ? "" : Id.ToString();
		public DateTime Time { get; set; }
		public string Phone { get; set; }
		public RoboatsCallStatus? Status { get; set; }
		public string CallStatus
		{
			get
			{
				if(_callStatus == null && Status.HasValue)
				{
					_callStatus = Status.GetEnumTitle();
				}
				return _callStatus;
			}
		}

		public RoboatsCallResult? Result { get; set; }
		public string CallResult
		{
			get
			{
				if(_callResult == null && Result.HasValue)
				{
					_callResult = Result.GetEnumTitle();
				}
				return _callResult;
			}
		}

		public int ProblemsCount { get; set; }
		public string Details { get; set; }

		public string Description
		{
			get
			{
				if(ProblemsCount > 0)
				{
					return $"Проблемы: {ProblemsCount}";
				}
				else
				{
					return Details;
				}
			}
		}

		public int? ParentId { get; set; }
		public RoboatsCallJournalNode Parent { get; set; }
		public IList<RoboatsCallJournalNode> Children { get; set; }
	}
}
