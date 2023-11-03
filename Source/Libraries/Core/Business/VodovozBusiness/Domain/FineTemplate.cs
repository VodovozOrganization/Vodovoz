﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны комментариев для штрафов",
		Nominative = "шаблон комментария для штрафа",
		GenitivePlural = "шаблонов комментариев для штрафов")]
	[EntityPermission]
	public class FineTemplate : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string reason;

		[Display(Name = "Причина")]
		public virtual string Reason { 
			get { return reason; } 
			set { SetField (ref reason, value, () => Reason); }
		}

		private decimal fineMoney;

		[Display(Name = "Сумма штрафа")]
		public virtual decimal FineMoney
		{
			get { return fineMoney; }
			set { SetField(ref fineMoney, value, () => FineMoney); }
		}


		#endregion

		public FineTemplate ()
		{
			Reason = String.Empty;
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty (Reason))
				yield return new ValidationResult ("Текст комментария должен быть заполнен.", new [] { "Comment" });
		}

		#endregion
	}
}

