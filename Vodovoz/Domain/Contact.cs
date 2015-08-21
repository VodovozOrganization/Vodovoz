using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSContacts;
using QSOrmProject;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "контакты",
		Nominative = "контакт",
		Accusative = "контакта",
		AccusativePlural = "контакты"

	)]
	public class Contact : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[PropertyChangedAlso ("FullName")]
		[Display (Name = "Имя")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string lastName;

		[PropertyChangedAlso ("FullName")]
		[Display (Name = "Фамилия")]
		public virtual string Lastname {
			get { return lastName; }
			set { SetField (ref lastName, value, () => Lastname); }
		}

		string surname;

		[PropertyChangedAlso ("FullName")]
		[Display (Name = "Отчество")]
		public virtual string Surname {
			get { return surname; }
			set { SetField (ref surname, value, () => Surname); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		bool fired;

		[Display (Name = "Уволенный")]
		public virtual bool Fired {
			get { return fired; }
			set { SetField (ref fired, value, () => Fired); }
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

		public string PointCurator {
			get {
				if (DeliveryPoints == null || DeliveryPoints.Count <= 0)
					return String.Empty;
				if (DeliveryPoints.Count == 1)
					return DeliveryPoints [0].Name;
				return String.Format ("{0} и еще {1}", DeliveryPoints [0].Name, DeliveryPoints.Count);
			}
		}

		public Contact ()
		{
			Name = String.Empty;
			Surname = String.Empty;
			Lastname = String.Empty;
			Comment = String.Empty;
			Fired = false;
		}

		public string FullName { get { return String.Format ("{0} {1} {2}", Surname, Name, Lastname); } }

		public string MainPhoneString { 
			get { 
				if (Phones.Count > 0 && Phones [0].Number != String.Empty)
					return String.Format ("{0}{1}", 
						Phones [0].NumberType != null ? Phones [0].NumberType.Name + " " : String.Empty, 
						Phones [0].Number);
				else
					return String.Empty; 
			} 
		}

		public string PostName {
			get { 
				if (Post == null)
					return String.Empty;
				else
					return Post.Name;
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

