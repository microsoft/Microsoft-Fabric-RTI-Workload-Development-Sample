import React, { useState } from "react";
import { Image, Divider } from "@fluentui/react-components";
import { KqlExplorerComponentProps } from "../../App";
import { CallExecuteControlCommand, CallExecuteQuery } from "../../controller/KqlExplorerController";
import { KqlQueryResultComponent } from "./KqlQueryResult";
import { Spinner, SpinnerSize, MessageBar, MessageBarType } from "@fluentui/react";

export function KqlExplorerComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KqlExplorerComponentProps) {
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
        <div className='kql-explorer'>
            <h2>KQL Explorer</h2>
            <div className="message-bar-container">
                <MessageBar
                    messageBarType={MessageBarType.info}
                    isMultiline={true}
                >
                    Execute KQL queries on the KQL database and explore the ingested data.
                </MessageBar>
            </div>
            <Divider alignContent="start" className="divider">
                <b>KQL Database details</b>
            </Divider>
            <div>
                <label className='label-key'>KQL Database name:</label>
                <label className='label-value'>{kqlDatabaseDisplayName}</label>
            </div>
            <div>
                <label className='label-key'>KQL Database Query Url:</label>
                <label className='label-value'>{kqlDatabaseQueryUrl}</label>
            </div>
            <textarea
                className='kql-query-input'
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
                                <KqlQueryResultComponent rawQueryResult={queryResult} />
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