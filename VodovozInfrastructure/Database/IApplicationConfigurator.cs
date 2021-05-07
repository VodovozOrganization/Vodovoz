namespace VodovozInfrastructure.Database
{
    public interface IApplicationConfigurator
    {
        void ConfigureOrm();
        void CreateApplicationConfig();
    }
}
