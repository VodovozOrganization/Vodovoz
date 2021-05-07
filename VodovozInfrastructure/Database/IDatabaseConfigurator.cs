namespace VodovozInfrastructure.Database
{
    public interface IDatabaseConfigurator
    {
        void ConfigureOrm();
        void CreateBaseConfig();
    }
}
