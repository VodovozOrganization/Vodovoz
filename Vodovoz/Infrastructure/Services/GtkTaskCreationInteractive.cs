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
			var questionWindow = new TaskCreationWindow();
			questionWindow.Modal = true;
			questionWindow.ShowAll();
			dateTime = questionWindow.Date;
			return questionWindow.creationTaskResult;
		}
	}
}
