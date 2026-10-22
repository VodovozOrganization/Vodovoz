using System.Collections.Generic;

namespace VodovozBusiness.Nodes.TTN
{
	/// <summary>
	/// Модель данных для формирования товарно-транспортной накладной (ТТН)
	/// </summary>
	public class TtnReport
	{
		#region Шапка

		/// <summary>
		/// Номер документа перемещения
		/// </summary>
		public string DocNumber { get; set; }

		/// <summary>
		/// День составления документа 
		/// </summary>
		public string DocDay { get; set; }

		/// <summary>
		/// Месяц составления документа 
		/// </summary>
		public string DocMonth { get; set; }

		/// <summary>
		/// Год составления документа
		/// </summary>
		public string DocYear { get; set; }

		/// <summary>
		/// Текстовая строка грузоотправителя: полное наименование, адрес, ИНН, КПП
		/// </summary>
		public string CargoSenderText { get; set; }

		/// <summary>
		/// Текстовая строка грузополучателя: полное наименование, адрес, ИНН, КПП
		/// </summary>
		public string CargoReceiverText { get; set; }

		/// <summary>
		/// Текстовая строка плательщика: полное наименование, адрес, ИНН, КПП, банковские реквизиты
		/// </summary>
		public string PayerText { get; set; }

		#endregion

		#region Товарный раздел

		/// <summary>
		/// Строки табличной части ТТН (позиции номенклатуры)
		/// </summary>
		public IList<TtnReportRow> Rows { get; set; } = new List<TtnReportRow>();

		/// <summary>
		/// Общее количество единиц товара по всем строкам
		/// </summary>
		public int GrandTotalCount { get; set; }

		/// <summary>
		/// Общее количество единиц товара прописью
		/// </summary>
		public string GrandTotalCountText { get; set; }

		/// <summary>
		/// Масса груза брутто в килограммах 
		/// </summary>
		public decimal MassBruttoValue { get; set; }

		/// <summary>
		/// Масса груза брутто в килограммах прописью
		/// </summary>
		public string MassBruttoText { get; set; }

		/// <summary>
		/// Масса груза брутто в тоннах 
		/// </summary>
		public decimal MassBruttoTonsValue { get; set; }

		/// <summary>
		/// Масса груза брутто в тоннах прописью
		/// </summary>
		public string MassBruttoTonsText { get; set; }

		/// <summary>
		/// Должность сотрудника, разрешившего отпуск груза
		/// </summary>
		public string ReleaseAllowedPosition { get; set; }

		/// <summary>
		/// ФИО сотрудника, разрешившего отпуск груза
		/// </summary>
		public string ReleaseAllowedName { get; set; }

		/// <summary>
		/// Должность сотрудника, произведшего отпуск груза
		/// </summary>
		public string ReleaseProducedPosition { get; set; }

		/// <summary>
		/// ФИО сотрудника, произведшего отпуск груза
		/// </summary>
		public string ReleaseProducedName { get; set; }

		/// <summary>
		/// Должность сотрудника, принявшего груз к перевозке
		/// </summary>
		public string CargoAcceptedPosition { get; set; }

		/// <summary>
		/// ФИО сотрудника, принявшего груз к перевозке
		/// </summary>
		public string CargoAcceptedName { get; set; }

		#endregion

		#region Транспортный раздел

		/// <summary>
		/// День срока доставки груза 
		/// </summary>
		public string DeliveryDay { get; set; }

		/// <summary>
		/// Месяц срока доставки груза прописью
		/// </summary>
		public string DeliveryMonthText { get; set; }

		/// <summary>
		/// Год срока доставки груза
		/// </summary>
		public string DeliveryYear { get; set; }

		/// <summary>
		/// Текстовая строка организации-владельца автотранспорта
		/// </summary>
		public string OrganizationText { get; set; }

		/// <summary>
		/// Марка (модель) автомобиля
		/// </summary>
		public string CarModel { get; set; }

		/// <summary>
		/// Государственный регистрационный номер автомобиля
		/// </summary>
		public string CarRegistrationNumber { get; set; }

		/// <summary>
		/// ФИО водителя
		/// </summary>
		public string DriverFullName { get; set; }

		/// <summary>
		/// Номер водительского удостоверения
		/// </summary>
		public string DriverLicenseNumber { get; set; }

		/// <summary>
		/// Адрес пункта погрузки
		/// </summary>
		public string LoadingPointAddress { get; set; }

		/// <summary>
		/// Адрес пункта разгрузки
		/// </summary>
		public string UnloadingPointAddress { get; set; }

		/// <summary>
		/// Марка (модель) полуприцепа
		/// </summary>
		public string TrailerModel { get; set; }

		/// <summary>
		/// Государственный регистрационный номер полуприцепа
		/// </summary>
		public string TrailerRegistrationNumber { get; set; }

		#endregion
	}
}
