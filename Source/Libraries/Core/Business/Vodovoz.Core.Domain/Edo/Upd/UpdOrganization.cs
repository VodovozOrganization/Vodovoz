using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo.Upd
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "Организации",
		Nominative = "Организация")]
	public class UpdOrganization
	{

		/// <summary>
		/// Наименование
		/// </summary>
		[Display(Name = "Наименование")]
		public virtual string Name { get; set; }

		/// <summary>
		/// Код страны
		/// </summary>
		[Display(Name = "Код страны")]
		public virtual string CountryCode { get; set; }

		/// <summary>
		/// Наименование страны
		/// </summary>
		[Display(Name = "Наименование страны")]
		public virtual string CountryName { get; set; }

		/// <summary>
		/// Адрес
		/// </summary>
		[Display(Name = "Адрес")]
		public virtual string Address { get; set; }

		/// <summary>
		/// ИНН
		/// </summary>
		[Display(Name = "ИНН")]
		public virtual string Inn { get; set; }

		/// <summary>
		/// КПП
		/// </summary>
		[Display(Name = "КПП")]
		public virtual string Kpp { get; set; }

		/// <summary>
		/// ОГРН/ОГРНИП
		/// </summary>
		[Display(Name = "ОГРН/ОГРНИП")]
		public virtual string Ogrn { get; set; }

		/// <summary>
		/// Дата ОГРН/ОГРНИП
		/// </summary>
		[Display(Name = "Дата ОГРН/ОГРНИП")]
		public virtual DateTime? OgrnDate { get; set; }

		/// <summary>
		/// Номер кабинета в ЭДО
		/// </summary>
		[Display(Name = "Номер кабинета в ЭДО")]
		public virtual string EdoAccountId { get; set; }

		/// <summary>
		/// Организация работает без НДС
		/// </summary>
		[Display(Name = "Организация работает без НДС")]
		public virtual bool IsWithoutVat { get; set; }
	}
}
