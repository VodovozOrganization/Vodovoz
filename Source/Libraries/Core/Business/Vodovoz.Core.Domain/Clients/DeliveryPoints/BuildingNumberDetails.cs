using System.Text;

namespace Vodovoz.Core.Domain.Clients.DeliveryPoints
{
	/// <summary>
	/// Класс для хранения информации о доме с разбиением на составные части(номер дома, корпус, литера, строение)
	/// В константах зашито сокращенное наименование параметров(корпус, литера, строение), поэтому, если в адресной базе поменяется их обозначение,
	/// то эти изменения нужно перенести сюда, для правильной проверки на халявщиков
	/// </summary>
	public class BuildingNumberDetails
	{
		private const string _literShort = "литер ";
		private const string _corpusShort = "к. ";
		private const string _structureShort = "стр. ";
		
		/// <summary>
		/// Номер дома
		/// </summary>
		public string BuildingNumber { get; set; }
		/// <summary>
		/// Корпус
		/// </summary>
		public string Corpus { get; set; }
		/// <summary>
		/// Строение
		/// </summary>
		public string Structure { get; set; }
		/// <summary>
		/// Литера
		/// </summary>
		public string Liter { get; set; }

		/// <summary>
		/// Вывод информации о номере дома в нужном формате
		/// </summary>
		/// <returns>Отформатированная строка с номером дома</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			
			if(!string.IsNullOrWhiteSpace(BuildingNumber))
			{
				sb.Append(BuildingNumber);
			}
			
			if(!string.IsNullOrWhiteSpace(Corpus))
			{
				sb.Append(", ");
				sb.Append(_corpusShort);
				sb.Append(Corpus);
			}
			
			if(!string.IsNullOrWhiteSpace(Structure))
			{
				sb.Append(", ");
				sb.Append(_structureShort);
				sb.Append(Structure);
			}
			
			if(!string.IsNullOrWhiteSpace(Liter))
			{
				sb.Append(", ");
				sb.Append(_literShort);
				sb.Append(Liter);
			}
			
			return sb.ToString();
		}
	}
}
