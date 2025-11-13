using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Contacts
{
	/// <summary>
	/// Тип e-mail
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "типы e-mail",
		Nominative = "тип e-mail")]
	[EntityPermission]
	public class EmailTypeEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _name;
		private EmailPurpose _emailPurpose;

		/// <summary>
		/// Конструктор по умолчанию
		/// </summary>
		public EmailTypeEntity()
		{
			Name = string.Empty;
		}

		#region Свойства

		/// <summary>
		/// Идентификатор<br/>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// E-mail
		/// </summary>
		[Display(Name = "E-mail")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Дополнительный тип
		/// </summary>
		[Display(Name = "Дополнительный тип")]
		public virtual EmailPurpose EmailPurpose
		{
			get => _emailPurpose;
			set => SetField(ref _emailPurpose, value);
		}

		#endregion Свойства
	}
}
