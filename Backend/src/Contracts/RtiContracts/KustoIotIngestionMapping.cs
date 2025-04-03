using System.Collections.Generic;
using Kusto.Data.Common;

namespace Fabric.Rti.workload.Backend.Contracts.RtiContracts;

public class KustoIotIngestionMapping
{
    public static readonly IEnumerable<ColumnMapping> Mapping = new[]
    {
        new ColumnMapping
        {
            ColumnName = "Timestamp",
            Properties = new Dictionary<string, string>()
            {
                { MappingConsts.Path, "$.timestamp" }
            }
        },
        new ColumnMapping
        {
            ColumnName = "Name",
            Properties = new Dictionary<string, string>()
            {
                { MappingConsts.Path, "$.name" }
            }
        },
        new ColumnMapping
        {
            ColumnName = "Value",
            Properties = new Dictionary<string, string>()
            {
                { MappingConsts.Path, "$.value" }
            }
        }
    };
}
