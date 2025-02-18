import React, { useState } from "react";
import { KustoExplorerProps } from "../../App";
import { CallExecuteControlCommand, CallExecuteQuery } from "../../controller/KustoExplorerController";
import { KustoQueryResultComponent } from "./kustoQueryResult";

export function KustoExplorerComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KustoExplorerProps) {
    const sampleWorkloadBEUrl = process.env.WORKLOAD_BE_URL;
    const [queryResult, setQueryResult] = useState<object[]>();
    const [queryToExecute, setQueryToExecute] = useState<string>("");
    const [isQueryInProgress, setIsQueryInProgress] = useState<boolean>(false);

    async function onRunQueryButtonClick() {
        const trimmedQuery = queryToExecute.trimStart();
        let result = null;
        setIsQueryInProgress(true);
        try {
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
        }
        catch (error) {
            setQueryResult(null);
            console.error("Error executing query:", error);
        }
        finally {
            setIsQueryInProgress(false);
        }
    };

    function cancelQuery() {
        setQueryResult(null);
    };

    function isDisabledExecuteQueryButton(): boolean {
        if (queryToExecute == null || queryToExecute.trim() == "") {
            return true;
        }
        if(isQueryInProgress){
            return true;
        }
        return false;
    }

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
                <button className={isDisabledExecuteQueryButton() ? 'run-query-button-disabled' : 'run-query-button'}
                    onClick={onRunQueryButtonClick}
                    disabled={isDisabledExecuteQueryButton()}
                >Run Query</button>
                <button className='cancel-query-button' onClick={cancelQuery}>Cancel Query</button>
            </div>
            <KustoQueryResultComponent rawQueryResult={queryResult} />
        </div>
    );
}