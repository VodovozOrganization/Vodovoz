using System;
using QS.Utilities.Text;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public class UndeliveredOrderCommentsNode
	{
		public string Comment { get; set; }

		public string UserDateAndName =>
			$"{Date:d MMM, HH:mm:ss}\n" +
			$"{PersonHelper.PersonNameWithInitials(LName, FName, MName)}: ";
		
		public DateTime Date { get; set; }
		public string FName { get; set; }
		public string MName { get; set; }
		public string LName { get; set; }
	}
}
