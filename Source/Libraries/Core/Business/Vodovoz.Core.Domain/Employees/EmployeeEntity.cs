using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Utilities.Text;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сотрудники",
		Nominative = "сотрудник",
		GenitivePlural = "сотрудников")]
	[EntityPermission]
	[HistoryTrace]
	public class EmployeeEntity : AccountOwnerBase, IDomainObject, IHasAttachedFilesInformations<EmployeeFileInformation>, IHasPhoto
	{
		private int _id;
		private DateTime _creationDate;
		private string _name;
		private string _lastName;
		private string _patronymic;
		private bool _isRussianCitizen = true;
		private DateTime? _birthdayDate;
		private string _drivingLicense;
		private string _addressRegistration;
		private string _addressCurrent;
		private string _inn;
		private int? _skilllevel;
		private byte[] _photo;
		private EmployeeCategory _category;
		private uint? _innerPhone;
		private string _androidLogin;
		private string _androidPassword;
		private string _androidSessionKey;
		private string _androidToken;
		private EmployeeStatus _status;
		private DateTime? _firstWorkDay;
		private DateTime? _dateHired;
		private DateTime? _dateFired;
		private DateTime? _dateCalculated;
		private DriverType? _driverType;
		private bool _largusDriver;
		private Gender? _gender;
		private float _driverSpeed = 1;
		private short _tripPriority = 6;
		private int _minRouteAddresses;
		private int _maxRouteAddresses;
		private string _email;
		private string _comment;
		private bool _visitingMaster;
		private bool _isChainStoreDriver;
		private bool _isDriverForOneDay;
		private string _loginForNewUser;
		private IObservableList<EmployeeFileInformation> _attachedFileInformations = new ObservableList<EmployeeFileInformation>();
		private string _photoFileName;

		public EmployeeEntity()
		{
			Name = String.Empty;
			LastName = String.Empty;
			Patronymic = String.Empty;
			DrivingLicense = String.Empty;
			Category = EmployeeCategory.office;
			AddressRegistration = String.Empty;
			AddressCurrent = String.Empty;
		}

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateFileInformations();
				}
			}
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Имя")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Фамилия")]
		public virtual string LastName
		{
			get => _lastName;
			set => SetField(ref _lastName, value?.Trim());
		}

		[Display(Name = "Отчество")]
		public virtual string Patronymic
		{
			get => _patronymic;
			set => SetField(ref _patronymic, value?.Trim());
		}

		[Display(Name = "Российское гражданство")]
		public virtual bool IsRussianCitizen
		{
			get => _isRussianCitizen;
			set => SetField(ref _isRussianCitizen, value);
		}

		[Display(Name = "Дата рождения")]
		public virtual DateTime? BirthdayDate
		{
			get => _birthdayDate;
			set => SetField(ref _birthdayDate, value);
		}

		[Display(Name = "Водительское удостоверение")]
		public virtual string DrivingLicense
		{
			get => _drivingLicense;
			set => SetField(ref _drivingLicense, value);
		}

		[Display(Name = "Адрес регистрации")]
		public virtual string AddressRegistration
		{
			get => _addressRegistration;
			set => SetField(ref _addressRegistration, value);
		}

		[Display(Name = "Фактический адрес")]
		public virtual string AddressCurrent
		{
			get => _addressCurrent;
			set => SetField(ref _addressCurrent, value);
		}

		[Display(Name = "ИНН")]
		public virtual string INN
		{
			get => _inn;
			set => SetField(ref _inn, value);
		}

		[Display(Name = "Уровень квалификации")]
		public virtual int? SkillLevel
		{
			get => _skilllevel;
			set => SetField(ref _skilllevel, value);
		}

		[Display(Name = "Фотография")]
		public virtual byte[] Photo
		{
			get => _photo;
			set => SetField(ref _photo, value);
		}

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

		[Display(Name = "Логин для Android приложения")]
		public virtual string AndroidLogin
		{
			get => _androidLogin;
			set => SetField(ref _androidLogin, value);
		}

		[Display(Name = "Пароль для Android приложения")]
		public virtual string AndroidPassword
		{
			get => _androidPassword;
			set => SetField(ref _androidPassword, value);
		}

		[Display(Name = "Ключ сессии для Android приложения")]
		public virtual string AndroidSessionKey
		{
			get => _androidSessionKey;
			set => SetField(ref _androidSessionKey, value);
		}

		[Display(Name = "Токен Android приложения пользователя для отправки Push-сообщений")]
		public virtual string AndroidToken
		{
			get => _androidToken;
			set => SetField(ref _androidToken, value);
		}

		[Display(Name = "Статус сотрудника")]
		public virtual EmployeeStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
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
		/// Приоритет(1-10) чем меньше тем лучше. Фактически это штраф.
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

		[Display(Name = "Разовый водитель")]
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

		[Display(Name = "Имя файла фотографии")]
		public virtual string PhotoFileName
		{
			get => _photoFileName;
			set => SetField(ref _photoFileName, value);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<EmployeeFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		[Display(Name = "ФИО")]
		public virtual string FullName
		{
			get => PersonHelper.PersonFullName(LastName, Name, Patronymic);
		}

		[Display(Name = "Фамилия и инициалы")]
		public virtual string ShortName
		{
			get => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);
		}

		public virtual string Title => FullName;

		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new EmployeeFileInformation
			{
				FileName = fileName,
				EmployeeId = Id
			});
		}

		public virtual void RemoveFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.EmployeeId = Id;
			}
		}
	}
}
