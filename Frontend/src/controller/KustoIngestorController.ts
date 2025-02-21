import { WorkloadClientAPI, AccessToken } from "@ms-fabric/workload-client";
import { callAuthAcquireAccessToken } from "./SampleWorkloadController";

export async function CallQueuedIngest(workloadBEUrl: string, kqlDatabaseIngestionUrl: string, kqlDatabaseItemId: string, tableName: string, content: string, workloadClient: WorkloadClientAPI) : Promise<object[]> {
    try {
        const accessToken: AccessToken = await callAuthAcquireAccessToken(workloadClient);
        const response: Response = await fetch(`${workloadBEUrl}/KqlDatabases/queuedIngest`, {
            method: `POST`,
            headers: {
                'Authorization': 'Bearer ' + accessToken.token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                'IngestionServiceUri': kqlDatabaseIngestionUrl,
                'KqlDatabaseItemId': kqlDatabaseItemId,
                'TableName': tableName,
                'Content': content
            })
        });
        if (!response.ok) {
            // Handle non-successful responses here
            const errorMessage: string = await response.text();
            console.error(`Error calling QueuedIngest API: ${errorMessage}`);
            throw new Error(`Error calling QueuedIngest API: ${errorMessage}`);
            
            //TODO copy logic from platform project
            //return await handleException(errorMessage, workloadClient, false /* isRetry */, true /* isDirectWorkloadCall */, CallQueuedIngest, workloadBEUrl, queryUrl, databaseName, query, setClientRequestId);
        }

        const result: object[] = await response.json();

        console.log('*** Successfully called QueuedIngest API');
        return result;
    }
    catch (error) {
        console.error('Error in CallQueuedIngest:', error);
        throw error;
    }
}


export async function CallStreamingIngest(workloadBEUrl: string, kqlDatabaseIngestionUrl: string, kqlDatabaseItemId: string, tableName: string, content: string, workloadClient: WorkloadClientAPI) : Promise<object[]> {
    try {
        const accessToken: AccessToken = await callAuthAcquireAccessToken(workloadClient);
        const response: Response = await fetch(`${workloadBEUrl}/KqlDatabases/streamingIngest`, {
            method: `POST`,
            headers: {
                'Authorization': 'Bearer ' + accessToken.token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                'IngestionServiceUri': kqlDatabaseIngestionUrl,
                'KqlDatabaseItemId': kqlDatabaseItemId,
                'TableName': tableName,
                'Content': content
            })
        });
        if (!response.ok) {
            // Handle non-successful responses here
            const errorMessage: string = await response.text();
            console.error(`Error calling streamingIngest API: ${errorMessage}`);
            throw new Error(`Error calling streamingIngest API: ${errorMessage}`);
            
            //TODO copy logic from platform project
            //return await handleException(errorMessage, workloadClient, false /* isRetry */, true /* isDirectWorkloadCall */, CallStreamingIngest, workloadBEUrl, queryUrl, databaseName, query, setClientRequestId);
        }

        const result: object[] = await response.json();

        console.log('*** Successfully called streamingIngest API');
        return result;
    }
    catch (error) {
        console.error('Error in CallStreamingIngest:', error);
        throw error;
    }
}