import { EventstreamComponentProps } from "../../App";
import { Button, Divider } from "@fluentui/react-components";
import React, { useState } from "react";
import { SampleWorkloadDataGenerator } from "../SampleWorkloadDataGenerator/SampleWorkloadDataGenerator";
import { MessageBar } from "@fluentui/react";
import { MessageBarType } from "@fluentui/react";
import { CallSendEvents } from "../../controller/EventstreamController";

export function EventstreamComponent({ workloadClient, workspaceObjectId, eventstreamDisplayName, eventstreamItemId }: EventstreamComponentProps) {
    const sampleWorkloadBEUrl = process.env.WORKLOAD_BE_URL;
    const targetTable = "IotData";

    const [sendSuccess, setSendSuccess] = useState<boolean | null>(null);
    const [isSendInProgress, setIsSendInProgress] = useState<boolean>(false);

    const dataGenerator = SampleWorkloadDataGenerator();
    const { UI: DataGeneratorUI, hasRows, removeAllRows, getRows } = dataGenerator;

    async function onSendButtonClick() {
        try {
            setSendSuccess(null);
            setIsSendInProgress(true);
            const contentToIngest = getRows();
            await CallSendEvents(
                sampleWorkloadBEUrl,
                workspaceObjectId,
                eventstreamItemId,
                contentToIngest,
                workloadClient
            );
            setSendSuccess(true);
            removeAllRows();
        }
        catch (error) {
            console.error("Error ingesting data:", error);
            setSendSuccess(false);
        } finally {
            setIsSendInProgress(false);
        }
    }

    function isDisabledSendButton(): boolean {
        return !hasRows() || isSendInProgress;
    }

    return (
        <div className='eventstream-editor'>
            <h2>Eventstream</h2>
            <div className="message-bar-container">
                <MessageBar
                    messageBarType={MessageBarType.info}
                    isMultiline={true}
                >
                    Ingest events into Eventstream to be processed and ingested into the KQL database
                </MessageBar>
            </div>
            <Divider alignContent="start" className="divider">
                <b>Eventstream details</b>
            </Divider>
            <div>
                <label className='label-key'>Eventstream Name:</label>
                <label className='label-value'>{eventstreamDisplayName}</label>
            </div>
            <div>
                <label className='label-key'>Eventstream Id:</label>
                <label className='label-value'>{eventstreamItemId}</label>
            </div>
            <Divider alignContent="start" className="divider">
                <b>Events generator</b>
            </Divider>
            {DataGeneratorUI}
            {hasRows() && (
                <>
                    <Divider alignContent="start" className="divider">
                        <b>Events ingestion</b>
                    </Divider>
                    <div className="events-ingestion">
                        <Button
                            className={`send-button ${isSendInProgress ? 'disabled' : ''}`}
                            onClick={onSendButtonClick}
                            disabled={isDisabledSendButton()}>
                            {isSendInProgress ? 'Sending...' : 'Send Events'}
                        </Button>
                    </div>
                </>
            )}
            {sendSuccess !== null && (
                <div className="message-bar-container">
                    <MessageBar
                        messageBarType={sendSuccess ? MessageBarType.success : MessageBarType.error}
                        isMultiline={true}
                        onDismiss={() => setSendSuccess(null)}
                    >
                        {sendSuccess ?
                            `Events sent successfully to Eventstream! Query ${targetTable} to see the new records in the table, note, it may take a few minutes to see the new records.`
                            : "Error sending events."
                        }
                    </MessageBar>
                </div>
            )}
        </div>
    );
}
