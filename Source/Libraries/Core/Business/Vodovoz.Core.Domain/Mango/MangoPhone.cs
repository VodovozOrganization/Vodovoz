using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Mango
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "телефоны манго",
		Nominative = "телефон манго")]
	[EntityPermission]
	[HistoryTrace]
	public class MangoPhone : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _phoneNumber;
		private string _description;


		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Номер телефона")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

	}
}
