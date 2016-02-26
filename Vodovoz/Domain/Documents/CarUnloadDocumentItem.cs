using System;
using QSOrmProject;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки документа разгрузки",
		Nominative = "строка документа разгрузки")]	
	public class CarUnloadDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		CarUnloadDocument document;

		public virtual CarUnloadDocument Document {
			get { return document; }
			set { SetField (ref document, value, () => Document); }
		}

		WarehouseMovementOperation movementOperation;

		public virtual WarehouseMovementOperation MovementOperation { 
			get { return movementOperation; }
			set { SetField (ref movementOperation, value, () => MovementOperation); }
		}

		public virtual string Title {
			get{
				return String.Format("{0} - {1}", 
					MovementOperation.Nomenclature.Name, 
					MovementOperation.Nomenclature.Unit.MakeAmountShortStr(MovementOperation.Amount));
			}
		}

	}
}

