import { ItemLikeV2 } from '@ms-fabric/workload-client';

// Represents an item as defined in the frontend manifest.
export interface ItemManifest {
    name: string;
    displayName: string;
    editor: {
        path: string;
    };
}

// Represents a reference to a fabric item.
export interface ItemReference {
    workspaceId: string;
    id: string;
}

// Represents a generic fabric item with common properties.
export interface GenericItem extends ItemReference {
    type: string;
    displayName: string;
    description: string;
    createdBy?: string;
    createdDate?: Date;
    lastModifiedBy?: string;
    lastModifiedDate?: Date;
}

// Represents a workload item with extended metadata.
export interface WorkloadItem<T> extends GenericItem {
    extendedMetadata?: T;
}

// Represents the core metadata for Item1 stored within the system's storage.
export interface Item1Metadata {
    eventhouseItemId?: string;
    eventhouseDisplayName?: string;
    kqlDatabaseItemId?: string;
    kqlDatabaseDisplayName?: string;
    kqlDatabaseQueryUrl?: string;
    kqlDatabaseIngestionUrl?: string;
}

// Represents extended metadata for item1,tailored for client-side usage.
export interface Item1ClientMetadata extends Item1Metadata {
}

// Represents the item-specific payload passed with the  CreateItem request
export interface CreateItemPayload {
    item1Metadata?: Item1Metadata;
}

// Represents the item-specific payload passed with the  UpdateItem request
export interface UpdateItemPayload {
    item1Metadata?: Item1Metadata;
}

// Represents the item-specific payload returned by the GetItemPayload  request
export interface ItemPayload {
    item1Metadata?: Item1ClientMetadata;
}

// Represents the generic action context received from fabric host
export interface ItemActionContext {
    item: ItemLikeV2;
}

export interface ItemTabActionContext{
    id: string;
}