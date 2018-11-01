using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сотрудники",
		Nominative = "сотрудник")]
	public class Employee : Personnel, ISpecialRowsRender, IEmployee
	{
		#region Свойства

		public override EmployeeType EmployeeType { 
			get { return EmployeeType.Employee; }
			set {}
		}

		EmployeeCategory category;

		[Display(Name = "Категория")]
		public virtual EmployeeCategory Category {
			get { return category; }
			set { SetField(ref category, value, () => Category); }
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

		private CarTypeOfUse? driverOf;
		[Display(Name = "Водитель автомобиля типа")]
		public virtual CarTypeOfUse? DriverOf {
			get { return driverOf; }
			set { SetField(ref driverOf, value, () => DriverOf); }
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

		WageCalculationType? wageCalcType;

		[Display(Name = "Тип расчёта зарплаты")]
		public virtual WageCalculationType? WageCalcType
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
		}

		#region IValidatableObject implementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			base.Validate(validationContext);

			if(!String.IsNullOrEmpty(AndroidLogin)) {
				Employee exist = Repository.EmployeeRepository.GetDriverByAndroidLogin(UoW, AndroidLogin);
				if(exist != null && exist.Id != Id)
					yield return new ValidationResult(String.Format("Другой водитель с логином {0} для Android уже есть в БД.", AndroidLogin),
						new[] { this.GetPropertyName(x => x.AndroidLogin) });
			}

			if(Category == EmployeeCategory.driver && !WageCalcType.HasValue) 
				yield return new ValidationResult("Для водителя необходимо указать тип расчёта заработной платы.", new[] { this.GetPropertyName(x => x.WageCalcType) });

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

	public enum EmployeeType
	{
		[Display(Name = "Сотрудник")]
		Employee,
		[Display(Name = "Стажер")]
		Trainee
	}

	public enum WageCalculationType
	{
		[Display(Name = "Обычный")]
		normal,
		[Display(Name = "Без оплаты (Разовый водитель)")]
		withoutPayment,
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

	public class EmployeeTypeStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeTypeStringType() : base(typeof(EmployeeType))
		{
		}
	}
}

