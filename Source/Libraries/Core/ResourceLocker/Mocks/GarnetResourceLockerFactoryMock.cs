using ResourceLocker.Library.Factories;

namespace ResourceLocker.Library.Mocks
{
	/// <summary>
	/// Фабрика заглушек <see cref="GarnetResourceLockerMock"/>
	/// </summary>
	public class GarnetResourceLockerFactoryMock : IResourceLockerFactory
	{
		public IResourceLocker Create(string resourceKey)
		{
			return new GarnetResourceLockerMock();
		}
	}
}
