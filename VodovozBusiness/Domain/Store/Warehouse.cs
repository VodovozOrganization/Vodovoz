using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System.Collections.Generic;
using Vodovoz.Domain.Operations;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace Vodovoz.Domain.Store
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "склады",
		Nominative = "склад")]
	public class Warehouse : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название склада должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		bool canReceiveBottles;
		public virtual bool CanReceiveBottles{
			get{ return canReceiveBottles; }
			set{ SetField (ref canReceiveBottles, value, () => CanReceiveBottles); }
		}

		bool canReceiveEquipment;
		public virtual bool CanReceiveEquipment{
			get{ return canReceiveEquipment; }
			set{ SetField (ref canReceiveEquipment, value, () => CanReceiveEquipment); }
		}

		#endregion

		public Warehouse ()
		{
			Name = String.Empty;
		}
	}
}