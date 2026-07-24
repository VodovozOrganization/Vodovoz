using System;

namespace Vodovoz.Settings.Mango
{
	public interface IMangoSettings
	{
		string ServiceHost { get; }
		uint ServicePort { get; }
		string VpbxApiKey { get; }
		string VpbxApiSalt { get; }
		bool MangoEnabled { get; }
		bool TestMode { get; }

		int GrpcKeepAliveTimeMs { get; }
		int GrpcKeepAliveTimeoutMs { get; }
		bool GrpcKeepAlivePermitWithoutCalls { get; }
		int GrpcMaxPingWithoutData { get; }

		/// <summary>
		/// Номер линии Манго, предназначенный для связи водителя с клиентом
		/// </summary>
		string DriversCallsLineNumber { get; }

		/// <summary>
		/// URL для совершения звонков через вебхук Манго
		/// </summary>
		string WebhookCallsUrl { get; }

		/// <summary>
		/// Базовый URL API ВАТС Манго
		/// </summary>
		string VpbxApiUrl { get; }

		/// <summary>
		/// Id роли, назначаемой создаваемому сотруднику ВАТС для водителя
		/// </summary>
		string DriverAccessRoleId { get; }

		/// <summary>
		/// Id исходящей линии создаваемого сотрудника ВАТС для водителя
		/// </summary>
		string DriverLineId { get; }

		/// <summary>
		/// Id группы ВАТС, в которую добавляются водители
		/// </summary>
		string DriversGroupId { get; }

		/// <summary>
		/// Минимальный добавочный номер пула, выделяемого водителям
		/// </summary>
		int DriverMangoExtensionNumberPoolStart { get; }

		/// <summary>
		/// Максимальный добавочный номер пула, выделяемого водителям
		/// </summary>
		int DriverMangoExtensionNumberPoolEnd { get; }

		/// <summary>
		/// Включена ли работа воркера регистрации водителей в Манго
		/// </summary>
		bool DriverMangoEmployeeRegistrationWorkerEnabled { get; }

		/// <summary>
		/// Включена ли работа воркера деактивации сотрудников Манго
		/// </summary>
		bool DriverMangoEmployeeDeactivationWorkerEnabled { get; }

		/// <summary>
		/// Интервал работы воркера регистрации водителей в Манго
		/// </summary>
		TimeSpan DriverMangoEmployeeRegistrationInterval { get; }

		/// <summary>
		/// Интервал проверки условия запуска воркера деактивации сотрудников Манго
		/// </summary>
		TimeSpan DriverMangoEmployeeDeactivationInterval { get; }

		/// <summary>
		/// Время (МСК), в которое запускается воркер деактивации сотрудников Манго
		/// </summary>
		TimeSpan DriverMangoEmployeeDeactivationRunTime { get; }

		/// <summary>
		/// Дата последнего запуска воркера удаление сотрудников (водителей) в Манго
		/// </summary>
		DateTime DriverMangoEmployeeDeactivationLastRunDate { get; }

		/// <summary>
		/// Сохраняет дату последнего запуска воркера удаления сотрудников (водителей) в Манго
		/// </summary>
		void UpdateDriverMangoEmployeeDeactivationLastRunDate(DateTime lastRunDate);
	}
}
