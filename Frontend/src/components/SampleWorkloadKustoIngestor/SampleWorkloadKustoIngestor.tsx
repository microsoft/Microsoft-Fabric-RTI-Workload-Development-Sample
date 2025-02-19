import React from "react";
import { KustoComponentProps } from "../../App";

export function KustoIngestorComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseQueryUrl }: KustoComponentProps) {
    return (
        <div className='kusto-ingestor'>
            <h2>Kusto Ingestion Wizard</h2>
            <div>
                <label>Coming soon! Stay tuned as we present Kusto streaming and queued ingestion capabilities.</label>
            </div>
            {/* <Divider alignContent="start" className="divider">
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
                <b>Records generator</b>
            </Divider> */}
        </div>
    );
}