using Gamma.Utilities;
using QS.Project.Journal;
using System;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Roboats
{
	public class RoboatsCallJournalNode : JournalNodeBase
	{
		private string _callStatus = null;
		private string _callResult = null;

		public int Id { get; set; }
		public DateTime Time { get; set; }
		public string Phone { get; set; }
		public RoboatsCallStatus Status { get; set; }
		public string CallStatus
		{
			get
			{
				if(_callStatus == null)
				{
					_callStatus = Status.GetEnumTitle();
				}
				return _callStatus;
			}
		}

		public RoboatsCallResult Result { get; set; }
		public string CallResult
		{
			get
			{
				if(_callResult == null)
				{
					_callResult = Result.GetEnumTitle();
				}
				return _callResult;
			}
		}

		public string Details { get; set; }
	}
}
