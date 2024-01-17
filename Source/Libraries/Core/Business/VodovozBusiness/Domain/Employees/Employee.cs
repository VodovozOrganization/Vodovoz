using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.Utilities;
using MySqlConnector;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
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
		private uint? _innerPhone;
		private CarOwnType? _driverOfCarOwnType;
		private EmployeeStatus _status;
		private User _user;
		private Subdivision _subdivision;
		private DateTime? _firstWorkDay;
		private DateTime? _dateHired;
		private DateTime? _dateFired;
		private DateTime? _dateCalculated;
		private string _comment;
		private EmployeeCategory _category;
		private Employee _defaultForwarder;
		private DriverType? _driverType;
		private bool _largusDriver;
		private CarTypeOfUse? _driverOfCarTypeOfUse;
		private Gender? _gender;
		private float _driverSpeed = 1;
		private short _tripPriority = 6;
		private int _minRouteAddresses;
		private int _maxRouteAddresses;
		private bool _visitingMaster;
		private bool _isChainStoreDriver;
		private bool _isDriverForOneDay;
		private string _loginForNewUser;
		private Organization _organisationForSalary;
		private string _email;
		private bool _hasAccessToWarehouseApp;

		private IList<EmployeeContract> _contracts = new List<EmployeeContract>();
		private GenericObservableList<EmployeeContract> _observableContracts;
		private IList<DriverDistrictPrioritySet> _driverDistrictPrioritySets = new List<DriverDistrictPrioritySet>();
		private GenericObservableList<DriverDistrictPrioritySet> _observableDriverDistrictPrioritySets;
		private IList<DriverWorkScheduleSet> _driverWorkScheduleSets = new List<DriverWorkScheduleSet>();
		private GenericObservableList<DriverWorkScheduleSet> _observableDriverWorkScheduleSets;
		private IList<EmployeeRegistrationVersion> _employeeRegistrationVersions = new List<EmployeeRegistrationVersion>();
		private GenericObservableList<EmployeeRegistrationVersion> _observableEmployeeRegistrationVersions;
		private IList<EmployeeWageParameter> _wageParameters = new List<EmployeeWageParameter>();
		private GenericObservableList<EmployeeWageParameter> _observableWageParameters;
		private IList<ExternalApplicationUser> _externalApplicationsUsers = new List<ExternalApplicationUser>();

		#region Свойства

		public override EmployeeType EmployeeType => EmployeeType.Employee;

		[Display(Name = "Категория")]
		public virtual EmployeeCategory Category
		{
			get => _category;
			set => SetField(ref _category, value);
		}

		[Display(Name = "Внутренний номер")]
		public virtual uint? InnerPhone
		{
			get => _innerPhone;
			set => SetField(ref _innerPhone, value);
		}

		[Display(Name = "Статус сотрудника")]
		public virtual EmployeeStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Пользователь")]
		public virtual User User
		{
			get => _user;
			set => SetField(ref _user, value);
		}
		
		[Display(Name = "Пользователи внешних приложений")]
		public virtual IList<ExternalApplicationUser> ExternalApplicationsUsers
		{
			get => _externalApplicationsUsers;
			set => SetField(ref _externalApplicationsUsers, value);
		}

		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		[Display(Name = "Первый день работы")]
		public virtual DateTime? FirstWorkDay
		{
			get => _firstWorkDay;
			set => SetField(ref _firstWorkDay, value);
		}

		[Display(Name = "Дата приема")]
		public virtual DateTime? DateHired
		{
			get => _dateHired;
			set => SetField(ref _dateHired, value);
		}

		[Display(Name = "Дата увольнения")]
		public virtual DateTime? DateFired
		{
			get => _dateFired;
			set => SetField(ref _dateFired, value);
		}

		[Display(Name = "Дата расчета")]
		public virtual DateTime? DateCalculated
		{
			get => _dateCalculated;
			set => SetField(ref _dateCalculated, value);
		}

		[Display(Name = "Договора")]
		public virtual IList<EmployeeContract> Contracts
		{
			get => _contracts;
			set => SetField(ref _contracts, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeContract> ObservableContracts
		{
			get
			{
				if(_observableContracts == null)
				{
					_observableContracts = new GenericObservableList<EmployeeContract>(Contracts);
				}

				return _observableContracts;
			}
		}

		[Display(Name = "Экспедитор по умолчанию")]
		public virtual Employee DefaultForwarder
		{
			get => _defaultForwarder;
			set => SetField(ref _defaultForwarder, value);
		}

		public virtual DriverType? DriverType
		{
			get => _driverType;
			set => SetField(ref _driverType, value);
		}

		[Display(Name = "Сотрудник - водитель Ларгуса")]
		public virtual bool LargusDriver
		{
			get => _largusDriver;
			set => SetField(ref _largusDriver, value);
		}

		[Display(Name = "Водитель автомобиля типа")]
		public virtual CarTypeOfUse? DriverOfCarTypeOfUse
		{
			get => _driverOfCarTypeOfUse;
			set => SetField(ref _driverOfCarTypeOfUse, value);
		}

		[Display(Name = "Водитель автомобиля принадлежности")]
		public virtual CarOwnType? DriverOfCarOwnType
		{
			get => _driverOfCarOwnType;
			set => SetField(ref _driverOfCarOwnType, value);
		}

		[Display(Name = "Пол сотрудника")]
		public virtual Gender? Gender
		{
			get => _gender;
			set => SetField(ref _gender, value);
		}

		[Display(Name = "Скорость работы водителя")]
		public virtual float DriverSpeed
		{
			get => _driverSpeed;
			set => SetField(ref _driverSpeed, value);
		}

		/// <summary>
		/// Приорите(1-10) чем меньше тем лучше. Фактически это штраф.
		/// </summary>
		[Display(Name = "Приоритет для маршрутов")]
		public virtual short TripPriority
		{
			get => _tripPriority;
			set => SetField(ref _tripPriority, value);
		}

		[Display(Name = "Минимум адресов")]
		public virtual int MinRouteAddresses
		{
			get => _minRouteAddresses;
			set => SetField(ref _minRouteAddresses, value);
		}

		[Display(Name = "Максимум адресов")]
		public virtual int MaxRouteAddresses
		{
			get => _maxRouteAddresses;
			set => SetField(ref _maxRouteAddresses, value);
		}

		#region DriverDistrictPrioritySets

		[Display(Name = "Версии приоритетов районов водителя")]
		public virtual IList<DriverDistrictPrioritySet> DriverDistrictPrioritySets
		{
			get => _driverDistrictPrioritySets;
			set => SetField(ref _driverDistrictPrioritySets, value);
		}

		public virtual GenericObservableList<DriverDistrictPrioritySet> ObservableDriverDistrictPrioritySets =>
			_observableDriverDistrictPrioritySets ?? (_observableDriverDistrictPrioritySets =
				new GenericObservableList<DriverDistrictPrioritySet>(DriverDistrictPrioritySets));

		#endregion

		#region ObservableDriverWorkScheduleSets

		[Display(Name = "Версии графиков работы водителя")]
		public virtual IList<DriverWorkScheduleSet> DriverWorkScheduleSets
		{
			get => _driverWorkScheduleSets;
			set => SetField(ref _driverWorkScheduleSets, value);
		}

		public virtual GenericObservableList<DriverWorkScheduleSet> ObservableDriverWorkScheduleSets
			=> _observableDriverWorkScheduleSets ?? (_observableDriverWorkScheduleSets =
				new GenericObservableList<DriverWorkScheduleSet>(DriverWorkScheduleSets));

		#endregion

		[Display(Name = "Параметры расчета зарплаты")]
		public virtual IList<EmployeeWageParameter> WageParameters
		{
			get => _wageParameters;
			set => SetField(ref _wageParameters, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeWageParameter> ObservableWageParameters
		{
			get
			{
				if(_observableWageParameters == null)
					_observableWageParameters = new GenericObservableList<EmployeeWageParameter>(WageParameters);
				return _observableWageParameters;
			}
		}

		[Display(Name = "Выездной мастер")]
		public virtual bool VisitingMaster
		{
			get => _visitingMaster;
			set => SetField(ref _visitingMaster, value);
		}

		[Display(Name = "Водитель для сетей")]
		public virtual bool IsChainStoreDriver
		{
			get => _isChainStoreDriver;
			set => SetField(ref _isChainStoreDriver, value);
		}

		public virtual bool IsDriverForOneDay
		{
			get => _isDriverForOneDay;
			set => SetField(ref _isDriverForOneDay, value);
		}

		[Display(Name = "Логин нового пользователя")]
		public virtual string LoginForNewUser
		{
			get => _loginForNewUser;
			set => SetField(ref _loginForNewUser, value);
		}

		public virtual Organization OrganisationForSalary
		{
			get => _organisationForSalary;
			set => SetField(ref _organisationForSalary, value);
		}

		[Display(Name = "Электронная почта пользователя")]
		public virtual string Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}

		[Display(Name = "Комментарий по сотруднику")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}
		
		[Display(Name = "Есть доступ к складскому приложению")]
		public virtual bool HasAccessToWarehouseApp
		{
			get => _hasAccessToWarehouseApp;
			set => SetField(ref _hasAccessToWarehouseApp, value);
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
				throw new ArgumentNullException($"Не найден репозиторий {nameof(employeeRepository)}");
			}

			if(!(validationContext.ServiceContainer.GetService(typeof(ISubdivisionParametersProvider)) is ISubdivisionParametersProvider
					subdivisionParametersProvider))
			{
				throw new ArgumentNullException($"Не найден сервис {nameof(subdivisionParametersProvider)}");
			}

			if(!(validationContext.ServiceContainer.GetService(typeof(IUserRepository)) is IUserRepository userRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(userRepository)}");
			}

			foreach(var item in base.Validate(validationContext))
			{
				yield return item;
			}

			if(ExternalApplicationsUsers.Any())
			{
				var login = ExternalApplicationsUsers.First().Login;
				var exist = employeeRepository.GetEmployeeByAndroidLogin(UoW, login);

				if(exist != null && exist.Id != Id)
				{
					yield return new ValidationResult(
						$"Пользователь с таким логином {login} уже есть в БД");
				}
			}

			if(!String.IsNullOrEmpty(LoginForNewUser) && User != null)
			{
				yield return new ValidationResult($"Сотрудник уже привязан к пользователю",
					new[] { nameof(LoginForNewUser) });
			}

			var regex = new Regex(@"^[A-Za-z\d.,_-]+\Z");
			if(!string.IsNullOrEmpty(LoginForNewUser) && !regex.IsMatch(LoginForNewUser))
			{
				yield return new ValidationResult(
					"Логин может состоять только из букв английского алфавита, нижнего подчеркивания, дефиса, точки и запятой",
					new[] { nameof(LoginForNewUser) });
			}

			if(!String.IsNullOrEmpty(LoginForNewUser))
			{
				User exist = userRepository.GetUserByLogin(UoW, LoginForNewUser);
				if(exist != null && exist.Id != Id)
				{
					yield return new ValidationResult($"Пользователь с логином {LoginForNewUser} уже существует в базе",
						new[] { nameof(LoginForNewUser) });
				}
			}

			if(!String.IsNullOrEmpty(LoginForNewUser))
			{
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

			if(!String.IsNullOrEmpty(LoginForNewUser) &&
				!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_users"))
			{
				yield return new ValidationResult($"Недостаточно прав для создания нового пользователя",
					new[] { nameof(LoginForNewUser) });
			}

			if(Status == EmployeeStatus.IsFired &&
				!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_fire_employees"))
			{
				yield return new ValidationResult($"Недостаточно прав для увольнения сотрудников",
					new[] { nameof(Status) });
			}

			if(!String.IsNullOrEmpty(LoginForNewUser))
			{
				string exist = GetPhoneForSmsNotification();
				if(exist == null)
					yield return new ValidationResult($"Для создания пользователя должен быть правильно указан мобильный телефон",
						new[] { nameof(LoginForNewUser) });
				if(String.IsNullOrEmpty(Email))
				{
					yield return new ValidationResult($"Для создания пользователя должен быть правильно указан e-mail адрес",
						new[] { nameof(Email) });
				}
			}

			if(Category == EmployeeCategory.driver)
			{
				if(DriverOfCarTypeOfUse == null || DriverOfCarOwnType == null)
				{
					yield return new ValidationResult(
						@"Обязательно должны быть выбраны поля 'Управляет а\м' для типа и принадлежности авто",
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

			if(FirstWorkDay == null)
			{
				yield return new ValidationResult($"Не указана дата первого рабочего дня сотрудника",
					new[] { nameof(FirstWorkDay) });
			}

			if(DateHired == null)
			{
				yield return new ValidationResult($"Не указана дата приема сотрудника",
					new[] { nameof(DateHired) });
			}

			var emailRegEx = @"^[a-zA-Z0-9]+([\._-]?[a-zA-Z0-9]+)*@[a-zA-Z0-9]+([\.-]?[a-zA-Z0-9]+)*(\.[a-zA-Z]{2,10})+$";

			if(!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, emailRegEx))
			{
				yield return new ValidationResult($"Неверно указан email", new[] { nameof(Email) });
			}
		}

		#endregion

		#region Функции

		public virtual ExternalApplicationUser DriverAppUser =>
			ExternalApplicationsUsers.SingleOrDefault(x => x.ExternalApplicationType == ExternalApplicationType.DriverApp);
		
		public virtual ExternalApplicationUser WarehouseAppUser =>
			ExternalApplicationsUsers.SingleOrDefault(x => x.ExternalApplicationType == ExternalApplicationType.WarehouseApp);
		
		public virtual IWageCalculationRepository WageCalculationRepository { get; set; } = new WageCalculationRepository();

		public virtual string GetPersonNameWithInitials() => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);

		public virtual double TimeCorrection(long timeValue) => (double)timeValue / DriverSpeed;

		public virtual bool CheckStartDateForNewWageParameter(DateTime newStartDate)
		{
			WageParameter oldWageParameter = ObservableWageParameters.FirstOrDefault(x => x.EndDate == null);
			if(oldWageParameter == null)
			{
				return true;
			}

			return oldWageParameter.StartDate < newStartDate;
		}

		public virtual void ChangeWageParameter(EmployeeWageParameter wageParameter, DateTime startDate)
		{
			if(wageParameter == null)
			{
				throw new ArgumentNullException(nameof(wageParameter));
			}

			wageParameter.Employee = this;
			wageParameter.StartDate = startDate;
			WageParameter oldWageParameter = ObservableWageParameters.FirstOrDefault(x => x.EndDate == null);
			if(oldWageParameter != null)
			{
				if(oldWageParameter.StartDate > startDate)
				{
					throw new InvalidOperationException(
						"Нельзя создать новую запись с датой более ранней уже существующей записи. Неверно выбрана дата");
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

		public virtual void CreateDefaultWageParameter(IWageCalculationRepository wageRepository,
			IWageParametersProvider wageParametersProvider, IInteractiveService interactiveService)
		{
			if(wageRepository == null)
			{
				throw new ArgumentNullException(nameof(wageRepository));
			}

			if(wageParametersProvider == null)
			{
				throw new ArgumentNullException(nameof(wageParametersProvider));
			}

			if(interactiveService == null)
			{
				throw new ArgumentNullException(nameof(interactiveService));
			}

			var defaultLevel = wageRepository.DefaultLevelForNewEmployees(UoW);
			if(defaultLevel == null)
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning,
					"\"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Найм)\"!\"",
					"Невозможно создать расчет зарплаты");
				return;
			}

			var defaultLevelForOurCar = wageRepository.DefaultLevelForNewEmployeesOnOurCars(UoW);
			if(defaultLevelForOurCar == null)
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning,
					"\"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Для наших авто)\"!\"",
					"Невозможно создать расчет зарплаты");
				return;
			}

			var defaultLevelForRaskatCar = wageRepository.DefaultLevelForNewEmployeesOnRaskatCars(UoW);
			if(defaultLevelForRaskatCar == null)
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning,
					"\"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Для авто в раскате)\"!\"",
					"Невозможно создать расчет зарплаты");
				return;
			}

			if(Id != 0) return;

			ObservableWageParameters.Clear();
			switch(Category)
			{
				case EmployeeCategory.driver:
					EmployeeWageParameter parameterForDriver = new EmployeeWageParameter
					{
						WageParameterItem = new ManualWageParameterItem(),
						WageParameterItemForOurCars = new ManualWageParameterItem(),
						WageParameterItemForRaskatCars = new ManualWageParameterItem()
					};
					if(VisitingMaster && !IsDriverForOneDay)
					{
						parameterForDriver = new EmployeeWageParameter
						{
							WageParameterItem = new PercentWageParameterItem
							{
								PercentWageType = PercentWageTypes.Service
							},
							WageParameterItemForOurCars = new PercentWageParameterItem
							{
								PercentWageType = PercentWageTypes.Service
							},
							WageParameterItemForRaskatCars = new PercentWageParameterItem
							{
								PercentWageType = PercentWageTypes.Service
							}
						};
					}
					else if(!IsDriverForOneDay)
					{
						parameterForDriver = new EmployeeWageParameter
						{
							WageParameterItem = new RatesLevelWageParameterItem
							{
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
					}

					ChangeWageParameter(parameterForDriver, DateTime.Today);
					break;
				case EmployeeCategory.forwarder:
					var parameterForForwarder = new EmployeeWageParameter
					{
						WageParameterItem = new RatesLevelWageParameterItem
						{
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
			string stringPhoneNumber = Phones.FirstOrDefault(p => p?.DigitsNumber != null && p.DigitsNumber.Count() == 10)?.DigitsNumber
				.TrimStart('+').TrimStart('7').TrimStart('8');
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

			if(currentActiveSet != null)
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
			if(currentActiveSet != null)
			{
				currentActiveSet.IsActive = false;

				currentActiveSet.DateDeactivated = currentActiveSet.DateActivated.Date > DateTime.Today
					? currentActiveSet.DateActivated.Date.AddDays(1).AddMilliseconds(-1)
					: DateTime.Today.AddDays(1).AddMilliseconds(-1);
			}

			activeDriverWorkScheduleSet.IsActive = true;
			activeDriverWorkScheduleSet.DateActivated = currentActiveSet?.DateDeactivated.Value.Date.AddDays(1) ?? DateTime.Today;

			if(ObservableDriverWorkScheduleSets.Any())
			{
				ObservableDriverWorkScheduleSets.Insert(0, activeDriverWorkScheduleSet);
			}
			else
			{
				ObservableDriverWorkScheduleSets.Add(activeDriverWorkScheduleSet);
			}
		}

		public virtual bool IsDriverHasActiveStopListRemoval(IUnitOfWork unitOfWork)
		{
			return unitOfWork.GetAll<DriverStopListRemoval>()
				.Where(r =>
					r.Driver.Id == Id
					&& r.DateFrom <= DateTime.Now
					&& r.DateTo > DateTime.Now)
				.Any();
		}

		#endregion
	}
}
