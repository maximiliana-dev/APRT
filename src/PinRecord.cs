using CsvHelper.Configuration;

public class PinRecord
    {
        public string? ICCID { get; set; }
        public string? PIN { get; set; }
        public string? PUK { get; set; }
    }

    public sealed class PinRecordMap : ClassMap<PinRecord>
{
    public PinRecordMap()
    {
        Map(m => m.ICCID).Name("ICC", "ICCID");
        Map(m => m.PIN).Name("PIN");
        Map(m => m.PUK).Name("PUK");
    }
}