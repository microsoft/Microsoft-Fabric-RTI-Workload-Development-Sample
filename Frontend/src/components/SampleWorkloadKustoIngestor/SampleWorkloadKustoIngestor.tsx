import React, { useState } from "react";
import { KustoComponentProps } from "../../App";
import { Divider, Button, Input } from "@fluentui/react-components";
import { DismissCircle48Regular, AddCircle32Regular } from "@fluentui/react-icons";

interface IotDataTableRow {
    timestamp: string;
    name: string;
    value: string;
}

export function KustoIngestorComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KustoComponentProps) {
    const [rows, setRows] = useState<IotDataTableRow[]>([]);
    const [stagingRow, setStagingRow] = useState<IotDataTableRow>(generateRandomRow());
    const targetTable = "IotData";
    const maxRows = 20;

    function generateRandomRow(): IotDataTableRow {
        const timestamp = new Date().toISOString();
        const name = `sensor-${Math.floor(Math.random() * 1000)}`;
        const value = (Math.random()).toString();
        return { timestamp, name, value };
    }

    function addRow() {
        if (rows.length < maxRows) {
            setRows([...rows, stagingRow]);
            setStagingRow(generateRandomRow());
        }
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
            <div className="input-container">
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
                <Button
                    icon={<AddCircle32Regular />}
                    onClick={addRow}
                    disabled={rows.length >= maxRows}
                    className="add-row-button">
                </Button>
            </div>
            <div className="records-container">
                <table>
                    <thead>
                        <tr>
                            <th>Timestamp</th>
                            <th>Name</th>
                            <th>Value</th>
                            <th className="actions-column"></th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows.map((row, index) => (
                            <tr key={index}>
                                <td>{row.timestamp}</td>
                                <td>{row.name}</td>
                                <td>{row.value}</td>
                                <td className="actions-column">
                                    <Button
                                        icon={<DismissCircle48Regular />}
                                        className="dismiss-button"
                                        onClick={() => removeRow(index)}
                                    />
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}