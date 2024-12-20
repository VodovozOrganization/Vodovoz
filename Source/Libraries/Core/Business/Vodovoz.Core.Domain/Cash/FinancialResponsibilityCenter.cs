using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Domain.Cash
{
	/// <summary>
	/// Центр финансовой ответственности
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "центр фининсовой ответственности",
		Genitive = "центра фининсовой ответственности",
		Accusative = "центр фининсовой ответственности",
		Prepositional = "центре фининсовой ответственности",
		NominativePlural = "центры финансовой ответственности",
		GenitivePlural = "центров фининсовой ответственности",
		AccusativePlural = "центры фининсовой ответственности",
		PrepositionalPlural = "центрах фининсовой ответственности")]
	[HistoryTrace]
	[EntityPermission]
	public class FinancialResponsibilityCenter : PropertyChangedBase, IDomainObject, IArchivable, INamed
	{
		private int _id;
		private string _name;
		private int? _responsibleEmployeeId;
		private int? _viceResponsibleEmployeeId;
		private bool _isArchive;
		private bool _requestApprovalDenied;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Название ЦФО
		/// </summary>
		[Display(Name = "Название ЦФО")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Идентификатор сотрудника - ответственного лица
		/// </summary>
		[Display(Name = "Ответственное лицо")]
		[HistoryIdentifier(TargetType = typeof(EmployeeEntity))]
		public virtual int? ResponsibleEmployeeId
		{
			get => _responsibleEmployeeId;
			set => SetField(ref _responsibleEmployeeId, value);
		}

		/// <summary>
		/// Идентификатор сотрудника - заместителя ответственного лица
		/// </summary>
		[Display(Name = "Заместитель")]
		[HistoryIdentifier(TargetType = typeof(EmployeeEntity))]
		public virtual int? ViceResponsibleEmployeeId
		{
			get => _viceResponsibleEmployeeId;
			set => SetField(ref _viceResponsibleEmployeeId, value);
		}

		/// <summary>
		/// Архив
		/// </summary>
		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Подтверждение заявок запрещено
		/// </summary>
		[Display(Name = "Подтверждение заявок запрещено")]
		public virtual bool RequestApprovalDenied
		{
			get => _requestApprovalDenied;
			set => SetField(ref _requestApprovalDenied, value);
		}
	}
}
