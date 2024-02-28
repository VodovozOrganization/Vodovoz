﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "контакты",
		Nominative = "контакт",
		Accusative = "контакта",
		AccusativePlural = "контакты"

	)]
	[EntityPermission]
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
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		[Display (Name = "Телефоны")]
		public virtual IList<Phone> Phones { get; set; }

		[Display(Name = "E-mail адреса")]
		public virtual IList<Email> Emails { get; set; } = new List<Email>();

		[Display (Name = "Точки доставки")]
		public virtual IList<DeliveryPoint> DeliveryPoints { get; set; }

		#endregion

		public virtual string PointCurator {
			get {
				if (DeliveryPoints == null || DeliveryPoints.Count <= 0)
					return String.Empty;
				if (DeliveryPoints.Count == 1)
					return DeliveryPoints [0].CompiledAddress;
				return $"{DeliveryPoints[0].CompiledAddress} и еще {DeliveryPoints.Count}";
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

		public virtual string FullName => PersonHelper.PersonFullName (Surname, Name, Patronymic);

		public virtual string Title => FullName;

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
}

