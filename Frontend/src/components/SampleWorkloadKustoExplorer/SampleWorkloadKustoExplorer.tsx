import React, { useState } from "react";
import { KustoExplorerProps } from "../../App";
import { CallExecuteControlCommand, CallExecuteQuery } from "../../controller/KustoExplorerController";
import { KustoQueryResultComponent } from "./kustoQueryResult";

export function KustoExplorerComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KustoExplorerProps) {
    const sampleWorkloadBEUrl = process.env.WORKLOAD_BE_URL;
    const [queryResult, setQueryResult] = useState<object[]>();
    const [queryToExecute, setQueryToExecute] = useState<string>("");

    const onRunQueryButtonClick = async () => {
        const trimmedQuery = queryToExecute.trimStart();
        let result = null;
        if (trimmedQuery.startsWith(".")) {
            result = await CallExecuteControlCommand(
                sampleWorkloadBEUrl,
                kqlDatabaseQueryUrl,
                kqlDatabaseItemId,
                queryToExecute,
                workloadClient
            );
        }
        else {
            result = await CallExecuteQuery(
                sampleWorkloadBEUrl,
                kqlDatabaseQueryUrl,
                kqlDatabaseItemId,
                queryToExecute,
                workloadClient
            );
        }

        if (result) {
            setQueryResult(result);
        }
    };

    const cancelQuery = () => {
        setQueryResult(null);
    };

    //TODO add logic for enable/disable query button
    //TODO add logic for enable/disable cancel button
    //TODO add add spinner for query running in background

    return (
        <div className='kusto-explorer'>
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
                onChange={(e) => setQueryToExecute(e.target.value)}
            />
            <div className='button-group'>
                <button className='run-query-button' onClick={onRunQueryButtonClick}>Run Query</button>
                <button className='cancel-query-button' onClick={cancelQuery}>Cancel Query</button>
            </div>
            {queryResult && (<KustoQueryResultComponent rawQueryResult={queryResult} />)}
        </div>
    );
}