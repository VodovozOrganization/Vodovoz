using System.ComponentModel.Design;

namespace VodovozInfrastructure.Extensions
{
	public static class BooleanExtensions
	{
		public static string ConvertToYesOrNo(this bool value) => value ? "Да" : "Нет";
		
		public static string ConvertToNullOrYesOrNo(this bool? value)
		{
			switch(value)
			{
				case null:
					return "Null";
				default:
					return ConvertToYesOrNo(value.Value);
			}
		}
	}
}
