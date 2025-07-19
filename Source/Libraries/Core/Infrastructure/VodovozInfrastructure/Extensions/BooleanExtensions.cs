namespace VodovozInfrastructure.Extensions
{
	public static class BooleanExtensions
	{
		public static string ConvertToYesOrNo(this bool value) => value ? "Да" : "Нет";
		public static string ConvertToYesOrEmpty(this bool value) => value ? "Да" : string.Empty;
		
		public static string ConvertToNotSetOrYesOrNo(this bool? value)
		{
			switch(value)
			{
				case null:
					return "Не задано";
				default:
					return ConvertToYesOrNo(value.Value);
			}
		}

		public static string ConvertToNullOrYesOrNo(this bool? value)
		{
			switch(value)
			{
				case null:
					return null;
				default:
					return ConvertToYesOrNo(value.Value);
			}
		}
	}
}
