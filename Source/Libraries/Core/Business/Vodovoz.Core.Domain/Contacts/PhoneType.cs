using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Contacts
{
	/// <summary>
	/// Тип телефона
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы телефонов",
		Nominative = "тип телефона")]
	[EntityPermission]
	public class PhoneTypeEntity : PropertyChangedBase, IDomainObject
	{
		protected int _id;
		private string _name;
		private PhonePurpose _phonePurpose;

		/// <summary>
		/// Конструктор по умолчанию
		/// </summary>
		public PhoneTypeEntity()
		{
			Name = string.Empty;
		}

		/// <summary>
		/// Идентификатор
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Тип телефона
		/// </summary>
		[Display(Name = "Тип телефона")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Назначение типа телефона
		/// </summary>
		[Display(Name = "Назначение типа телефона")]
		public virtual PhonePurpose PhonePurpose
		{
			get => _phonePurpose;
			set => SetField(ref _phonePurpose, value);
		}
	}
}
