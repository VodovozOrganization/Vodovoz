using System;
using Gtk;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.ViewWidgets
{
	public partial class TaskCreationDialog : Gtk.Dialog
	{
		public CreationTaskResult creationTaskResult { get; set; } = CreationTaskResult.Cancel;

		public DateTime? Date { get; set; }

		public TaskCreationDialog() 
		{
			this.Build();
		}

		protected void OnButtonAutoClicked(object sender, EventArgs e)
		{
			creationTaskResult = CreationTaskResult.Auto;
			Date = null;

			Destroy();
		}

		protected void OnButtonPickDateClicked(object sender, EventArgs e)
		{
			Window parentWin = (Window)this.Toplevel;
			Dialog editDate = new Dialog("Укажите дату",
				parentWin, DialogFlags.DestroyWithParent) {
				Modal = true
			};
			editDate.AddButton("Отмена", ResponseType.Cancel);
			editDate.AddButton("Ok", ResponseType.Ok);
			Calendar SelectDate = new Calendar();
			SelectDate.DisplayOptions = CalendarDisplayOptions.ShowHeading |
				CalendarDisplayOptions.ShowDayNames |
					CalendarDisplayOptions.ShowWeekNumbers;
			SelectDate.DaySelectedDoubleClick += (send, args) => editDate.Respond(ResponseType.Ok);
			SelectDate.Date = Date ?? DateTime.Now.Date;

			editDate.VBox.Add(SelectDate);
			editDate.ShowAll();
			int response = editDate.Run();
			if(response == (int)ResponseType.Ok)
				Date = SelectDate.GetDate();
			SelectDate.Destroy();
			editDate.Destroy();

			if(Date != null)
				creationTaskResult = CreationTaskResult.DatePick;

			Destroy();
		}
	}
}
