namespace Edo.Common
{
	public class ResultCodesDuplicatesException : System.Exception
	{
		public override string Message => "Обнаружено дублирование Result кодов";
	}
}
