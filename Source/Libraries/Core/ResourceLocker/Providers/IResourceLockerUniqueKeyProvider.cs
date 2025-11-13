namespace ResourceLocker.Library.Providers
{
	public interface IResourceLockerUniqueKeyProvider
	{
		string GetResourceLockerUniqueKeyByResourceName(string resourceName);
	}
}
