using System;

namespace TaxcomEdoApi.Library.Config
{
	/// <summary>
	/// Данные машиночитаемой доверенности
	/// </summary>
	public sealed class WarrantOptions
	{
		public const string Path = "WarrantOptions";

		/// <summary>
		/// ИНН доверенного лица
		/// </summary>
		public string RepresentativeInn { get; set; }
		/// <summary>
		/// Начало действия доверенности
		/// </summary>
		public DateTime StartDate { get; set; }
		/// <summary>
		/// Окончание действия доверенности
		/// </summary>
		public DateTime EndDate { get; set; }
		/// <summary>
		/// Номер доверенности
		/// </summary>
		public string WarrantNumber { get; set; }
		/// <summary>
		/// Должность подписанта
		/// </summary>
		public string JobPosition { get; set; }
	}
}
