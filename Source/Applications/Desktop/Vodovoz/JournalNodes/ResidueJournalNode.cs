using System;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.Client;

namespace Vodovoz.JournalNodes
{
	public class ResidueJournalNode : JournalEntityNodeBase<Residue>
	{
		public override string Title => Counterparty;

		public DateTime Date { get; set; }

		public string DateString => Date.ToShortDateString() + " " + Date.ToShortTimeString();

		public string Counterparty { get; set; }

		public string Comment { get; set; }

		public DateTime LastEditedTime { get; set; }

		public string AuthorSurname { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string Author => PersonHelper.PersonNameWithInitials(AuthorSurname, AuthorName, AuthorPatronymic);

		public string LastEditorSurname { get; set; }
		public string LastEditorName { get; set; }
		public string LastEditorPatronymic { get; set; }

		public string LastEditor => PersonHelper.PersonNameWithInitials(LastEditorSurname, LastEditorName, LastEditorPatronymic);

		string deliveryPoint;
		public string DeliveryPoint {
			get => deliveryPoint ?? "Самовывоз";
			set => deliveryPoint = value;
		}
	}
}