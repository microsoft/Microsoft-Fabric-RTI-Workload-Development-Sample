import React, { useState } from "react";
import { KustoExplorerProps } from "../../App";
import { Stack } from "@fluentui/react";

export function KustoExplorerComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseQueryUrl }: KustoExplorerProps) {
    const [queryResult, setQueryResult] = useState<string>("");


    const runQuery = () => {
        setQueryResult('Query result will be displayed here...');
    };

    const cancelQuery = () => {
        setQueryResult('');
    };

    return (
        <>
            <Stack className={`kusto-explorer`}>
                <div className='section'>
                    <h2>Kusto Explorer</h2>
                    <div>
                        <label className='label-key'>KQL Database name:</label>
                        <label className='label-value'>{kqlDatabaseDisplayName}</label>
                    </div>
                    <div>
                        <label className='label-key'>KQL Database Query Url:</label>
                        <label className='label-value'>{kqlDatabaseQueryUrl}</label>
                    </div>
                    <textarea
                        className='kusto-query-input'
                        rows={5}
                        placeholder='Type your query here...'
                    />
                    <div className='button-group'>
                        <button className='run-query-button' onClick={runQuery}>Run Query</button>
                        <button className='cancel-query-button' onClick={cancelQuery}>Cancel Query</button>
                    </div>
                    <div className='result-table'>
                        {queryResult}
                    </div>
                </div>
            </Stack>
        </>
    );
}