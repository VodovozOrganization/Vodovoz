using System;
using QS.Tdi;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewWidgets;

namespace Vodovoz.Infrastructure.Services
{
	public class GtkTaskCreationInteractive : ITaskCreationInteractive
	{
		public CreationTaskResult RunQuestion(ref DateTime? dateTime)
		{
			var questionWindow = new TaskCreationDialog();
			questionWindow.Modal = true;
			questionWindow.Run();
			dateTime = questionWindow.Date;
			return questionWindow.creationTaskResult;
		}
	}
}
