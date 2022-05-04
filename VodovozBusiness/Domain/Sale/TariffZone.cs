using System;
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

		private TimeSpan from;
		[Display(Name = "От часа")]
		public virtual TimeSpan From
		{
			get => from;
			set => SetField(ref @from, value, () => From);
		}

		private TimeSpan to;
		[Display(Name = "До часа")]
		public virtual TimeSpan To
		{
			get => to;
			set => SetField(ref to, value, () => To);
		}
	}
}
