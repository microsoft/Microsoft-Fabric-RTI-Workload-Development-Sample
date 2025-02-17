import React, { useState } from "react";
import { PageProps } from "../../App";
import { Stack } from "@fluentui/react";
import { Subtitle2 } from "@fluentui/react-components";

export function KustoExplorerComponent({ workloadClient }: PageProps) {
    const [queryResult, setQueryResult] = useState<string>("");


    const runQuery = () => {
        // Placeholder for running the query
        setQueryResult('Query result will be displayed here...');
    };

    const cancelQuery = () => {
        // Placeholder for canceling the query
        setQueryResult('');
    };

    return (
        <>
            <Stack className={`kusto-explorer`}>
                <div className='section'>
                    <h1>Kusto Explorer</h1>
                    {<Subtitle2>KQL Database name:</Subtitle2>}
                    {<Subtitle2>KQL Database Query Url:</Subtitle2>}
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
            {/* {loadingStatus === "loading" && <Spinner className="main-body" label="Loading Tables" />} }
                {/* {selectedLakehouse && loadingStatus == "idle" && isExplorerVisible && (
                    <Tree
                        aria-label="Tables in Lakehouse"
                        className="selector-body"
                        size="medium"
                        defaultOpenItems={["Lakehouse", "Tables", "Schemas"]}
                    >
                        <div className="tree-container">
                            <TreeItem className="selector-tree-item" itemType="branch" value="Lakehouse">
                                <Tooltip relationship="label" content={selectedLakehouse.displayName}>
                                    <TreeItemLayout
                                        aside={
                                            <Button appearance="subtle" icon={<ArrowSwap20Regular />} onClick={onDatahubClicked}></Button>
                                        }
                                    >
                                        {selectedLakehouse.displayName}
                                    </TreeItemLayout>
                                </Tooltip>
                                <Tree className="tree" selectionMode="single">
                                    {hasSchema &&
                                        <TableTreeWithSchema
                                            allTablesInLakehouse={tablesInLakehouse}
                                            onSelectTableCallback={tableSelectedCallback} />
                                    }
                                    {!hasSchema &&
                                        <TableTreeWithoutSchema
                                            allTablesInLakehouse={tablesInLakehouse}
                                            onSelectTableCallback={tableSelectedCallback} />
                                    }
                                </Tree>
                            </TreeItem>
                        </div>
                    </Tree>
                )} */}
            {/* {loadingStatus === "error" && isExplorerVisible && <div className="main-body">l
                    <Subtitle2>Error loading tables</Subtitle2>
                    <p>Do you have permission to view this lakehouse?</p>
                </div>} */}
            {/* </Stack> */}
        </>
    );
}