namespace FastPaymentsAPI.Library.Managers
{
	public interface IDTOManager
	{
		string GetXmlStringFromDTO<T>(T dto)
			where T : class;
	}
}
