using System;

namespace Vodovoz.EntityRepositories.Logistic
{
	/// <summary>
	/// Количество выполненных адресов водителя в разрезе района точки доставки
	/// </summary>
	public class CompletedAddressesCountNode
	{
		/// <summary>
		/// Идентификатор водителя МЛ
		/// </summary>
		public int DriverId { get; set; }

		/// <summary>
		/// Фамилия водителя
		/// </summary>
		public string DriverLastName { get; set; }

		/// <summary>
		/// Имя водителя
		/// </summary>
		public string DriverName { get; set; }

		/// <summary>
		/// Отчество водителя
		/// </summary>
		public string DriverPatronymic { get; set; }

		/// <summary>
		/// Идентификатор района точки доставки (null, если у адреса нет района)
		/// </summary>
		public int? DistrictId { get; set; }

		/// <summary>
		/// Название района точки доставки (null, если у адреса нет района)
		/// </summary>
		public string DistrictName { get; set; }

		/// <summary>
		/// Количество выполненных адресов
		/// </summary>
		public int CompletedAddressesCount { get; set; }
	}
}
