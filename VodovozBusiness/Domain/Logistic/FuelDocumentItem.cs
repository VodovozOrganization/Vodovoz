using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
	NominativePlural = "строки оплаты топлива",
	Nominative = "строка оплаты топлива")]
	public class FuelDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private FuelDocument document;

		[Display (Name = "Документ")]
		public virtual FuelDocument Document {
			get { return document; }
			set { SetField(ref document, value, () => Document); }
		}

		private GazTicket gasTicket;

		[Display (Name = "Талон на топливо")]
		public virtual GazTicket GasTicket {
			get { return gasTicket; }
			set { SetField(ref gasTicket, value, () => GasTicket); }
		}

		private int ticketsCount;

		[Display (Name = "Количество талонов")]
		public virtual int  TicketsCount {
			get { return ticketsCount; }
			set { SetField(ref ticketsCount, value, () => TicketsCount); }
		}

		private IList<FuelDocumentItem> items;

		[Display (Name = "Строки")]
		public virtual IList<FuelDocumentItem>  Items {
			get { return items; }
			set { SetField(ref items, value, () => Items); }
		}

		public FuelDocumentItem()
		{
		}
	}
}

