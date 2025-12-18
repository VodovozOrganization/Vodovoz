using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Sale;

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
		private string _shortName;
		private string _address;
		private bool _isArchive;
		private bool _pacsTimeManagementEnabled;
		private int? _financialResponsibilityCenterId;
		private int? _chiefId;
		private GeoGroupEntity _geographicGroup;

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

		[Display(Name = "Сокращенное наименование")]
		public virtual string ShortName
		{
			get => _shortName;
			set => SetField(ref _shortName, value);
		}

		[Display(Name = "Адрес подразделения")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
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

		[Display(Name = "Обслуживаемая часть города")]
		public virtual GeoGroupEntity GeographicGroup
		{
			get => _geographicGroup;
			set => SetField(ref _geographicGroup, value);
		}
	}
}
