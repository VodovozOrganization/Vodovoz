using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSContacts;
using QS.DomainModel.Entity;
using QSOrmProject;
using QSProjectsLib;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Client
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
		NominativePlural = "контакты",
		Nominative = "контакт",
		Accusative = "контакта",
		AccusativePlural = "контакты"

	)]
	public class Contact : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string surname;

		[PropertyChangedAlso ("FullName")]
		[Display (Name = "Фамилия")]
		public virtual string Surname {
			get { return surname; }
			set { SetField (ref surname, value, () => Surname); }
		}

		string name;

		[PropertyChangedAlso ("FullName")]
		[Display (Name = "Имя")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string patronymic;

		[PropertyChangedAlso ("FullName")]
		[Display (Name = "Отчество")]
		public virtual string Patronymic {
			get { return patronymic; }
			set { SetField (ref patronymic, value, () => Patronymic); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		bool isFired;

		[Display (Name = "Уволенный")]
		public virtual bool IsFired {
			get { return isFired; }
			set { SetField (ref isFired, value, () => IsFired); }
		}

		Post post;

		[Display (Name = "Должность")]
		public virtual Post Post {
			get { return post; }
			set { SetField (ref post, value, () => Post); }
		}

		Counterparty counterparty;

		[Required]
		[Display (Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get { return counterparty; }
			protected set { SetField (ref counterparty, value, () => Counterparty); }
		}

		[Display (Name = "Телефоны")]
		public virtual IList<Phone> Phones { get; set; }

		[Display (Name = "E-mail адреса")]
		public virtual IList<Email> Emails { get; set; }

		[Display (Name = "Точки доставки")]
		public virtual IList<DeliveryPoint> DeliveryPoints { get; set; }

		#endregion

		public virtual string PointCurator {
			get {
				if (DeliveryPoints == null || DeliveryPoints.Count <= 0)
					return String.Empty;
				if (DeliveryPoints.Count == 1)
					return DeliveryPoints [0].CompiledAddress;
				return String.Format ("{0} и еще {1}", DeliveryPoints [0].CompiledAddress, DeliveryPoints.Count);
			}
		}

		public Contact ()
		{
			Name = String.Empty;
			Surname = String.Empty;
			Patronymic = String.Empty;
			Comment = String.Empty;
			IsFired = false;
		}

		public virtual string FullName { get { return StringWorks.PersonFullName (Surname, Name, Patronymic); } }

		public virtual string Title {get {return FullName;}}

		public virtual string MainPhoneString { 
			get { 
				if (Phones.Count > 0 && Phones [0].Number != String.Empty)
					return String.Format ("{0}{1}", 
						Phones [0].NumberType != null ? Phones [0].NumberType.Name + " " : String.Empty, 
						Phones [0].Number);
				else
					return String.Empty; 
			} 
		}

		public override bool Equals (Object obj)
		{
			Contact contactObj = obj as Contact; 
			if (contactObj == null)
				return false;
			else
				return Id.Equals (contactObj.Id);
		}

		public override int GetHashCode ()
		{
			return Id.GetHashCode (); 
		}

		public static IUnitOfWorkGeneric<Contact> Create (Counterparty counterparty)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<Contact> ();
			uow.Root.Counterparty = counterparty;
			return uow;
		}
	}
}

