using CsvHelper.Configuration;

public class BGMData
{
    public int Number { get; set; }
    public eSCENE_TYPE State { get; set; }
    public string Name { get; set; }
    public string FileName { get; set; }
}

public sealed class BGMDataMap : ClassMap<BGMData>
{
    public BGMDataMap()
    {
        Map(m => m.Number).Name("Number");
        Map(m => m.State).Name("State");
        Map(m => m.Name).Name("Name");
        Map(m => m.FileName).Name("FileName");
    }
}