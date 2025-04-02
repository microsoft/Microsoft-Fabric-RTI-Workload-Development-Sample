import React, { useState } from "react";
import { KqlIngestorComponentProps } from "../../App";
import { Divider, Button, RadioGroup, Radio } from "@fluentui/react-components";
import { MessageBar } from "@fluentui/react";
import { MessageBarType } from "@fluentui/react";
import { CallQueuedIngest, CallStreamingIngest } from "../../controller/KqlIngestorController";
import { SampleWorkloadDataGenerator } from "../SampleWorkloadDataGenerator/SampleWorkloadDataGenerator";

export function KqlIngestorComponent({ workloadClient, kqlDatabaseDisplayName, kqlDatabaseItemId, kqlDatabaseIngestionUrl }: KqlIngestorComponentProps) {
    const sampleWorkloadBEUrl = process.env.WORKLOAD_BE_URL;
    const [ingestionType, setIngestionType] = useState<string>("streaming");
    const [ingestionSuccess, setIngestionSuccess] = useState<boolean | null>(null);
    const [isIngestionInProgress, setIsIngestionInProgress] = useState<boolean>(false);
    const targetTable = "IotData";

    const dataGenerator = SampleWorkloadDataGenerator();
    const { UI: DataGeneratorUI, hasRows, removeAllRows, getRowsAsCSV } = dataGenerator;

    async function onIngestButtonClick() {
        try {
            setIngestionSuccess(null);
            setIsIngestionInProgress(true);
            const contentToIngest = getRowsAsCSV();
            if (ingestionType === "queued") {
                await CallQueuedIngest(
                    sampleWorkloadBEUrl,
                    kqlDatabaseIngestionUrl,
                    kqlDatabaseItemId,
                    targetTable,
                    contentToIngest,
                    workloadClient
                );
            } else if (ingestionType === "streaming") {
                await CallStreamingIngest(
                    sampleWorkloadBEUrl,
                    kqlDatabaseIngestionUrl,
                    kqlDatabaseItemId,
                    targetTable,
                    contentToIngest,
                    workloadClient
                );
            }
            setIngestionSuccess(true);
            removeAllRows();
        }
        catch (error) {
            console.error("Error ingesting data:", error);
            setIngestionSuccess(false);
        } finally {
            setIsIngestionInProgress(false);
        }
    }

    function isDisabledIngestButton(): boolean {
        return !hasRows() || isIngestionInProgress;
    }

    return (
        <div className='kql-ingestor'>
            <h2>KQL Ingestion Wizard</h2>
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
            {DataGeneratorUI}
            {hasRows() && (
                <>
                    <Divider alignContent="start" className="divider">
                        <b>Data Ingestion</b>
                    </Divider>
                    <div className="data-ingestion">
                        <Button
                            className={`ingest-button ${isIngestionInProgress ? 'disabled' : ''}`}
                            onClick={onIngestButtonClick}
                            disabled={isDisabledIngestButton()}>
                            {isIngestionInProgress ? 'Ingesting...' : 'Ingest Data'}
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
            {ingestionSuccess !== null && (
                <div className="message-bar-container">
                    <MessageBar
                        messageBarType={ingestionSuccess ? MessageBarType.success : MessageBarType.error}
                        isMultiline={true}
                        onDismiss={() => setIngestionSuccess(null)}
                    >
                        {ingestionSuccess ?
                            `Data ingested successfully! Query ${targetTable} to see the new records in the table, if queued ingestion was used, it may take a few minutes to see the new records.`
                            : "Error ingesting data."
                        }
                    </MessageBar>
                </div>
            )}
        </div>
    );
}