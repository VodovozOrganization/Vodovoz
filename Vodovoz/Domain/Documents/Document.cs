using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;

namespace Vodovoz.Domain.Documents
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
		[Display (Name = "Входящая накладная")]
		IncomingInvoice,
		[Display (Name = "Документ производства")]
		IncomingWater,
		[Display (Name = "Документ перемещения")]
		MovementDocument,
		[Display (Name = "Акт списания")]
		WriteoffDocument,
		[Display (Name = "Талон погрузки")]
		CarLoadDocument
	}

	public class DocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public DocumentTypeStringType () : base (typeof(DocumentType))
		{
		}
	}
}

