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

		public override int MorningAddresses => 0;

		public override int MorningBottles => 0;

		public override int EveningAddresses => 0;

		public override int EveningBottles => 0;
	}

	/// <summary>
	/// Строка с итоговыми адресами
	/// </summary>
	public class DriverScheduleTotalAddressesRow : DriverScheduleTotalRow
	{
		public override string LastModifiedDateTimeString => "Общее количество адресов";
	}

	/// <summary>
	/// Строка с итоговыми бутылями
	/// </summary>
	public class DriverScheduleTotalBottlesRow : DriverScheduleTotalRow
	{
		public override string LastModifiedDateTimeString => "Общее количество бутылей";
	}
}
