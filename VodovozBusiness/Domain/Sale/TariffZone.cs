using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "тарифные зоны",
		Nominative = "тарифная зона")]
	[EntityPermission]
	public class TariffZone : BusinessObjectBase<TariffZone>, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Required(ErrorMessage = "Имя обязательно")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}
	}
}
