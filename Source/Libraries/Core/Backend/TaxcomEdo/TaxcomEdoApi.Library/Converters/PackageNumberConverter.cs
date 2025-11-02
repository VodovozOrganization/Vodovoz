namespace TaxcomEdoApi.Library.Converters
{
	public class PackageNumberConverter
	{
		/// <summary>
		/// Конвертируем gtin в номер упаковки для указания в УПД при объемно сортовом учете
		/// Паттерн создания: [02][gtin][37][count], где 02 и 37 константы, gtin - собственно сам gtin,
		/// по правилам, он должен состоять из 14 символов, если он меньше, то он дополняется лидирующими нулями,
		/// count - количество товара
		/// </summary>
		/// <param name="gtin">Глобальный номер предмета товара</param>
		/// <param name="count">Количество товара</param>
		/// <returns>Номер упаковки для УПД</returns>
		public string ConvertGtinToPackageNumberUpd(string gtin, decimal count)
		{
			var diff = 14 - gtin.Length;
			var additionGtin = string.Empty;
			
			//Если Gtin меньше 14 символов, то дополняем его лидирующими нулями
			if(diff != 0)
			{
				for(int i = 0; i < diff; i++)
				{
					additionGtin += 0;
				}
			}
			
			return $"02{additionGtin}{gtin}37{count}";
		}
	}
}
