namespace ResourceLocker.Library.Factories
{
	public interface IResourceLockerFactory
	{
		IResourceLocker Create(string resourceKey);
	}
}
