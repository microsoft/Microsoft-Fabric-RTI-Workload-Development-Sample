import { WorkloadClientAPI, AccessToken } from "@ms-fabric/workload-client";
import { callAuthAcquireAccessToken } from "./SampleWorkloadController";

/**
 * Calls the CallExecuteQuery endpoint to perform a query on KQL database.
 * 
 * @param {string} kqlDatabaseQueryUrl - The database query url.
 * @param {string} kqlDatabaseItemId - The KQL database item id.
 * @param {string} query - The query to execute.
 * @param {WorkloadClientAPI} workloadClient - An instance of the WorkloadClientAPI.
 * @returns {Promise<object[]>} A Promise that resolves to an object containing the queries result.
 */
export async function CallExecuteQuery(workloadBEUrl: string, kqlDatabaseQueryUrl: string, kqlDatabaseItemId: string, query: string, workloadClient: WorkloadClientAPI) : Promise<object[]> {
    try {
        const accessToken: AccessToken = await callAuthAcquireAccessToken(workloadClient);
        const response: Response = await fetch(`${workloadBEUrl}/KqlDatabases/query`, {
            method: `POST`,
            headers: {
                'Authorization': 'Bearer ' + accessToken.token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                'KqlDatabaseQueryUrl': kqlDatabaseQueryUrl,
                'KqlDatabaseItemId': kqlDatabaseItemId,
                'Query': query
            })
        });
        if (!response.ok) {
            // Handle non-successful responses here
            const errorMessage: string = await response.text();
            console.error(`Error calling ExecuteQuery API: ${errorMessage}`);
            throw new Error(`Error calling ExecuteQuery API: ${errorMessage}`);
            
            //TODO copy logic from platform project
            //return await handleException(errorMessage, workloadClient, false /* isRetry */, true /* isDirectWorkloadCall */, CallExecuteQuery, workloadBEUrl, queryUrl, databaseName, query, setClientRequestId);
        }

        const result: object[] = await response.json();

        console.log('*** Successfully called ExecuteQuery API');
        return result;
    }
    catch (error) {
        console.error('Error in CallExecuteQuery:', error);
        return null;
    }
}

/**
 * Calls the CallExecuteControlCommand endpoint to execute a KQL command on KQL database.
 * 
 * @param {string} kqlDatabaseQueryUrl - The database query url.
 * @param {string} kqlDatabaseItemId - The KQL database item id.
 * @param {string} command - The KQL command to execute.
 * @param {WorkloadClientAPI} workloadClient - An instance of the WorkloadClientAPI.
 * @returns {Promise<object[]>} A Promise that resolves to an object containing the queries result.
 */
export async function CallExecuteControlCommand(workloadBEUrl: string, kqlDatabaseQueryUrl: string, kqlDatabaseItemId: string, command: string, workloadClient: WorkloadClientAPI) : Promise<object[]> {
    try {
        const accessToken: AccessToken = await callAuthAcquireAccessToken(workloadClient);
        const response: Response = await fetch(`${workloadBEUrl}/KqlDatabases/mgmt`, {
            method: `POST`,
            headers: {
                'Authorization': 'Bearer ' + accessToken.token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                'KqlDatabaseQueryUrl': kqlDatabaseQueryUrl,
                'KqlDatabaseItemId': kqlDatabaseItemId,
                'Command': command
            })
        });
        if (!response.ok) {
            // Handle non-successful responses here
            const errorMessage: string = await response.text();
            console.error(`Error calling ExecuteControlCommand API: ${errorMessage}`);
            throw new Error(`Error calling ExecuteControlCommand API: ${errorMessage}`);
            
            //TODO copy logic from platform project
            //return await handleException(errorMessage, workloadClient, false /* isRetry */, true /* isDirectWorkloadCall */, CallExecuteQuery, workloadBEUrl, queryUrl, databaseName, query, setClientRequestId);
        }

        const result: object[] = await response.json();

        console.log('*** Successfully called ExecuteControlCommand API');
        return result;
    }
    catch (error) {
        console.error('Error in ExecuteControlCommand:', error);
        return null;
    }
}