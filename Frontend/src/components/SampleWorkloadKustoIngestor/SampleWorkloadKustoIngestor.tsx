import React, { useState } from "react";
import { KustoComponentProps } from "../../App";
import { Divider, Button, Input } from "@fluentui/react-components";

interface TableRow {
    column1: string;
    column2: string;
    column3: string;
}

export function KustoIngestorComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KustoComponentProps) {
    const [rows, setRows] = useState<TableRow[]>([]);
    const [stagingRow, setStagingRow] = useState<TableRow>({
        column1: "",
        column2: "",
        column3: ""
    });
    const targetTable = "IotData";

    function addRow() {
        setRows([...rows, stagingRow]);
        setStagingRow({ column1: "", column2: "", column3: "" });
    }

    function removeRow(index: number) {
        const newRows = rows.filter((_, i) => i !== index);
        setRows(newRows);
    }

    function handleInputChange(e: React.ChangeEvent<HTMLInputElement>, column: keyof TableRow) {
        setStagingRow({ ...stagingRow, [column]: e.target.value });
    }

    return (
        <div className='kusto-ingestor'>
            <h2>Kusto Ingestion Wizard</h2>
            <Divider alignContent="start" className="divider">
                <b>Ingestion target</b>
            </Divider>
            <div>
                <label className='label-key'>KQL Database Query Url:</label>
                <label className='label-value'>{kqlDatabaseQueryUrl}</label>
            </div>
            <div>
                <label className='label-key'>KQL Database name:</label>
                <label className='label-value'>{kqlDatabaseDisplayName}</label>
            </div>
            <div>
                <label className='label-key'>Table:</label>
                <label className='label-value'>{targetTable}</label>
            </div>
            <Divider alignContent="start" className="divider">
                <b>Records generator</b>
            </Divider>
            <div>
                <Input
                    placeholder="Column 1"
                    value={stagingRow.column1}
                    onChange={(e) => handleInputChange(e, "column1")}
                />
                <Input
                    placeholder="Column 2"
                    value={stagingRow.column2}
                    onChange={(e) => handleInputChange(e, "column2")}
                />
                <Input
                    placeholder="Column 3"
                    value={stagingRow.column3}
                    onChange={(e) => handleInputChange(e, "column3")}
                />
                <Button onClick={addRow}>Add Row</Button>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Column 1</th>
                        <th>Column 2</th>
                        <th>Column 3</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {rows.map((row, index) => (
                        <tr key={index}>
                            <td>{row.column1}</td>
                            <td>{row.column2}</td>
                            <td>{row.column3}</td>
                            <td>
                                <Button onClick={() => removeRow(index)}>Remove</Button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}