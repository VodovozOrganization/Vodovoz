using System.Text;

namespace CustomerOrdersApi.Library.V8.Services.Converters
{
	public class RepeatEveryWeeksConverter : IRepeatEveryWeeksConverter
	{
		public string Convert(int repeatEveryWeeks)
		{
			var sb = new StringBuilder();
			sb.Append("раз в ");
			
			switch(repeatEveryWeeks)
			{
				case 1:
					sb.Append("неделю");
					break;
				default:
					sb.Append(repeatEveryWeeks);
					sb.Append(' ');
					sb.Append(DeclensionWeekWord(repeatEveryWeeks.ToString()));
					break;
			}
			
			return sb.ToString();
		}

		/// <summary>
		/// Склоняем слово неделя
		/// </summary>
		/// <param name="repeatEveryWeeks">Число недель, для склонения</param>
		/// <returns></returns>
		private string DeclensionWeekWord(string repeatEveryWeeks)
		{
			//Символ, отвечающий за десятки. Если строка представляет число меньше десяти - просто первый символ
			var dozensChar = repeatEveryWeeks.Length == 1 ? '0' : repeatEveryWeeks[^2];
			var lastChar = repeatEveryWeeks[^1];
			var dozensInt =  System.Convert.ToInt32(dozensChar);
			var lastInt =  System.Convert.ToInt32(lastChar);

			if(lastInt is >= 2 and <= 4 && dozensInt != 1)
			{
				return "недели";
			}
			
			if(lastInt == 1 && dozensInt != 1)
			{
				return "неделю";
			}

			return "недель";
		} 
	}
}
