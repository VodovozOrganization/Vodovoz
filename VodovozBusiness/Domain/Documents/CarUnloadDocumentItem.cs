using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Service;

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

		ReciveTypes reciveType;

		public virtual ReciveTypes ReciveType { 
			get { return reciveType; } 
			set { SetField (ref reciveType, value, () => ReciveType); }
		}

		WarehouseMovementOperation movementOperation;

		public virtual WarehouseMovementOperation MovementOperation { 
			get { return movementOperation; }
			set { SetField (ref movementOperation, value, () => MovementOperation); }
		}

		ServiceClaim serviceClaim;

		[Display (Name = "Заявка на сервис")]
		public virtual ServiceClaim ServiceClaim {
			get { return serviceClaim; }
			set { SetField (ref serviceClaim, value, () => ServiceClaim); }
		}

		string redhead;
		[Display(Name = "№ кулера")]
		public virtual string Redhead {
			get { return redhead; }
			set { SetField(ref redhead, value, () => Redhead); }
		}

		CullingCategory typeOfDefect;
		[Display(Name = "Тип брака")]
		public virtual CullingCategory TypeOfDefect {
			get { return typeOfDefect; }
			set { SetField(ref typeOfDefect, value, () => TypeOfDefect); }
		}

		DefectSource source;
		[Display(Name = "Источник брака")]
		public virtual DefectSource Source {
			get { return source; }
			set { SetField(ref source, value, () => Source); }
		}

		public virtual string Title {
			get {
				return String.Format("[{2}] {0} - {1}",
					MovementOperation.Nomenclature.Name,
				    MovementOperation.Nomenclature.Unit.MakeAmountShortStr(MovementOperation.Amount),
					document.Title);
			}
		}

	}

	public enum ReciveTypes
	{
		[Display (Name = "Возврат тары")]
		Bottle,
		[Display (Name = "Оборудование по заявкам")]
		Equipment,
		[Display (Name = "Возврат недовоза")]
		Returnes,
		[Display(Name = "Брак")]
		Defective
	}

	public class ReciveTypesStringType : NHibernate.Type.EnumStringType
	{
		public ReciveTypesStringType () : base (typeof(ReciveTypes))
		{
		}
	}
}

