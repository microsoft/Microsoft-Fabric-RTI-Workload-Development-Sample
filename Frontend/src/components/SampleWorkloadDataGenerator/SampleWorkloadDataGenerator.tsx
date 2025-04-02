import { Button, Input, Tooltip } from "@fluentui/react-components";
import { AddCircle32Regular, DismissCircle48Regular } from "@fluentui/react-icons";
import React, { useState } from "react";

interface IotDataTableRow {
    timestamp: string;
    name: string;
    value: string;
}

export function SampleWorkloadDataGenerator() {
    const maxRows = 20;
    const [rows, setRows] = useState<IotDataTableRow[]>([]);
    const [stagingRow, setStagingRow] = useState<IotDataTableRow>(generateRandomRow());

    function hasRows(): boolean {
        return rows.length > 0;
    }

    function removeAllRows() {
        setRows([]);    
        setStagingRow(generateRandomRow());
    }

    function getRowsAsCSV(): string {
        const csvRows = rows.map(row => `${row.timestamp},${row.name},${row.value}`);
        return csvRows.join("\n");
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

    function generateRandomRow(): IotDataTableRow {
        const timestamp = new Date().toISOString();
        const name = `sensor-${Math.floor(Math.random() * 1000)}`;
        const value = (Math.random()).toString();
        return { timestamp, name, value };
    }

    function handleInputChange(e: React.ChangeEvent<HTMLInputElement>, column: keyof IotDataTableRow) {
        setStagingRow({ ...stagingRow, [column]: e.target.value });
    }

    function isDisabledAddRowButton(): boolean {
        return rows.length >= maxRows;
    }

    return {
        UI: (
            <div className='data-generator'>
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
            </div>
        ),
        hasRows,
        removeAllRows,
        getRowsAsCSV
    };
}