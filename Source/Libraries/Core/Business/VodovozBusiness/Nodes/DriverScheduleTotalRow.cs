namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Базовый класс для итоговых строк расписания водителей
	/// </summary>
	public abstract class DriverScheduleTotalRow : DriverScheduleRow
	{
		/// <summary>
		/// Итоговая строка
		/// </summary>
		public bool IsTotalRow => true;

		/// <summary>
		/// Название итоговой строки
		/// </summary>
		public abstract string TotalTitle { get; }

		public override int MorningAddresses => 0;

		public override int MorningBottles => 0;

		public override int EveningAddresses => 0;

		public override int EveningBottles => 0;

		public override string LastModifiedDateTimeString => TotalTitle;
	}

	/// <summary>
	/// Строка с итоговыми адресами
	/// </summary>
	public class DriverScheduleTotalAddressesRow : DriverScheduleTotalRow
	{
		public override string TotalTitle => "Общее количество адресов";
	}

	/// <summary>
	/// Строка с итоговыми бутылями
	/// </summary>
	public class DriverScheduleTotalBottlesRow : DriverScheduleTotalRow
	{
		public override string TotalTitle => "Общее количество бутылей";
	}
}
