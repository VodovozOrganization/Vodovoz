using System;
using QSOrmProject;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки документа погрузки",
		Nominative = "строка документа погрузки")]	
	public class CarLoadDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		CarLoadDocument document;

		public virtual CarLoadDocument Document {
			get { return document; }
			set { SetField (ref document, value, () => Document); }
		}

		WarehouseMovementOperation movementOperation;

		public virtual WarehouseMovementOperation MovementOperation { 
			get { return movementOperation; }
			set { SetField (ref movementOperation, value, () => MovementOperation); }
		}
	}
}

