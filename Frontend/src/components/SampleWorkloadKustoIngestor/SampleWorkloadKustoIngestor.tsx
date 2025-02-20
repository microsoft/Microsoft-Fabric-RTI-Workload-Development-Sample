import React, { useState } from "react";
import { KustoComponentProps } from "../../App";
import { Divider, Button, Input } from "@fluentui/react-components";

interface IotDataTableRow {
    timestamp: string;
    name: string;
    value: string;
}

export function KustoIngestorComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KustoComponentProps) {
    const [rows, setRows] = useState<IotDataTableRow[]>([]);
    const [stagingRow, setStagingRow] = useState<IotDataTableRow>({
        timestamp: "",
        name: "",
        value: ""
    });
    const targetTable = "IotData";

    function addRow() {
        setRows([...rows, stagingRow]);
        setStagingRow({ timestamp: "", name: "", value: "" });
    }

    function removeRow(index: number) {
        const newRows = rows.filter((_, i) => i !== index);
        setRows(newRows);
    }

    function handleInputChange(e: React.ChangeEvent<HTMLInputElement>, column: keyof IotDataTableRow) {
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
                <b>Rows generator</b>
            </Divider>
            <div>
                <Input
                    placeholder="Timestamp"
                    value={stagingRow.timestamp}
                    onChange={(e) => handleInputChange(e, "timestamp")}
                />
                <Input
                    placeholder="Name"
                    value={stagingRow.name}
                    onChange={(e) => handleInputChange(e, "name")}
                />
                <Input
                    placeholder="Value"
                    value={stagingRow.value}
                    onChange={(e) => handleInputChange(e, "value")}
                />
                <Button onClick={addRow}>Add Row</Button>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Timestamp</th>
                        <th>Name</th>
                        <th>Value</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {rows.map((row, index) => (
                        <tr key={index}>
                            <td>{row.timestamp}</td>
                            <td>{row.name}</td>
                            <td>{row.value}</td>
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