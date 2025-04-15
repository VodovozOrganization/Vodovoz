namespace Edo.Common
{
	public class OrderContactMissingException : System.Exception
	{
		public override string Message => "Не найден контакт для заказа";
	}
}
