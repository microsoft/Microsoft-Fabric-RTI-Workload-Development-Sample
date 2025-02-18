import React from "react";

interface Column {
    ColumnName: string;
    DataType: string;
    ColumnType: string;
}

interface Table {
    TableName: string;
    Columns: Column[];
    Rows: string[][];
}

interface KustoQueryResultProps {
    rawQueryResult: any;
}

function parseRawQueryResult(rawQueryResult: any): Table | undefined {
    if (!rawQueryResult || !Array.isArray(rawQueryResult.Tables)) {
        return undefined;
    }
    return rawQueryResult.Tables.find((table: Table) => table.TableName === "Table_0");
}

export function KustoQueryResultComponent({ rawQueryResult }: KustoQueryResultProps) {
    const resultTable = parseRawQueryResult(rawQueryResult);

    return (
        <div className="kusto-query-result-table">

            <table className="styled-table">
                <thead>
                    <tr>
                        {resultTable.Columns.map((column) => (
                            <th key={column.ColumnName}>{column.ColumnName}</th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {resultTable.Rows.map((row, rowIndex) => (
                        <tr key={rowIndex}>
                            {row.map((cell, cellIndex) => (
                                <td key={cellIndex}>{cell}</td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>

        </div>
    );
}