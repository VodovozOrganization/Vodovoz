using System;
using QS.Utilities.Text;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public class UndeliveredOrderCommentsNode
	{
		public string Comment { get; set; }

		public string MarkedupComment => String.Format(
			"<span foreground=\"{0}\">{1}</span>",
			Color,
			Comment
		);

		public string UserDateAndName => String.Format(
			"<span foreground=\"{0}\"><b>{1}\n{2}: </b></span>",
			Color,
			Date.ToString("d MMM, HH:mm:ss"),
			PersonHelper.PersonNameWithInitials(LName, FName, MName)
		);
		
		public DateTime Date { get; set; }
		public string FName { get; set; }
		public string MName { get; set; }
		public string LName { get; set; }
		public string Color { get; set; }
	}
}