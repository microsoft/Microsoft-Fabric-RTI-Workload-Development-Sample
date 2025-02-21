import React, { useState } from "react";
import { KustoIngestorComponentProps } from "../../App";
import { Divider, Button, Input, Tooltip, RadioGroup, Radio } from "@fluentui/react-components";
import { DismissCircle48Regular, AddCircle32Regular } from "@fluentui/react-icons";
import { CallQueuedIngest } from "../../controller/KustoIngestorController";

interface IotDataTableRow {
    timestamp: string;
    name: string;
    value: string;
}

export function KustoIngestorComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseIngestionUrl }: KustoIngestorComponentProps) {
    const sampleWorkloadBEUrl = process.env.WORKLOAD_BE_URL;
    const [rows, setRows] = useState<IotDataTableRow[]>([]);
    const [stagingRow, setStagingRow] = useState<IotDataTableRow>(generateRandomRow());
    const [ingestionType, setIngestionType] = useState<string>("streaming");
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

    async function onIngestButtonClick() {
        try {
            const contentToIngest = convertRowsToCSV(rows);
            await CallQueuedIngest(
                sampleWorkloadBEUrl,
                kqlDatabaseIngestionUrl,
                kqlDatabaseItemId,
                targetTable,
                contentToIngest,
                workloadClient
            );
        }
        catch (error) {
            console.error("Error executing query:", error);
        }
    }

    function isDisabledIngestButton(): boolean {
        return rows.length === 0;
    }

    function isDisabledAddRowButton(): boolean {
        return rows.length >= maxRows;
    }

    function hasRows(): boolean {
        return rows.length > 0;
    }

    function convertRowsToCSV(rows: IotDataTableRow[]): string {
        const csvRows = rows.map(row => `${row.timestamp},${row.name},${row.value}`);
        return csvRows.join("\n");
    }

    return (
        <div className='kusto-ingestor'>
            <h2>Kusto Ingestion Wizard</h2>
            <Divider alignContent="start" className="divider">
                <b>Ingestion target</b>
            </Divider>
            <div>
                <label className='label-key'>KQL Database Ingestion Url:</label>
                <label className='label-value'>{kqlDatabaseIngestionUrl}</label>
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
                <Tooltip content={rows.length >= maxRows ? `${maxRows} limit reached` : "add a new record"} relationship={"label"}>
                    <Button
                        icon={<AddCircle32Regular />}
                        onClick={addRow}
                        disabled={isDisabledAddRowButton()}
                        className="add-row-button">
                    </Button>
                </Tooltip>
            </div>
            {hasRows() && (
                <>
                    <div className="rows-container">
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
                                                className="remove-row-button"
                                                onClick={() => removeRow(index)}
                                            />
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                    <Divider alignContent="start" className="divider">
                        <b>Data Ingestion</b>
                    </Divider>
                    <div className="data-ingestion">
                        <Button
                            className="ingest-button"
                            onClick={onIngestButtonClick}
                            disabled={isDisabledIngestButton()}>
                            Ingest Data
                        </Button>
                        <RadioGroup
                            className="ingestion-type-radio-group"
                            value={ingestionType}
                            onChange={(_e, data) => setIngestionType(data.value)}
                        >
                            <Radio value="streaming" label="Streaming Ingestion" />
                            <Radio value="queued" label="Queued Ingestion" />
                        </RadioGroup>
                    </div>
                </>
            )}
        </div>
    );
}