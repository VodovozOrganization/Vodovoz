using Gamma.Utilities;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Osrm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Settings.Common;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "точки доставки",
		Nominative = "точка доставки",
		Accusative = "точки доставки")]
	[HistoryTrace]
	[EntityPermission]
	public class DeliveryPoint : DeliveryPointEntity, IValidatableObject
	{
		public const int IntercomMaxLength = 100;

		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private IOsrmSettings _globalSettings;
		private IList<DeliveryPointResponsiblePerson> _responsiblePersons = new List<DeliveryPointResponsiblePerson>();
		private GenericObservableList<DeliveryPointResponsiblePerson> _observableResponsiblePersons;
		private District _district;
		private DeliverySchedule _deliverySchedule;
		private Counterparty _counterparty;
		private Nomenclature _defaultWaterNomenclature;
		private User _coordsLastChangeUser;
		private IList<Phone> _phones = new List<Phone>();
		private GenericObservableList<Phone> _observablePhones;
		private IList<NomenclatureFixedPrice> _nomenclatureFixedPrices = new List<NomenclatureFixedPrice>();
		private GenericObservableList<NomenclatureFixedPrice> _observableNomenclatureFixedPrices;
		private IList<DeliveryPointEstimatedCoordinate> _deliveryPointEstimatedCoordinates = new List<DeliveryPointEstimatedCoordinate>();
		private GenericObservableList<DeliveryPointEstimatedCoordinate> _observableDeliveryPointEstimatedCoordinates;
		private LogisticsRequirements _logisticsRequirements;
		private DeliveryPointCategory _category;

		#region Свойства

		[Display(Name = "Ответственные лица")]
		public virtual IList<DeliveryPointResponsiblePerson> ResponsiblePersons
		{
			get => _responsiblePersons;
			set => SetField(ref _responsiblePersons, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPointResponsiblePerson> ObservableResponsiblePersons
		{
			get
			{
				if(_observableResponsiblePersons == null)
				{
					_observableResponsiblePersons = new GenericObservableList<DeliveryPointResponsiblePerson>(ResponsiblePersons);
				}

				return _observableResponsiblePersons;
			}
		}

		[Display(Name = "Район доставки")]
		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		[Display(Name = "График доставки")]
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}

		[Display(Name = "Контрагент")]
		public virtual new Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Вода по умолчанию")]
		public virtual Nomenclature DefaultWaterNomenclature
		{
			get => _defaultWaterNomenclature;
			set => SetField(ref _defaultWaterNomenclature, value);
		}

		[Display(Name = "Последнее изменение пользователем")]
		public virtual User СoordsLastChangeUser
		{
			get => _coordsLastChangeUser;
			set => SetField(ref _coordsLastChangeUser, value);
		}

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Phone> ObservablePhones
		{
			get
			{
				if(_observablePhones == null)
				{
					_observablePhones = new GenericObservableList<Phone>(Phones);
				}

				return _observablePhones;
			}
		}

		[Display(Name = "Фиксированные цены")]
		public virtual new IList<NomenclatureFixedPrice> NomenclatureFixedPrices
		{
			get => _nomenclatureFixedPrices;
			set => SetField(ref _nomenclatureFixedPrices, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureFixedPrice> ObservableNomenclatureFixedPrices
		{
			get
			{
				if(_observableNomenclatureFixedPrices == null)
				{
					_observableNomenclatureFixedPrices = new GenericObservableList<NomenclatureFixedPrice>(NomenclatureFixedPrices);
				}

				return _observableNomenclatureFixedPrices;
			}
		}

		[Display(Name = "Предполагаемые координаты доставки")]
		public virtual IList<DeliveryPointEstimatedCoordinate> DeliveryPointEstimatedCoordinates
		{
			get => _deliveryPointEstimatedCoordinates;
			set => SetField(ref _deliveryPointEstimatedCoordinates, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPointEstimatedCoordinate> ObservableDeliveryPointEstimatedCoordinates => _observableDeliveryPointEstimatedCoordinates
					?? (_observableDeliveryPointEstimatedCoordinates = new GenericObservableList<DeliveryPointEstimatedCoordinate>(DeliveryPointEstimatedCoordinates));

		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}
		
		[Display(Name = "Тип объекта")]
		public virtual DeliveryPointCategory Category
		{
			get => _category;
			set
			{
				if(value != null && value.IsArchive)
				{
					value = null;
				}

				SetField(ref _category, value);
			}
		}

		#endregion Временные поля для хранения фиксированных цен из 1с

		#region Расчетные

		public virtual string Title => string.IsNullOrWhiteSpace(CompiledAddress) ? "АДРЕС ПУСТОЙ" : CompiledAddress;
		
		public virtual bool CoordinatesExist => Latitude.HasValue && Longitude.HasValue;

		public virtual Point NetTopologyPoint => CoordinatesExist ? new Point((double)Latitude, (double)Longitude) : null;

		public virtual PointOnEarth PointOnEarth => new PointOnEarth(Latitude.Value, Longitude.Value);
		public virtual PointCoordinates PointCoordinates => new PointCoordinates(Latitude, Longitude);

		public virtual GMap.NET.PointLatLng GmapPoint => new GMap.NET.PointLatLng((double)Latitude, (double)Longitude);

		public virtual bool HasFixedPrices => NomenclatureFixedPrices.Any();

		#endregion Расчетные

		/// <summary>
		/// Возврат районов доставки, в которые попадает точка доставки
		/// </summary>
		/// <param name="uow">UnitOfWork через который будет получены все районы доставки,
		/// среди которых будет производится поиск подходящего района</param>
		public virtual IEnumerable<District> CalculateDistricts(IUnitOfWork uow, IDeliveryRepository deliveryRepository)
		{
			return !CoordinatesExist ? new List<District>() : deliveryRepository.GetDistricts(uow, Latitude.Value, Longitude.Value);
		}

		/// <summary>
		/// Поиск района города, в котором находится текущая точка доставки
		/// </summary>
		/// <returns><c>true</c>, если район города найден</returns>
		/// <param name="uow">UnitOfWork через который будет производится поиск подходящего района города</param>
		/// <param name="districtsSet">Версия районов, из которой будет ассоциироваться район. Если равно null, то будет браться активная версия</param>
		public virtual bool FindAndAssociateDistrict(IUnitOfWork uow, IDeliveryRepository deliveryRepository, DistrictsSet districtsSet = null)
		{
			if(!CoordinatesExist)
			{
				return false;
			}

			District foundDistrict = deliveryRepository.GetDistrict(uow, Latitude.Value, Longitude.Value, districtsSet);

			if(foundDistrict == null)
			{
				return false;
			}

			District = foundDistrict;

			return true;
		}

		/// <summary>
		/// Устанавливает правильно координты точки.
		/// </summary>
		/// <returns><c>true</c>, если координаты установлены</returns>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="uow">UnitOfWork через который будет производится поиск подходящего района города
		/// для определения расстояния до базы</param>
		public virtual bool SetСoordinates(
			decimal? latitude, 
			decimal? longitude, 
			IDeliveryRepository deliveryRepository, 
			IOsrmSettings globalSettings,
			IOsrmClient osrmClient,
			IUnitOfWork uow = null)
		{
			Latitude = latitude;
			Longitude = longitude;

			OnPropertyChanged(nameof(CoordinatesExist));

			if(Longitude == null || Latitude == null || !FindAndAssociateDistrict(uow, deliveryRepository))
			{
				return true;
			}

			GeoGroupVersion geoGroupVersion = District.GeographicGroup.GetActualVersionOrNull();

			if(geoGroupVersion == null)
			{
				throw new InvalidOperationException($"Не установлена активная версия данных в части города {District.GeographicGroup.Name}");
			}

			List<PointOnEarth> route = new List<PointOnEarth>(2) {
				new PointOnEarth(geoGroupVersion.BaseLatitude.Value, geoGroupVersion.BaseLongitude.Value),
				new PointOnEarth(Latitude.Value, Longitude.Value)
			};

			var result = osrmClient.GetRoute(route, false, GeometryOverview.False, globalSettings.ExcludeToll);

			if(result == null)
			{
				_logger.Error("Сервер расчета расстояний не вернул ответа.");
				return false;
			}

			if(result.Code != "Ok")
			{
				_logger.Error("Сервер расчета расстояний вернул следующее сообщение:\n" + result.StatusMessageRus);
				return false;
			}

			DistanceFromBaseMeters = result.Routes[0].TotalDistance;

			return true;
		}

		#region IValidatableObject Implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Category == null)
			{
				yield return new ValidationResult(
					"Необходимо выбрать тип точки доставки",
					new[] { this.GetPropertyName(o => o.Category) });
			}

			if(Counterparty == null)
			{
				yield return new ValidationResult(
					"Необходимо выбрать клиента",
					new[] { this.GetPropertyName(o => o.Counterparty) });
			}

			if(Building?.Length == 0)
			{
				yield return new ValidationResult(
					"Заполните поле \"Дом\"",
					new[] { this.GetPropertyName(o => o.Building) });
			}

			if(Building?.Length > 20)
			{
				yield return new ValidationResult(
					"Длина строки \"Дом\" не должна превышать 20 символов",
					new[] { this.GetPropertyName(o => o.Building) });
			}

			if(City?.Length == 0)
			{
				yield return new ValidationResult(
					"Заполните поле \"Город\"",
					new[] { this.GetPropertyName(o => o.City) });
			}

			if(City?.Length > 45)
			{
				yield return new ValidationResult(
					"Длина строки \"Город\" не должна превышать 45 символов",
					new[] { this.GetPropertyName(o => o.City) });
			}

			if(Street?.Length == 0)
			{
				yield return new ValidationResult(
					"Заполните поле \"Улица\"",
					new[] { this.GetPropertyName(o => o.Street) });
			}

			if(Street?.Length > 50)
			{
				yield return new ValidationResult(
					"Длина строки \"Улица\" не должна превышать 50 символов",
					new[] { this.GetPropertyName(o => o.Street) });
			}

			if(Room?.Length > 20)
			{
				yield return new ValidationResult(
					"Длина строки \"Офис/Квартира\" не должна превышать 20 символов",
					new[] { this.GetPropertyName(o => o.Room) });
			}

			if(Entrance?.Length > 50)
			{
				yield return new ValidationResult(
					"Длина строки \"Парадная\" не должна превышать 50 символов",
					new[] { this.GetPropertyName(o => o.Entrance) });
			}

			if(Floor?.Length > 20)
			{
				yield return new ValidationResult(
					"Длина строки \"Этаж\" не должна превышать 20 символов",
					new[] { this.GetPropertyName(o => o.Floor) });
			}

			if(Code1c?.Length > 10)
			{
				yield return new ValidationResult(
					"Длина строки \"Код 1С\" не должна превышать 10 символов",
					new[] { this.GetPropertyName(o => o.Code1c) });
			}

			if(KPP?.Length > 45)
			{
				yield return new ValidationResult(
					"Длина строки \"КПП\" не должна превышать 45 символов",
					new[] { this.GetPropertyName(o => o.KPP) });
			}

			RoomType[] notNeedOrganizationRoomTypes = new RoomType[] { RoomType.Apartment, RoomType.Chamber };

			if(Counterparty.PersonType == PersonType.natural && !notNeedOrganizationRoomTypes.Contains(RoomType))
			{
				if(string.IsNullOrWhiteSpace(Organization))
				{
					yield return new ValidationResult(
						"Необходимо заполнить поле \"Организация\"",
						new[] { this.GetPropertyName(o => o.Organization) });
				}
			}

			if(Organization?.Length > 45)
			{
				yield return new ValidationResult(
					"Длина строки \"Организация\" не должна превышать 45 символов",
					new[] { this.GetPropertyName(o => o.Organization) });
			}
			
			if(Intercom?.Length > IntercomMaxLength)
			{
				yield return new ValidationResult(
					$"Длина строки \"Домофон\" не должна превышать {IntercomMaxLength} символов");
			}

			var everyAddedMinCountValueCount = NomenclatureFixedPrices
				.GroupBy(p => new { p.Nomenclature, p.MinCount })
				.Select(p => new { NomenclatureName = p.Key.Nomenclature?.Name, MinCountValue = p.Key.MinCount, Count = p.Count() });

			foreach(var p in everyAddedMinCountValueCount)
			{
				if(p.Count > 1)
				{
					yield return new ValidationResult(
							$"\"{p.NomenclatureName}\": фиксированная цена для количества \"{p.MinCountValue}\" указана {p.Count} раз(а)",
							new[] { this.GetPropertyName(o => o.NomenclatureFixedPrices) });
				}
			}

			foreach(NomenclatureFixedPrice fixedPrice in NomenclatureFixedPrices)
			{
				IEnumerable<ValidationResult> fixedPriceValidationResults = fixedPrice.Validate(validationContext);

				foreach(ValidationResult fixedPriceValidationResult in fixedPriceValidationResults)
				{
					yield return fixedPriceValidationResult;
				}
			}

			if(LunchTimeFrom == null && LunchTimeTo != null)
			{
				yield return new ValidationResult("При заполненной дате окончания обеда должна быть указана и дата начала обеда.",
					new[] { nameof(LunchTimeTo) });
			}

			if(LunchTimeTo == null && LunchTimeFrom != null)
			{
				yield return new ValidationResult("При заполненной дате начала обеда должна быть указана и дата окончания обеда.",
					new[] { nameof(LunchTimeTo) });
			}

			StringBuilder phonesValidationStringBuilder = new StringBuilder();

			foreach(Phone phone in Phones)
			{
				if(phone.RoboAtsCounterpartyName == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано имя контрагента.");
				}

				if(phone.RoboAtsCounterpartyPatronymic == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано отчество контрагента.");
				}

				if(!phone.IsValidPhoneNumber)
				{
					phonesValidationStringBuilder.AppendLine($"Номер {phone.Number} имеет неправильный формат.");
				}
			}

			string phonesValidationMessage = phonesValidationStringBuilder.ToString();

			if(!string.IsNullOrEmpty(phonesValidationMessage))
			{
				yield return new ValidationResult(phonesValidationMessage);
			}

			if(ResponsiblePersons.Any(x => x.DeliveryPointResponsiblePersonType == null || x.Employee == null || string.IsNullOrWhiteSpace(x.Phone)))
			{
				yield return new ValidationResult("Для ответственных лиц должны быть заполнены Тип, Сотрудник и Телефон", new[] { nameof(ResponsiblePersons) });
			}
		}

		#endregion IValidatableObject Implementation
	}
}
