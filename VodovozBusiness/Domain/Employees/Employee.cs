using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSBanks;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "сотрудники",
		Nominative = "сотрудник")]
	public class Employee : AccountOwnerBase, IDomainObject, IValidatableObject, ISpecialRowsRender, IBusinessObject
	{
		#region Свойства

		public virtual IUnitOfWork UoW { set; get; }

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Имя")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value?.Trim(), () => Name); }
		}

		string lastName;

		[Display(Name = "Фамилия")]
		public virtual string LastName {
			get { return lastName; }
			set { SetField(ref lastName, value?.Trim(), () => LastName); }
		}

		string patronymic;

		[Display(Name = "Отчество")]
		public virtual string Patronymic {
			get { return patronymic; }
			set { SetField(ref patronymic, value?.Trim(), () => Patronymic); }
		}

		EmployeeCategory category;

		[Display(Name = "Категория")]
		public virtual EmployeeCategory Category {
			get { return category; }
			set { SetField(ref category, value, () => Category); }
		}

		string passportSeria;

		[Display(Name = "Серия паспорта")]
		public virtual string PassportSeria {
			get { return passportSeria; }
			set { SetField(ref passportSeria, value, () => PassportSeria); }
		}

		string passportNumber;

		[Display(Name = "Номер паспорта")]
		public virtual string PassportNumber {
			get { return passportNumber; }
			set { SetField(ref passportNumber, value, () => PassportNumber); }
		}

		string passportIssuedOrg;

		[Display(Name = "Кем выдан паспорт")]
		public virtual string PassportIssuedOrg {
			get { return passportIssuedOrg; }
			set { SetField(ref passportIssuedOrg, value, () => PassportIssuedOrg); }
		}

		private DateTime? passportIssuedDate;

		[Display(Name = "Дата выдачи паспорта")]
		public virtual DateTime? PassportIssuedDate {
			get { return passportIssuedDate; }
			set { SetField(ref passportIssuedDate, value, () => PassportIssuedDate); }
		}

		string drivingNumber;

		[Display(Name = "Водительское удостоверение")]
		public virtual string DrivingNumber {
			get { return drivingNumber; }
			set { SetField(ref drivingNumber, value, () => DrivingNumber); }
		}

		string addressRegistration;

		[Display(Name = "Адрес регистрации")]
		public virtual string AddressRegistration {
			get { return addressRegistration; }
			set { SetField(ref addressRegistration, value, () => AddressRegistration); }
		}

		string addressCurrent;

		[Display(Name = "Фактический адрес")]
		public virtual string AddressCurrent {
			get { return addressCurrent; }
			set { SetField(ref addressCurrent, value, () => AddressCurrent); }
		}

		string inn;

		[Display(Name = "ИНН")]
		public virtual string INN {
			get { return inn; }
			set { SetField(ref inn, value, () => INN); }
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

		bool isFired;

		[Display(Name = "Сотрудник уволен")]
		public virtual bool IsFired {
			get { return isFired; }
			set { SetField(ref isFired, value, () => IsFired); }
		}

		IList<QSContacts.Phone> phones;

		[Display(Name = "Телефоны")]
		public virtual IList<QSContacts.Phone> Phones {
			get { return phones; }
			set { SetField(ref phones, value, () => Phones); }
		}

		Nationality nationality;

		[Display(Name = "Национальность")]
		public virtual Nationality Nationality {
			get { return nationality; }
			set { SetField(ref nationality, value, () => Nationality); }
		}

		User user;

		[Display(Name = "Пользователь")]
		public virtual User User {
			get { return user; }
			set { SetField(ref user, value, () => User); }
		}

		byte[] photo;

		[Display(Name = "Фотография")]
		public virtual byte[] Photo {
			get { return photo; }
			set { SetField(ref photo, value, () => Photo); }
		}

		private DateTime dateOfCreate;

		[Display(Name = "Дата создания")]
		public virtual DateTime DateOfCreate {
			get { return dateOfCreate; }
			set { SetField(ref dateOfCreate, value, () => DateOfCreate); }
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

		private DeliveryDaySchedule defaultDaySheldule;

		[Display(Name = "График работы по молчанию")]
		public virtual DeliveryDaySchedule DefaultDaySheldule {
			get { return defaultDaySheldule; }
			set { SetField(ref defaultDaySheldule, value, () => DefaultDaySheldule); }
		}

		private Employee defaultForwarder;

		[Display(Name = "Экспедитор по умолчанию")]
		public virtual Employee DefaultForwarder {
			get { return defaultForwarder; }
			set { SetField(ref defaultForwarder, value, () => DefaultForwarder); }
		}

		private bool largusDriver;

		[Display(Name = "Сотрудник - водитель Ларгуса")]
		public virtual bool LargusDriver{
			get { return largusDriver; }
			set {SetField(ref largusDriver, value, () => LargusDriver);}
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

		private IList<DriverDistrictPriority> districts = new List<DriverDistrictPriority>();

		[Display(Name = "Районы")]
		public virtual IList<DriverDistrictPriority> Districts {
			get { return districts; }
			set { SetField(ref districts, value, () => Districts); }
		}

		GenericObservableList<DriverDistrictPriority> observableDistricts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DriverDistrictPriority> ObservableDistricts {
			get {
				if(observableDistricts == null) {
					observableDistricts = new GenericObservableList<DriverDistrictPriority>(districts);
					observableDistricts.ElementAdded += ObservableDistricts_ElementAdded;
					observableDistricts.ElementRemoved += ObservableDistricts_ElementRemoved;
				}
				return observableDistricts;
			}
		}

		WageCalculationType wageCalcType;

		[Display(Name = "Тип расчёта зарплаты")]
		public virtual WageCalculationType WageCalcType
		{
			get { return wageCalcType; }
			set { SetField(ref wageCalcType, value, () => WageCalcType);}
		}

		decimal wageCalcRate;

		[Display(Name = "Ставка для расчёта зарплаты")]
		public virtual decimal WageCalcRate
		{
			get { return wageCalcRate; }
			set { SetField(ref wageCalcRate, value, () => WageCalcRate);}
		}

		bool visitingMaster;

		public virtual bool VisitingMaster
		{
			get { return visitingMaster; }
			set { SetField(ref visitingMaster, value, () => VisitingMaster);}
		}

		#endregion

		public Employee()
		{
			Name = String.Empty;
			LastName = String.Empty;
			Patronymic = String.Empty;
			PassportSeria = String.Empty;
			PassportNumber = String.Empty;
			DrivingNumber = String.Empty;
			Category = EmployeeCategory.office;
			AddressRegistration = String.Empty;
			AddressCurrent = String.Empty;
			DateOfCreate = DateTime.Now;
		}

		[Display(Name = "ФИО")]
		public virtual string FullName {
			get { return String.Format("{0} {1} {2}", LastName, Name, Patronymic); }
		}

		[Display(Name = "Фамилия и инициалы")]
		public virtual string ShortName {
			get { return StringWorks.PersonNameWithInitials(LastName, Name, Patronymic); }
		}

		public virtual string Title {
			get { return ShortName; }
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(LastName))
				yield return new ValidationResult("Фамилия должна быть заполнена", new[] { "LastName" });

#if !SHORT
			if (String.IsNullOrEmpty (Name))
				yield return new ValidationResult ("Имя дожно быть заполнено", new[] { "Name" });

			if (String.IsNullOrEmpty (Patronymic))
				yield return new ValidationResult ("Отчество должно быть заполнено", new[] { "Patronymic" });

			if(Subdivision == null)
				yield return new ValidationResult ("Подразделение должно быть заполнено", new[] { "Subdivision" });

			if(Phones.Count <= 0 || Phones.FirstOrDefault(p => p.DigitsNumber != string.Empty) == null)
				yield return new ValidationResult ("Должен быть заполнен хотя бы один телефон", new[] { "Phones" });

			if(string.IsNullOrWhiteSpace(PassportNumber))
				yield return new ValidationResult ("Номер паспорта должен быть заполнен", new[] { "PassportNumber" });

			if(string.IsNullOrWhiteSpace(PassportSeria))
				yield return new ValidationResult ("Серия паспорта должна быть заполнена", new[] { "PassportSeria" });

			if(string.IsNullOrWhiteSpace(AddressRegistration))
				yield return new ValidationResult ("Адрес регистрации должен быть заполнен", new[] { "AddressRegistration" });

			if(string.IsNullOrWhiteSpace(AddressCurrent))
				yield return new ValidationResult ("Фактический адрес должен быть заполнен", new[] { "AddressCurrent" });

#endif

			var employees = UoW.Session.QueryOver<Employee>()
				.Where(e => e.Name == this.Name && e.LastName == this.LastName && e.Patronymic == this.Patronymic)
				.WhereNot(e => e.Id == this.Id)
				.List();

			if(employees.Count > 0)
				yield return new ValidationResult("Сотрудник уже существует", new[] { "Duplication" });

			if(!String.IsNullOrEmpty(AndroidLogin)) {
				Employee exist = Repository.EmployeeRepository.GetDriverByAndroidLogin(UoW, AndroidLogin);
				if(exist != null && exist.Id != Id)
					yield return new ValidationResult(String.Format("Другой водитель с логином {0} для Android уже есть в БД.", AndroidLogin),
						new[] { this.GetPropertyName(x => x.AndroidLogin) });
			}
		}

		#endregion

		#region ISpecialRowsRender implementation

		public virtual string TextColor { get { return IsFired ? "grey" : "black"; } }

		#endregion

		#region Функции 
		private void CheckDistrictsPriorities()
		{
			for(int i = 0; i < Districts.Count; i++) {
				if(Districts[i] == null) {
					Districts.RemoveAt(i);
					i--;
					continue;
				}

				if(Districts[i].Priority != i)
					Districts[i].Priority = i;
			}
		}

		public virtual double TimeCorrection(long timeValue)
		{
			return (double)timeValue / DriverSpeed;
		}

		#endregion

		void ObservableDistricts_ElementAdded(object aList, int[] aIdx)
		{
			CheckDistrictsPriorities();
		}

		void ObservableDistricts_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			CheckDistrictsPriorities();
		}
	}

	public enum EmployeeCategory
	{
		[Display (Name = "Офисный работник")]
		office,
		[Display (Name = "Водитель")]
		driver,
		[Display (Name = "Экспедитор")]
		forwarder
	}

	public enum WageCalculationType
	{
		[Display(Name = "Обычный")]
		normal,
		[Display(Name = "Процент от стоимости")]
		percentage,
		[Display(Name = "Процент от стоимости (СЦ)")]
		percentageForService,
		[Display(Name = "Фиксированная ставка за МЛ")]
		fixedRoute,
		[Display(Name = "Фиксированная ставка за день")]
		fixedDay
	}

	public class EmployeeCategoryStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeCategoryStringType () : base (typeof(EmployeeCategory))
		{
		}
	}
}

