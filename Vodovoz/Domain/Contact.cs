using System;
using QSOrmProject;
using System.Collections.Generic;
using System.Data.Bindings;
using QSContacts;

namespace Vodovoz
{
	[OrmSubject ("Контакты")]
	public class Contact : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string lastName;

		public virtual string Lastname {
			get { return lastName; }
			set { SetField (ref lastName, value, () => Lastname); }
		}

		string surname;

		public virtual string Surname {
			get { return surname; }
			set { SetField (ref surname, value, () => Surname); }
		}
			
		string comment;

		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		bool fired;

		public virtual bool Fired {
			get { return fired; }
			set { SetField (ref fired, value, () => Fired); }
		}

		Post post;

		public virtual Post Post {
			get { return post; }
			set { SetField (ref post, value, () => Post); }
		}

		public virtual IList<Phone> Phones { get; set; }

		public virtual IList<Email> Emails { get; set; }

		public virtual IList<DeliveryPoint> DeliveryPoints { get; set; }

		#endregion

		public string PointCurator {
			get{
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
	}

	public interface IContactOwner
	{
		IList<Contact> Contacts { get; set; }
	}
}

