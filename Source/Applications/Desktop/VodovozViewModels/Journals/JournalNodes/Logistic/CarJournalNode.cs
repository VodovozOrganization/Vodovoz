using QS.Project.Journal;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	/// <summary>
	/// Узел журнала автомобилей
	/// </summary>
	public class CarJournalNode : JournalEntityNodeBase<Car>
	{
		/// <summary>
		/// Заголовок узла журнала
		/// </summary>
		public override string Title => $"{ManufacturerName} {ModelName} ({RegistrationNumber}) {DriverName}";

		/// <summary>
		/// Собственник автомобиля
		/// </summary>
		public string CarOwner { get; set; }

		/// <summary>
		/// Название модели автомобиля
		/// </summary>
		public string ModelName { get; set; }

		/// <summary>
		/// Название производителя автомобиля
		/// </summary>
		public string ManufacturerName { get; set; }

		/// <summary>
		/// Государственный регистрационный номер
		/// </summary>
		public string RegistrationNumber { get; set; }

		/// <summary>
		/// ФИО водителя, закреплённого за автомобилем
		/// </summary>
		public string DriverName { get; set; }

		/// <summary>
		/// Название страховой компании по ОСАГО
		/// </summary>
		public string OsagoInsurer { get; set; }

		/// <summary>
		/// Название страховой компании по КАСКО
		/// </summary>
		public string KaskoInsurer { get; set; }

		/// <summary>
		/// Признак того, что автомобиль находится в архиве
		/// </summary>
		public bool IsArchive { get; set; }

		/// <summary>
		/// Признак необходимости подсветить строку журнала цветом
		/// </summary>
		public bool IsShowBackgroundColorNotification { get; set; }

		/// <summary>
		/// Названия страховых компаний по ОСАГО и КАСКО, объединённые в одну строку
		/// </summary>
		public string InsurersNames
		{
			get
			{
				var insurersNames = new List<string>();

				if(!string.IsNullOrEmpty(OsagoInsurer))
				{
					insurersNames.Add(OsagoInsurer);
				}

				if(!string.IsNullOrEmpty(KaskoInsurer))
				{
					insurersNames.Add(KaskoInsurer);
				}

				return string.Join(", ", insurersNames);
			}
		}

		/// <summary>
		/// VIN-номер автомобиля
		/// </summary>
		public string VIN { get; set; }

		/// <summary>
		/// Тип модели авто
		/// </summary>
		public CarTypeOfUse CarTypeOfUse { get; set; }
	}
}
