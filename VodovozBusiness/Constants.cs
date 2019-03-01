using System;
namespace Vodovoz
{
	public static class Constants
	{
		//Координаты базы
		[Obsolete("Теперь несколько баз. Брать координаты из GeographicGroup", true)]
		public static double BaseLatitude = 59.88632093834261;
		[Obsolete("Теперь несколько баз. Брать координаты из GeographicGroup", true)]
		public static double BaseLongitude = 30.394406318664547;
		//Координаты центра города
		public static double CenterOfCityLatitude = 59.9390;
		public static double CenterOfCityLongitude = 30.3157;
	}
}
