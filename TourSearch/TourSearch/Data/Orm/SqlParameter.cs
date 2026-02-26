namespace TourSearch.Data.Orm;

public class SqlParameter
{
    public string Name { get; }
    public object? Value { get; }

    public SqlParameter(string name, object? value)
    {
        Name = name;
        Value = value;
    }
}
