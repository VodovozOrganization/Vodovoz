namespace Vodovoz.Database
{
    public interface IDatabaseConfigurator
    {
        void ConfigureOrm();
        void CreateBaseConfig();
    }
}