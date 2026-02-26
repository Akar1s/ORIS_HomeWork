namespace TourSearch.Data.Orm;

public interface IDbSessionFactory
{
    IDbSession CreateSession();
}
