import { WorkloadItem} from "./models/SampleWorkloadModel";
import { GetItemResult } from "@ms-fabric/workload-client";

export function convertGetItemResultToWorkloadItem<T>(item: GetItemResult): WorkloadItem<T> {
    let payload: T;
    if (item.workloadPayload) {
        try {
            payload = JSON.parse(item.workloadPayload);
            console.log(`Parsed payload of item ${item.objectId} is ${payload}`);
        } catch (payloadParseError) {
            console.error(`Failed parsing payload for item ${item.objectId}, payloadString: ${item.workloadPayload}`, payloadParseError);
        }
    }

    return {
        id: item.objectId,
        workspaceId: item.folderObjectId,
        type: item.itemType,
        displayName: item.displayName,
        description: item.description,
        extendedMetadata: payload,
        createdBy: item.createdByUser.name,
        createdDate: item.createdDate,
        lastModifiedBy: item.modifiedByUser.name,
        lastModifiedDate: item.lastUpdatedDate
    };
}