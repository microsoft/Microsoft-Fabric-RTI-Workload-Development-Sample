using System;
using System.Collections.Generic;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KustoIotDataTableRecord
{
    public DateTime Timestamp;
    public string Name;
    public double Value;
}

public static class KustoIotDataTableRecordExtensions
{
    private static readonly Random m_random = new Random();

    public static string ToCsvFormat(this KustoIotDataTableRecord record)
    {
        return string.Join(",", record.Timestamp, record.Name, record.Value);
    }

    public static IEnumerable<KustoIotDataTableRecord> GenerateRandomRecords(int numberOfRecords)
    {
        var records = new List<KustoIotDataTableRecord>(numberOfRecords);
        for (var i = 0; i < numberOfRecords; i++)
        {
            records.Add(GenerateRecord());
        }

        return records;
    }

    private static KustoIotDataTableRecord GenerateRecord()
    {
        return new KustoIotDataTableRecord
        {
            Timestamp = DateTime.Now,
            Name = "sensor-" + m_random.Next(1, 200),
            Value = m_random.NextDouble()
        };
    }
}