using Autofac;
using Gamma.Utilities;
using MySqlConnector;
using NHibernate;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Project.Services;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сотрудники",
		Nominative = "сотрудник",
		GenitivePlural = "сотрудников")]
	[EntityPermission]
	[HistoryTrace]
	public class Employee : EmployeeEntity, IBusinessObject, IAccountOwner, IValidatableObject
	{
		private const int _commentLimit = 255;

		private bool _hasAccessToWarehouseApp;

		private Counterparty _counterparty;
		private Citizenship _citizenship;
		private Nationality _nationality;
		private EmployeePost _post;
		private User _user;
		private Subdivision _subdivision;
		private Employee _defaultForwarder;
		private Organization _organisationForSalary;
		private CarTypeOfUse? _driverOfCarTypeOfUse;
		private CarOwnType? _driverOfCarOwnType;

		private IList<Phone> _phones = new List<Phone>();
		private IList<EmployeeDocument> _documents = new List<EmployeeDocument>();
		private IObservableList<Account> _accounts = new ObservableList<Account>();
		private IList<EmployeeContract> _contracts = new List<EmployeeContract>();
		private IList<EmployeeWageParameter> wageParameters = new List<EmployeeWageParameter>();
		private IList<EmployeeRegistrationVersion> _employeeRegistrationVersions = new List<EmployeeRegistrationVersion>();
		private IList<DriverDistrictPrioritySet> _driverDistrictPrioritySets = new List<DriverDistrictPrioritySet>();
		private IList<DriverWorkScheduleSet> _driverWorkScheduleSets = new List<DriverWorkScheduleSet>();
		private IList<ExternalApplicationUser> _externalApplicationsUsers = new List<ExternalApplicationUser>();

		private GenericObservableList<EmployeeDocument> _observableDocuments;
		private GenericObservableList<Account> _observableAccounts;
		private GenericObservableList<Attachment> _observableAttachments;
		private GenericObservableList<EmployeeContract> _observableContracts;
		private GenericObservableList<EmployeeWageParameter> _observableWageParameters;
		private GenericObservableList<EmployeeRegistrationVersion> _observableEmployeeRegistrationVersions;
		private GenericObservableList<DriverDistrictPrioritySet> _observableDriverDistrictPrioritySets;
		private GenericObservableList<DriverWorkScheduleSet> _observableDriverWorkScheduleSets;
		private IWageCalculationRepository _wageCalculationRepository;
		private bool _canRecieveCounterpartyCalls;
		private Phone _phoneForCounterpartyCalls;

		public virtual IUnitOfWork UoW { set; get; }

		[Display(Name = "Иностранное граждансво")]
		public virtual Citizenship Citizenship
		{
			get => _citizenship;
			set => SetField(ref _citizenship, value);
		}

		[Display(Name = "Национальность")]
		public virtual Nationality Nationality
		{
			get => _nationality;
			set => SetField(ref _nationality, value);
		}

		[Display(Name = "Должность")]
		public virtual EmployeePost Post
		{
			get => _post;
			set => SetField(ref _post, value);
		}

		[Display(Name = "Пользователь")]
		public virtual User User
		{
			get { return _user; }
			set { SetField(ref _user, value, () => User); }
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
			get { return _subdivision; }
			set { SetField(ref _subdivision, value, () => Subdivision); }
		}

		[Display(Name = "Экспедитор по умолчанию")]
		public virtual Employee DefaultForwarder
		{
			get { return _defaultForwarder; }
			set { SetField(ref _defaultForwarder, value, () => DefaultForwarder); }
		}

		[Display(Name = "Зарплатная организация")]
		public virtual Organization OrganisationForSalary
		{
			get => _organisationForSalary;
			set => SetField(ref _organisationForSalary, value);
		}
		
		[Display(Name = "Клиент ВВ")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
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

		[Display(Name = "Есть доступ к складскому приложению")]
		public virtual bool HasAccessToWarehouseApp
		{
			get => _hasAccessToWarehouseApp;
			set => SetField(ref _hasAccessToWarehouseApp, value);
		}

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		[Display(Name = "Водитель может принимать звонки от контрагентов")]
		public virtual bool CanRecieveCounterpartyCalls
		{
			get => _canRecieveCounterpartyCalls;
			set => SetField(ref _canRecieveCounterpartyCalls, value);
		}

		[Display(Name = "Телефон для приема звонков от контрагентов")]
		public virtual Phone PhoneForCounterpartyCalls
		{
			get => _phoneForCounterpartyCalls;
			set => SetField(ref _phoneForCounterpartyCalls, value);
		}

		[Display(Name = "Документы")]
		public virtual IList<EmployeeDocument> Documents
		{
			get => _documents;
			set => SetField(ref _documents, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeDocument> ObservableDocuments =>
			_observableDocuments ?? (_observableDocuments = new GenericObservableList<EmployeeDocument>(Documents));

		[Display(Name = "Договора")]
		public virtual IList<EmployeeContract> Contracts
		{
			get { return _contracts; }
			set { SetField(ref _contracts, value); }
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeContract> ObservableContracts =>
			_observableContracts ?? (_observableContracts = new GenericObservableList<EmployeeContract>(Contracts));


		[Display(Name = "Параметры расчета зарплаты")]
		public virtual IList<EmployeeWageParameter> WageParameters
		{
			get => wageParameters;
			set => SetField(ref wageParameters, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeWageParameter> ObservableWageParameters =>
			_observableWageParameters ?? (_observableWageParameters = new GenericObservableList<EmployeeWageParameter>(WageParameters));


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

		[Display(Name = "Версии приоритетов районов водителя")]
		public virtual IList<DriverDistrictPrioritySet> DriverDistrictPrioritySets
		{
			get => _driverDistrictPrioritySets;
			set => SetField(ref _driverDistrictPrioritySets, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DriverDistrictPrioritySet> ObservableDriverDistrictPrioritySets =>
			_observableDriverDistrictPrioritySets ?? (_observableDriverDistrictPrioritySets =
				new GenericObservableList<DriverDistrictPrioritySet>(DriverDistrictPrioritySets));

		[Display(Name = "Версии графиков работы водителя")]
		public virtual IList<DriverWorkScheduleSet> DriverWorkScheduleSets
		{
			get => _driverWorkScheduleSets;
			set => SetField(ref _driverWorkScheduleSets, value);
		}
		
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DriverWorkScheduleSet> ObservableDriverWorkScheduleSets
			=> _observableDriverWorkScheduleSets ?? (_observableDriverWorkScheduleSets =
				new GenericObservableList<DriverWorkScheduleSet>(DriverWorkScheduleSets));

		#region IAccountOwner implementation

		public virtual IObservableList<Account> Accounts
		{
			get => _accounts;
			set => SetField(ref _accounts, value);
		}

		[Display(Name = "Основной счет")]
		public virtual Account DefaultAccount
		{
			get
			{
				return Accounts.FirstOrDefault(x => x.IsDefault);
			}
			set
			{
				Account oldDefAccount = Accounts.FirstOrDefault(x => x.IsDefault);
				if(oldDefAccount != null && value != null && oldDefAccount.Id != value.Id)
				{
					oldDefAccount.IsDefault = false;
				}
				value.IsDefault = true;
			}
		}

		public virtual void AddAccount(Account account)
		{
			Accounts.Add(account);
			account.Owner = this;
			if(DefaultAccount == null)
				account.IsDefault = true;
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.GetService(typeof(IEmployeeRepository)) is IEmployeeRepository employeeRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(employeeRepository)}");
			}

			if(!(validationContext.GetService(typeof(ISubdivisionSettings)) is ISubdivisionSettings
					subdivisionSettings))
			{
				throw new ArgumentNullException($"Не найден сервис {nameof(subdivisionSettings)}");
			}

			if(!(validationContext.GetService(typeof(IUserRepository)) is IUserRepository userRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(userRepository)}");
			}

			if(String.IsNullOrEmpty(LastName))
			{
				yield return new ValidationResult("Фамилия должна быть заполнена", new[] { "LastName" });
			}

			var employees = UoW.Session.QueryOver<Employee>()
				.Where(p => p.Name == this.Name && p.LastName == this.LastName && p.Patronymic == this.Patronymic)
				.WhereNot(p => p.Id == this.Id)
				.List();

			if(employees.Count > 0)
			{
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
				!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanManageUsers))
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
			
			if(CanRecieveCounterpartyCalls && PhoneForCounterpartyCalls == null)
			{
				yield return new ValidationResult(
					"При включенной настройке возможности принимать звонки контрагента - требуется установка телефона для связи с водителем",
					new[] { nameof(CanRecieveCounterpartyCalls), nameof(PhoneForCounterpartyCalls) });
			}

			if(Subdivision == null || Subdivision.Id == subdivisionSettings.GetParentVodovozSubdivisionId())
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

		#region Без маппинга

		public virtual string Title
		{
			get => ShortName;
		}

		#endregion Без маппинга

		#region Функции

		public virtual ExternalApplicationUser DriverAppUser =>
			ExternalApplicationsUsers.SingleOrDefault(x => x.ExternalApplicationType == ExternalApplicationType.DriverApp);
		
		public virtual ExternalApplicationUser WarehouseAppUser =>
			ExternalApplicationsUsers.SingleOrDefault(x => x.ExternalApplicationType == ExternalApplicationType.WarehouseApp);

		public virtual IWageCalculationRepository WageCalculationRepository
		{
			get => _wageCalculationRepository ?? (_wageCalculationRepository = ScopeProvider.Scope.Resolve<IWageCalculationRepository>());
			set => _wageCalculationRepository = value;
		}

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
			IWageSettings wageSettings, IInteractiveService interactiveService)
		{
			if(wageRepository == null)
			{
				throw new ArgumentNullException(nameof(wageRepository));
			}

			if(wageSettings == null)
			{
				throw new ArgumentNullException(nameof(wageSettings));
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

		public virtual List<int> GetSkillLevels() => new List<int> { 0, 1, 2, 3, 4, 5 };

		public virtual List<EmployeeDocument> GetMainDocuments()
		{
			List<EmployeeDocument> mainDocuments = new List<EmployeeDocument>();
			foreach(var doc in Documents)
			{
				if(doc.MainDocument == true)
					mainDocuments.Add(doc);
			}
			return mainDocuments;
		}

		#endregion
	}
}
