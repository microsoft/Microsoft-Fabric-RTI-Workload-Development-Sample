import { EventstreamComponentProps } from "../../App";
import { Divider } from "@fluentui/react-components";
import React from "react";

export function EventstreamComponent({ eventstreamDisplayName, eventstreamItemId }: EventstreamComponentProps) {
    return (
        <div className='eventstream-editor'>
            <h2>Eventstream</h2>
            <div>
                <label className="description-label">Ingest events into the EventStream to be processed and ingested into the KQL database</label>
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
        </div>
    );
}