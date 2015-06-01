using System;
using QSOrmProject;
using System.Data.Bindings;

namespace Vodovoz.Domain
{
	public class Document : PropertyChangedBase, IDomainObject, IDocument
	{
		public virtual int Id { get; set; }

		DateTime timeStamp = DateTime.Now;

		public virtual DateTime TimeStamp {
			get { return timeStamp; }
			set { SetField (ref timeStamp, value, () => TimeStamp); }
		}

		#region IDocument implementation

		public virtual string DocType {
			get { return "Тип не указан!"; }
		}

		public virtual string Description {
			get { return "Описание не определено!"; }
		}

		#endregion

		public virtual string DateString { get { return TimeStamp.ToShortDateString () + " " + TimeStamp.ToShortTimeString (); } }

		public virtual string Number { get { return Id.ToString (); } }
	}

	public interface IDocument
	{
		string DocType { get; }

		string Description { get; }
	}

	public enum DocumentType
	{
		[ItemTitleAttribute ("Входящая накладная")]
		IncomingInvoice,
		[ItemTitleAttribute ("Документ производства")]
		IncomingWater,
		[ItemTitleAttribute ("Документ перемещения")]
		MovementDocument,
		[ItemTitleAttribute ("Акт списания")]
		WriteoffDocument

	}
}

