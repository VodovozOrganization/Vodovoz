using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Users.Settings;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "подразделения",
		Nominative = "подразделение",
		GenitivePlural = "подразделений")]
	[EntityPermission]
	[HistoryTrace]
	public class SubdivisionEntity : PropertyChangedBase, INamedDomainObject
	{
		private int _id;
		private string _name;
		private bool _pacsTimeManagementEnabled;
		private int? _financialResponsibilityCenterId;
		private int? _chiefId;
		
		private IObservableList<CashSubdivisionSortingSettings> _cashSubdivisionSortingSettings = new ObservableList<CashSubdivisionSortingSettings>();

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Название подразделения
		/// </summary>
		[Display(Name = "Название подразделения")]
		[Required(ErrorMessage = "Название подразделения должно быть заполнено.")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Включено ли управление временем по СКУД
		/// </summary>
		[Display(Name = "Контроль времени по СКУД")]
		public virtual bool PacsTimeManagementEnabled
		{
			get => _pacsTimeManagementEnabled;
			set => SetField(ref _pacsTimeManagementEnabled, value);
		}

		/// <summary>
		/// Идентификатор начальника подразделения
		/// </summary>
		[Display(Name = "Начальник подразделения")]
		[HistoryIdentifier(TargetType = typeof(EmployeeEntity))]
		public virtual int? ChiefId
		{
			get => _chiefId;
			set => SetField(ref _chiefId, value);
		}

		/// <summary>
		/// Идентификатор центра финансовой ответственности
		/// </summary>
		[Display(Name = "Центр финансовой ответственности")]
		[HistoryIdentifier(TargetType = typeof(FinancialResponsibilityCenter))]
		public virtual int? FinancialResponsibilityCenterId
		{
			get => _financialResponsibilityCenterId;
			set => SetField(ref _financialResponsibilityCenterId, value);
		}
		
		/// <summary>
		/// Настройки сортировки касс
		/// </summary>
		[Display(Name = "Настройки сортировки касс")]
		public virtual IObservableList<CashSubdivisionSortingSettings> CashSubdivisionSortingSettings
		{
			get => _cashSubdivisionSortingSettings;
			set => SetField(ref _cashSubdivisionSortingSettings, value);
		}
	}
}
