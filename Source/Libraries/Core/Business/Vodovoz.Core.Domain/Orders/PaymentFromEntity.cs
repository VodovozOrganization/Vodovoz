using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "место, откуда проведены оплаты",
		Nominative = "место, откуда проведена оплата")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentFromEntity : PropertyChangedBase, IDomainObject, INamed, IArchivable
	{
		private int _id;
		private string _name;
		private bool _isArchive;
		//private string _organizationCriterion;

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

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		
		/*[Display(Name = "Условия для установки организации")]
		[IgnoreHistoryTrace]
		public virtual string OrganizationCriterion
		{
			get => _organizationCriterion;
			set => SetField(ref _organizationCriterion, value);
		}*/
	}
}
