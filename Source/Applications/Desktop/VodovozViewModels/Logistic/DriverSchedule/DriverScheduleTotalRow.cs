namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
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

		public override int MorningAddresses
		{
			get => 0;
			set { }
		}

		public override int MorningBottles
		{
			get => 0;
			set { }
		}

		public override int EveningAddresses
		{
			get => 0;
			set { }
		}

		public override int EveningBottles
		{
			get => 0;
			set { }
		}
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
