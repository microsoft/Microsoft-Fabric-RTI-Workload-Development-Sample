import React, { useState } from "react";
import { Image } from "@fluentui/react-components";
import { KustoExplorerComponentProps } from "../../App";
import { CallExecuteControlCommand, CallExecuteQuery } from "../../controller/KustoExplorerController";
import { KustoQueryResultComponent } from "./kustoQueryResult";
import { Spinner, SpinnerSize } from "@fluentui/react";

export function KustoExplorerComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KustoExplorerComponentProps) {
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
        //TODO add logic for enable/disable cancel button
        setQueryResult(null);
    };

    function isDisabledExecuteQueryButton(): boolean {
        if (queryToExecute == null || queryToExecute.trim() == "") {
            return true;
        }
        if (isQueryInProgress) {
            return true;
        }
        return false;
    }

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
            {
                isQueryInProgress ?
                    (
                        <div className="run-query-explore-results">
                            <Spinner label="Query in progress" size={SpinnerSize.large} />
                        </div>
                    )
                    :
                    (
                        queryResult ?
                            (
                                <KustoQueryResultComponent rawQueryResult={queryResult} />
                            ) :
                            (
                                <div className="run-query-explore-results">
                                    <Image src="../../../internalAssets/Loupe.svg" />
                                    <h2>Run a query and explore the results here</h2>
                                </div>
                            )
                    )
            }
        </div>
    );
}