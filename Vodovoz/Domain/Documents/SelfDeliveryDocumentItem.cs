using System;
using QSOrmProject;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]	
	public class SelfDeliveryDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		SelfDeliveryDocument document;

		public virtual SelfDeliveryDocument Document {
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

