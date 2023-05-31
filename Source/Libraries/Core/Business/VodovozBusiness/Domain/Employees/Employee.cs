using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.Utilities;
using MySql.Data.MySqlClient;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Project.Services;
using QS.Utilities.Text;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сотрудники",
		Nominative = "сотрудник")]
	[EntityPermission]
	[HistoryTrace]
	public class Employee : Personnel, IEmployee
	{
		private const int _commentLimit = 255;
		private string _comment;
		
		private IList<EmployeeRegistrationVersion> _employeeRegistrationVersions = new List<EmployeeRegistrationVersion>();
		private GenericObservableList<EmployeeRegistrationVersion> _observableEmployeeRegistrationVersions;

		#region Свойства

		public override EmployeeType EmployeeType {
			get { return EmployeeType.Employee; }
			set { }
		}

		EmployeeCategory category;

		[Display(Name = "Категория")]
		public virtual EmployeeCategory Category {
			get => category;
			set => SetField(ref category, value);
		}
		
		uint? innerPhone;

		[Display(Name = "Внутренний номер")]
		public virtual uint? InnerPhone {
			get { return innerPhone; }
			set { SetField(ref innerPhone, value, () => InnerPhone); }
		}

		string androidLogin;

		[Display(Name = "Логин для Android приложения")]
		public virtual string AndroidLogin {
			get { return androidLogin; }
			set { SetField(ref androidLogin, value, () => AndroidLogin); }
		}

		string androidPassword;

		[Display(Name = "Пароль для Android приложения")]
		public virtual string AndroidPassword {
			get { return androidPassword; }
			set { SetField(ref androidPassword, value, () => AndroidPassword); }
		}

		string androidSessionKey;

		[Display(Name = "Ключ сессии для Android приложения")]
		public virtual string AndroidSessionKey {
			get { return androidSessionKey; }
			set { SetField(ref androidSessionKey, value, () => AndroidSessionKey); }
		}

		string androidToken;

		[Display(Name = "Токен Android приложения пользователя для отправки Push-сообщений")]
		public virtual string AndroidToken {
			get { return androidToken; }
			set { SetField(ref androidToken, value, () => AndroidToken); }
		}

		EmployeeStatus status;

		[Display(Name = "Статус сотрудника")]
		public virtual EmployeeStatus Status {
			get { return status; }
			set { SetField(ref status, value, () => Status); }
		}

		User user;

		[Display(Name = "Пользователь")]
		public virtual User User {
			get { return user; }
			set { SetField(ref user, value, () => User); }
		}

		private Subdivision subdivision;

		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField(ref subdivision, value, () => Subdivision); }
		}

		private DateTime? firstWorkDay;
		[Display(Name = "Первый день работы")]
		public virtual DateTime? FirstWorkDay {
			get { return firstWorkDay; }
			set { SetField(ref firstWorkDay, value, () => FirstWorkDay); }
		}

		private DateTime? dateHired;
		[Display(Name = "Дата приема")]
		public virtual DateTime? DateHired {
			get { return dateHired; }
			set { SetField(ref dateHired, value, () => DateHired); }
		}

		private DateTime? dateFired;
		[Display(Name = "Дата увольнения")]
		public virtual DateTime? DateFired {
			get { return dateFired; }
			set { SetField(ref dateFired, value, () => DateFired); }
		}

		private DateTime? dateCalculated;
		[Display(Name = "Дата расчета")]
		public virtual DateTime? DateCalculated {
			get { return dateCalculated; }
			set { SetField(ref dateCalculated, value, () => DateCalculated); }
		}

		IList<EmployeeContract> contracts = new List<EmployeeContract>();

		[Display(Name = "Договора")]
		public virtual IList<EmployeeContract> Contracts {
			get { return contracts; }
			set { SetField(ref contracts, value, () => Contracts); }
		}

		GenericObservableList<EmployeeContract> observableContracts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeContract> ObservableContracts {
			get {
				if(observableContracts == null) {
					observableContracts = new GenericObservableList<EmployeeContract>(Contracts);
				}
				return observableContracts;
			}
		}

		private Employee defaultForwarder;

		[Display(Name = "Экспедитор по умолчанию")]
		public virtual Employee DefaultForwarder {
			get { return defaultForwarder; }
			set { SetField(ref defaultForwarder, value, () => DefaultForwarder); }
		}

		private DriverType? driverType;
		public virtual DriverType? DriverType {
			get => driverType;
			set => SetField(ref driverType, value);
		}

		private bool largusDriver;
		[Display(Name = "Сотрудник - водитель Ларгуса")]
		public virtual bool LargusDriver {
			get { return largusDriver; }
			set { SetField(ref largusDriver, value, () => LargusDriver); }
		}

		private CarTypeOfUse? _driverOfCarTypeOfUse;
		[Display(Name = "Водитель автомобиля типа")]
		public virtual CarTypeOfUse? DriverOfCarTypeOfUse {
			get => _driverOfCarTypeOfUse;
			set => SetField(ref _driverOfCarTypeOfUse, value);
		}

		private CarOwnType? _driverOfCarOwnType;
		[Display(Name = "Водитель автомобиля принадлежности")]
		public virtual CarOwnType? DriverOfCarOwnType {
			get => _driverOfCarOwnType;
			set => SetField(ref _driverOfCarOwnType, value);
		}

		private Gender? gender;
		[Display(Name = "Пол сотрудника")]
		public virtual Gender? Gender {
			get { return gender; }
			set { SetField(ref gender, value); }
		}

		private float driverSpeed = 1;

		[Display(Name = "Скорость работы водителя")]
		public virtual float DriverSpeed {
			get { return driverSpeed; }
			set { SetField(ref driverSpeed, value, () => DriverSpeed); }
		}

		private short tripPriority = 6;

		/// <summary>
		/// Приорите(1-10) чем меньше тем лучше. Фактически это штраф.
		/// </summary>
		[Display(Name = "Приоритет для маршрутов")]
		public virtual short TripPriority {
			get { return tripPriority; }
			set { SetField(ref tripPriority, value, () => TripPriority); }
		}

		int minRouteAddresses;

		[Display(Name = "Минимум адресов")]
		public virtual int MinRouteAddresses {
			get { return minRouteAddresses; }
			set { SetField(ref minRouteAddresses, value, () => MinRouteAddresses); }
		}

		int maxRouteAddresses;

		[Display(Name = "Максимум адресов")]
		public virtual int MaxRouteAddresses {
			get { return maxRouteAddresses; }
			set { SetField(ref maxRouteAddresses, value, () => MaxRouteAddresses); }
		}

		#region DriverDistrictPrioritySets

		private IList<DriverDistrictPrioritySet> driverDistrictPrioritySets = new List<DriverDistrictPrioritySet>();
		[Display(Name = "Версии приоритетов районов водителя")]
		public virtual IList<DriverDistrictPrioritySet> DriverDistrictPrioritySets {
			get => driverDistrictPrioritySets;
			set => SetField(ref driverDistrictPrioritySets, value);
		}

		private GenericObservableList<DriverDistrictPrioritySet> observableDriverDistrictPrioritySets;
		public virtual GenericObservableList<DriverDistrictPrioritySet> ObservableDriverDistrictPrioritySets =>
			observableDriverDistrictPrioritySets ?? (observableDriverDistrictPrioritySets =
				new GenericObservableList<DriverDistrictPrioritySet>(DriverDistrictPrioritySets));

		#endregion

		#region ObservableDriverWorkScheduleSets

		private IList<DriverWorkScheduleSet> driverWorkScheduleSets = new List<DriverWorkScheduleSet>();
		[Display(Name = "Версии графиков работы водителя")]
		public virtual IList<DriverWorkScheduleSet> DriverWorkScheduleSets {
			get => driverWorkScheduleSets;
			set => SetField(ref driverWorkScheduleSets, value);
		}

		private GenericObservableList<DriverWorkScheduleSet> observableDriverWorkScheduleSets;
		public virtual GenericObservableList<DriverWorkScheduleSet> ObservableDriverWorkScheduleSets
			=> observableDriverWorkScheduleSets ?? (observableDriverWorkScheduleSets =
				new GenericObservableList<DriverWorkScheduleSet>(DriverWorkScheduleSets));

		#endregion

		IList<EmployeeWageParameter> wageParameters = new List<EmployeeWageParameter>();
		[Display(Name = "Параметры расчета зарплаты")]
		public virtual IList<EmployeeWageParameter> WageParameters {
			get => wageParameters;
			set => SetField(ref wageParameters, value, () => WageParameters);
		}

		GenericObservableList<EmployeeWageParameter> observableWageParameters;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeWageParameter> ObservableWageParameters {
			get {
				if(observableWageParameters == null)
					observableWageParameters = new GenericObservableList<EmployeeWageParameter>(WageParameters);
				return observableWageParameters;
			}
		}

		bool visitingMaster;
		[Display(Name = "Выездной мастер")]
		public virtual bool VisitingMaster {
			get => visitingMaster;
			set => SetField(ref visitingMaster, value);
		}

		private bool isChainStoreDriver;
		[Display(Name = "Водитель для сетей")]
		public virtual bool IsChainStoreDriver {
			get => isChainStoreDriver;
			set => SetField(ref isChainStoreDriver, value);
		}

		bool isDriverForOneDay;
		public virtual bool IsDriverForOneDay {
			get => isDriverForOneDay;
			set => SetField(ref isDriverForOneDay, value);
		}

		private string loginForNewUser;
		[Display(Name = "Логин нового пользователя")]
		public virtual string LoginForNewUser {
			get { return loginForNewUser; }
			set { SetField(ref loginForNewUser, value, () => LoginForNewUser); }
		}
		
		Organization organisationForSalary;
		public virtual Organization OrganisationForSalary {
			get => organisationForSalary;
			set => SetField(ref organisationForSalary, value);
		}

        private string email;

		[Display(Name = "Электронная почта пользователя")]
        public virtual string Email
        {
            get => email;
            set => SetField(ref email, value);
        }
		
		[Display(Name = "Комментарий по сотруднику")]
		public virtual string Comment {
			get => _comment;
			set => SetField(ref _comment, value);
		}
		
		[Display(Name = "Версии видов оформлений")]
		public virtual IList<EmployeeRegistrationVersion> EmployeeRegistrationVersions
		{
			get => _employeeRegistrationVersions;
			set => SetField(ref _employeeRegistrationVersions, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeRegistrationVersion> ObservableEmployeeRegistrationVersions =>
			_observableEmployeeRegistrationVersions ?? (_observableEmployeeRegistrationVersions =
				new GenericObservableList<EmployeeRegistrationVersion>(EmployeeRegistrationVersions));

        #endregion

        public Employee()
		{
			Name = String.Empty;
			LastName = String.Empty;
			Patronymic = String.Empty;
			DrivingLicense = String.Empty;
			Category = EmployeeCategory.office;
			AddressRegistration = String.Empty;
			AddressCurrent = String.Empty;
		}

		#region IValidatableObject implementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(typeof(IEmployeeRepository)) is IEmployeeRepository employeeRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(employeeRepository) }");
			}

			if(!(validationContext.ServiceContainer.GetService(typeof(ISubdivisionParametersProvider)) is ISubdivisionParametersProvider
				   subdivisionParametersProvider))
			{
				throw new ArgumentNullException($"Не найден сервис { nameof(subdivisionParametersProvider) }");
			}

			if(!(validationContext.ServiceContainer.GetService(typeof(IUserRepository)) is IUserRepository userRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(userRepository) }");
			}

			foreach(var item in base.Validate(validationContext)) {
				yield return item;
			}

			if(!string.IsNullOrEmpty(AndroidLogin))
			{
				var exist = employeeRepository.GetDriverByAndroidLogin(UoW, AndroidLogin);

				if(exist != null && exist.Id != Id)
				{
					yield return new ValidationResult(
						$"Другой водитель с логином { AndroidLogin } для Android уже есть в БД.",
						new[] { nameof(AndroidLogin) });
				}
			}

			if(!String.IsNullOrEmpty(LoginForNewUser) && User != null) {
				yield return new ValidationResult($"Сотрудник уже привязан к пользователю",
					new[] { nameof(LoginForNewUser) });
			}
			var regex = new Regex(@"^[a-zA-Z0-9].{3,25}$");
			if(!String.IsNullOrEmpty(LoginForNewUser) && !regex.IsMatch(LoginForNewUser)) {
				yield return new ValidationResult($"Логин пользователя должен иметь длину от 3 до 25 символов и состоять из латинских букв и цифр",
					new[] { nameof(LoginForNewUser) });
			}
			if(!String.IsNullOrEmpty(LoginForNewUser)) {
				User exist = userRepository.GetUserByLogin(UoW, LoginForNewUser);
				if(exist != null && exist.Id != Id)
				{
					yield return new ValidationResult($"Пользователь с логином {LoginForNewUser} уже существует в базе",
						new[] { nameof(LoginForNewUser) });
				}
			}

			if(!String.IsNullOrEmpty(LoginForNewUser)) {
				string mes = null;
				bool userExists = false;

				try
				{
					userExists = userRepository.MySQLUserWithLoginExists(UoW, LoginForNewUser);
				}
				catch(HibernateException ex)
				{
					if(ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1142)
					{
						mes = $"У вас недостаточно прав для создания нового пользователя";
					}
					else
					{
						throw;
					}
				}
				if(!String.IsNullOrWhiteSpace(mes))
				{
					yield return new ValidationResult(mes, new[] { nameof(LoginForNewUser) });
				}
				else if(userExists)
				{
					yield return new ValidationResult($"Пользователь с логином {LoginForNewUser} уже существует на сервере",
						new[] { nameof(LoginForNewUser) });
				}
			}

			if(!String.IsNullOrEmpty(LoginForNewUser) && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_users")) {
				yield return new ValidationResult($"Недостаточно прав для создания нового пользователя",
					new[] { nameof(LoginForNewUser) });
			}

			if(Status == EmployeeStatus.IsFired && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_fire_employees")) {
				yield return new ValidationResult($"Недостаточно прав для увольнения сотрудников",
						new[] { nameof(Status) });
			}

			if(!String.IsNullOrEmpty(LoginForNewUser)) {
				string exist = GetPhoneForSmsNotification();
				if(exist == null)
					yield return new ValidationResult($"Для создания пользователя должен быть правильно указан мобильный телефон",
							new[] { nameof(LoginForNewUser) });
				if (String.IsNullOrEmpty(Email)) {
					yield return new ValidationResult($"Для создания пользователя должен быть правильно указан e-mail адрес",
							new[] { nameof(Email) });
				}
			}

			if(Category == EmployeeCategory.driver)
			{
				if(DriverOfCarTypeOfUse == null || DriverOfCarOwnType == null)
				{
					yield return new ValidationResult(@"Обязательно должны быть выбраны поля 'Управляет а\м' для типа и принадлежности авто",
						new[] { nameof(DriverOfCarTypeOfUse), nameof(DriverOfCarOwnType) });
				}
			}

			if(Subdivision == null || Subdivision.Id == subdivisionParametersProvider.GetParentVodovozSubdivisionId())
			{
				yield return new ValidationResult("Поле подразделение должно быть заполнено и не должно являться" +
					" общим подразделением 'Веселый Водовоз'");
			}

			List<EmployeeDocument> mainDocuments = GetMainDocuments();
			if(mainDocuments.Count <= 0 && !IsDriverForOneDay)
				yield return new ValidationResult(String.Format("У сотрудника должен присутствовать главный документ"),
					new[] { this.GetPropertyName(x => x.Documents) });

			if(mainDocuments.Count > 1)
				yield return new ValidationResult(String.Format("Сотрудник может иметь только один главный документ"),
					new[] { this.GetPropertyName(x => x.Documents) });

			if(String.IsNullOrEmpty(DrivingLicense) && IsDriverForOneDay)
				yield return new ValidationResult(String.Format("У разового водителя должно быть водительское удостоверение"),
					new[] { this.GetPropertyName(x => x.DrivingLicense) });
			
			if(Comment != null && Comment.Length > _commentLimit)
			{
				yield return new ValidationResult($"Длина комментария превышена на {Comment.Length - _commentLimit}",
					new[] { nameof(Comment) });
			}
		}

		#endregion

		#region Функции

		public virtual IWageCalculationRepository WageCalculationRepository { get; set; } = new WageCalculationRepository();

		public virtual string GetPersonNameWithInitials() => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);

		public virtual double TimeCorrection(long timeValue) => (double)timeValue / DriverSpeed;

		public virtual bool CheckStartDateForNewWageParameter(DateTime newStartDate)
		{
			WageParameter oldWageParameter = ObservableWageParameters.FirstOrDefault(x => x.EndDate == null);
			if(oldWageParameter == null) {
				return true;
			}

			return oldWageParameter.StartDate < newStartDate;
		}

		public virtual void ChangeWageParameter(EmployeeWageParameter wageParameter, DateTime startDate)
		{
			if(wageParameter == null) {
				throw new ArgumentNullException(nameof(wageParameter));
			}

			wageParameter.Employee = this;
			wageParameter.StartDate = startDate;
			WageParameter oldWageParameter = ObservableWageParameters.FirstOrDefault(x => x.EndDate == null);
			if(oldWageParameter != null) {
				if(oldWageParameter.StartDate > startDate) {
					throw new InvalidOperationException("Нельзя создать новую запись с датой более ранней уже существующей записи. Неверно выбрана дата");
				}
				oldWageParameter.EndDate = startDate.AddMilliseconds(-1);
			}
			ObservableWageParameters.Add(wageParameter);
		}


		public virtual EmployeeWageParameter GetActualWageParameter(DateTime date)
		{
			return WageParameters.Where(x => x.StartDate <= date)
								 .OrderByDescending(x => x.StartDate)
								 .Take(1)
								 .SingleOrDefault();
		}

		public virtual void CreateDefaultWageParameter(IWageCalculationRepository wageRepository, IWageParametersProvider wageParametersProvider, IInteractiveService interactiveService)
		{
			if(wageRepository == null) {
				throw new ArgumentNullException(nameof(wageRepository));
			}
			if(wageParametersProvider == null) {
				throw new ArgumentNullException(nameof(wageParametersProvider));
			}
			if(interactiveService == null) {
				throw new ArgumentNullException(nameof(interactiveService));
			}

			var defaultLevel = wageRepository.DefaultLevelForNewEmployees(UoW);
			if(defaultLevel == null) {
				interactiveService.ShowMessage(ImportanceLevel.Warning, "\"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Найм)\"!\"", "Невозможно создать расчет зарплаты");
				return;
			}

			var defaultLevelForOurCar = wageRepository.DefaultLevelForNewEmployeesOnOurCars(UoW);
			if(defaultLevelForOurCar == null) {
				interactiveService.ShowMessage(ImportanceLevel.Warning, "\"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Для наших авто)\"!\"", "Невозможно создать расчет зарплаты");
				return;
			}

			var defaultLevelForRaskatCar = wageRepository.DefaultLevelForNewEmployeesOnRaskatCars(UoW);
			if(defaultLevelForRaskatCar == null)
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning, "\"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Для авто в раскате)\"!\"", "Невозможно создать расчет зарплаты");
				return;
			}

			if(Id != 0) return;

			ObservableWageParameters.Clear();
			switch(Category) {
				case EmployeeCategory.driver:
					EmployeeWageParameter parameterForDriver = new EmployeeWageParameter {
						WageParameterItem = new ManualWageParameterItem(),
						WageParameterItemForOurCars = new ManualWageParameterItem(),
						WageParameterItemForRaskatCars = new ManualWageParameterItem()
					};
					if(VisitingMaster && !IsDriverForOneDay) {
						parameterForDriver = new EmployeeWageParameter {
							WageParameterItem = new PercentWageParameterItem {
								PercentWageType = PercentWageTypes.Service
							},
							WageParameterItemForOurCars = new PercentWageParameterItem {
								PercentWageType = PercentWageTypes.Service
							},
							WageParameterItemForRaskatCars = new PercentWageParameterItem
							{
								PercentWageType = PercentWageTypes.Service
							}
						};
					}
					else if(!IsDriverForOneDay) {
						parameterForDriver = new EmployeeWageParameter {
							WageParameterItem = new RatesLevelWageParameterItem {
								WageDistrictLevelRates = defaultLevel
							},
							WageParameterItemForOurCars = new RatesLevelWageParameterItem {
								WageDistrictLevelRates = defaultLevelForOurCar
							},
							WageParameterItemForRaskatCars = new RatesLevelWageParameterItem
							{
								WageDistrictLevelRates = defaultLevelForRaskatCar
							}
						};
					}

					ChangeWageParameter(parameterForDriver, DateTime.Today);
					break;
				case EmployeeCategory.forwarder:
					var parameterForForwarder = new EmployeeWageParameter {
						WageParameterItem = new RatesLevelWageParameterItem {
							WageDistrictLevelRates = defaultLevel
						},
						WageParameterItemForOurCars = new RatesLevelWageParameterItem
						{
							WageDistrictLevelRates = defaultLevelForOurCar
						},
						WageParameterItemForRaskatCars = new RatesLevelWageParameterItem
						{
							WageDistrictLevelRates = defaultLevelForRaskatCar
						}
					};
					ChangeWageParameter(parameterForForwarder, DateTime.Today);
					break;
				case EmployeeCategory.office:
				default:
					WageParameterItem wageParameterItem;
					if(Subdivision?.DefaultSalesPlan != null)
					{
						wageParameterItem = new SalesPlanWageParameterItem()
						{
							SalesPlan = Subdivision.DefaultSalesPlan
						};
					}
					else
					{
						wageParameterItem = new ManualWageParameterItem();
					}

					ChangeWageParameter(
						new EmployeeWageParameter
						{
							WageParameterItem = wageParameterItem
						},
						DateTime.Today);
					break;
			}
		}

		public virtual string GetPhoneForSmsNotification()
		{
			string stringPhoneNumber = Phones.FirstOrDefault(p => p?.DigitsNumber != null && p.DigitsNumber.Count() == 10)?.DigitsNumber.TrimStart('+').TrimStart('7').TrimStart('8');
			if(String.IsNullOrWhiteSpace(stringPhoneNumber)
				|| stringPhoneNumber.Length == 0
				|| stringPhoneNumber.First() != '9'
				|| stringPhoneNumber.Length != 10)
				return null;

			return stringPhoneNumber;
		}

		public virtual void AddDriverDistrictPrioritySet(DriverDistrictPrioritySet districtPrioritySet)
		{
			ObservableDriverDistrictPrioritySets.Insert(0, districtPrioritySet);
		}

		public virtual void ActivateDriverDistrictPrioritySet(DriverDistrictPrioritySet driverDistrictPrioritySet, Employee editor)
		{
			var currentActiveSet = ObservableDriverDistrictPrioritySets.SingleOrDefault(x => x.IsActive);

			var now = DateTime.Now;

			if (currentActiveSet != null)
			{
				currentActiveSet.IsActive = false;
				currentActiveSet.DateLastChanged = now;
				currentActiveSet.LastEditor = editor;

				currentActiveSet.DateDeactivated = currentActiveSet.DateActivated.Value.Date > DateTime.Today
					? currentActiveSet.DateActivated.Value.Date.AddDays(1).AddMilliseconds(-1)
					: DateTime.Today.AddDays(1).AddMilliseconds(-1);
			}

			driverDistrictPrioritySet.IsActive = true;
			driverDistrictPrioritySet.DateLastChanged = now;
			driverDistrictPrioritySet.LastEditor = editor;
			driverDistrictPrioritySet.DateActivated 
				= currentActiveSet?.DateDeactivated.Value.Date.AddDays(1) ?? DateTime.Today;
		}
		
		public virtual void AddActiveDriverWorkScheduleSet(DriverWorkScheduleSet activeDriverWorkScheduleSet)
		{
			var currentActiveSet = ObservableDriverWorkScheduleSets.SingleOrDefault(x => x.IsActive);
			if(currentActiveSet != null) {
				currentActiveSet.IsActive = false;
				
				currentActiveSet.DateDeactivated = currentActiveSet.DateActivated.Date > DateTime.Today
					? currentActiveSet.DateActivated.Date.AddDays(1).AddMilliseconds(-1)
					: DateTime.Today.AddDays(1).AddMilliseconds(-1);
			}

			activeDriverWorkScheduleSet.IsActive = true;
			activeDriverWorkScheduleSet.DateActivated = currentActiveSet?.DateDeactivated.Value.Date.AddDays(1) ?? DateTime.Today;
			
			if(ObservableDriverWorkScheduleSets.Any()) {
				ObservableDriverWorkScheduleSets.Insert(0, activeDriverWorkScheduleSet);
			}
			else {
				ObservableDriverWorkScheduleSets.Add(activeDriverWorkScheduleSet);
			}
		}

		#endregion
	}

	public enum EmployeeCategory
	{
		[Display(Name = "Офисный работник")]
		office,
		[Display(Name = "Водитель")]
		driver,
		[Display(Name = "Экспедитор")]
		forwarder
	}

	public enum DriverType
	{
		[Display(Name = "Управляет ТС компании")]
		companydriver,
		[Display(Name = "Управляет ТС в раскате")]
		raskat,
		[Display(Name = "Управляет ТС личным")]
		hireddriver
	}

	public enum EmployeeType
	{
		[Display(Name = "Сотрудник")]
		Employee,
		[Display(Name = "Стажер")]
		Trainee
	}

	public enum EmployeeStatus
	{
		[Display(Name = "Работает")]
		IsWorking,
		[Display(Name = "На расчете")]
		OnCalculation,
		[Display(Name = "В декрете")]
		OnMaternityLeave,
		[Display(Name = "Уволен")]
		IsFired
	}
	
	public enum Gender
	{
		[Display(Name = "М")]
		male,
		[Display(Name = "Ж")]
		female
	}

	public enum DriverTerminalRelation
	{
		[Display(Name = "Водители с терминалами")]
		WithTerminal,
		[Display(Name = "Водители без терминалов")]
		WithoutTerminal
	}

	public class EmployeeCategoryStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeCategoryStringType() : base(typeof(EmployeeCategory))
		{
		}
	}

	public class RegistrationTypeStringType : NHibernate.Type.EnumStringType
	{
		public RegistrationTypeStringType() : base(typeof(RegistrationType))
		{
		}
	}

	public class EmployeeTypeStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeTypeStringType() : base(typeof(EmployeeType))
		{
		}
	}

	public class EmployeeStatusStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeStatusStringType() : base(typeof(EmployeeStatus))
		{
		}
	}	

	public class DriverTypeStringType : NHibernate.Type.EnumStringType
	{
		public DriverTypeStringType() : base(typeof(DriverType))
		{
		}
	}
}
