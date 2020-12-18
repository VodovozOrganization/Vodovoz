using NHibernate;
using NHibernate.Dialect.Function;
using NHibernate.Spatial.Dialect;

namespace Vodovoz.NhibernateExtensions
{
    public class MySQL57SpatialExtendedDialect : MySQL57SpatialDialect
    {
        public MySQL57SpatialExtendedDialect()
        {
            this.RegisterFunction("DATE", new StandardSQLFunction("DATE", NHibernateUtil.Date));
        }
    }
}