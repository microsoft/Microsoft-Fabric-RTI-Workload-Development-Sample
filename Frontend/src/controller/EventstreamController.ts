import { AccessToken, WorkloadClientAPI } from "@ms-fabric/workload-client";
import { callAuthAcquireAccessToken } from "./SampleWorkloadController";

export async function CallSendEvents(workloadBEUrl: string, workspaceId: string, eventstreamItemId: string, events: object[], workloadClient: WorkloadClientAPI): Promise<void> {
    try {
        const accessToken: AccessToken = await callAuthAcquireAccessToken(workloadClient);
        const response: Response = await fetch(`${workloadBEUrl}/workspaces/${workspaceId}/eventstreams/${eventstreamItemId}/sendEvents`, {
            method: `POST`,
            headers: {
                'Authorization': 'Bearer ' + accessToken.token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                'Events': events
            })
        });
        if (!response.ok) {
            // Handle non-successful responses here
            const errorMessage: string = await response.text();
            console.error(`Error calling QueuedIngest API: ${errorMessage}`);
            throw new Error(`Error calling QueuedIngest API: ${errorMessage}`);
        }
        console.log('*** Successfully called sendEvents API');
    }
    catch (error) {
        console.error('Error in CallSendEvents:', error);
        throw error;
    }
}
