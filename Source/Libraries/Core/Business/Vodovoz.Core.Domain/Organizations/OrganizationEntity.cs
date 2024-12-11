using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "организации",
		Nominative = "организация")]
	[EntityPermission]
	[HistoryTrace]
	public class OrganizationEntity : AccountOwnerBase, IDomainObject, INamed
	{
		private int _id;
		private string _name;
		private string _fullName;
		private string _iNN;
		private string _kPP;
		private string _oGRN;
		private string _oKPO;
		private string _oKVED;
		private string _email;

		public OrganizationEntity()
		{
			Name = "Новая организация";
			FullName = string.Empty;
			INN = string.Empty;
			KPP = string.Empty;
			OGRN = string.Empty;
			Email = string.Empty;
			OKPO = string.Empty;
			OKVED = string.Empty;
		}

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Полное название")]
		public virtual string FullName
		{
			get => _fullName;
			set => SetField(ref _fullName, value);
		}

		[Display(Name = "ИНН")]
		public virtual string INN
		{
			get => _iNN;
			set => SetField(ref _iNN, value);
		}

		[Display(Name = "КПП")]
		public virtual string KPP
		{
			get => _kPP;
			set => SetField(ref _kPP, value);
		}

		[Display(Name = "ОГРН/ОГРНИП")]
		public virtual string OGRN
		{
			get => _oGRN;
			set => SetField(ref _oGRN, value);
		}

		[Display(Name = "ОКПО")]
		public virtual string OKPO
		{
			get => _oKPO;
			set => SetField(ref _oKPO, value);
		}

		[Display(Name = "ОКВЭД")]
		public virtual string OKVED
		{
			get => _oKVED;
			set => SetField(ref _oKVED, value);
		}

		[Display(Name = "E-mail адреса")]
		public virtual string Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}
	}
}
