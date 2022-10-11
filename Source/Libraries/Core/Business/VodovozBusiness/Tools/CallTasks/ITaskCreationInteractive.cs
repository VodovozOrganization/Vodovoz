using System;

namespace Vodovoz.Tools.CallTasks
{
	public interface ITaskCreationInteractive
	{
		CreationTaskResult RunQuestion(ref DateTime? dateTime);
	}

	public enum CreationTaskResult
	{
		Auto,
		DatePick,
		Cancel
	}
}
