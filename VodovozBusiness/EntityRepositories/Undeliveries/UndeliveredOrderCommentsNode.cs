using System;
using QS.Utilities.Text;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public class UndeliveredOrderCommentsNode
	{
		public string Comment { get; set; }

		public string MarkedupComment => $"<span foreground=\"{Color}\">{Comment}</span>";

		public string UserDateAndName =>
			$"<span foreground=\"{ Color }\"><b>{ Date.ToString("d MMM, HH:mm:ss") }\n" +
			$"{ PersonHelper.PersonNameWithInitials(LName, FName, MName) }: </b></span>";
		
		public DateTime Date { get; set; }
		public string FName { get; set; }
		public string MName { get; set; }
		public string LName { get; set; }
		public string Color { get; set; }
	}
}