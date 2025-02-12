import {
    ItemCreateContext,
    createWorkloadClient,
    DialogType,
    InitParams,
    NotificationToastDuration,
    NotificationType,
} from '@ms-fabric/workload-client';

import * as Controller from './controller/SampleWorkloadController';

export async function initialize(params: InitParams) {

    const workloadClient = createWorkloadClient();
    const sampleWorkloadName = process.env.WORKLOAD_NAME;

    workloadClient.action.onAction(async function ({ action, data }) {
        switch (action) {
            /* This is the entry point for the Sample Workload Create experience, 
            as referenced by the Product->CreateExperience->Cards->onClick->action 'open.createSampleWorkload' in the localWorkloadManifest.json manifest.
             This will open a Save dialog, and after a successful creation, the editor experience of the saved sampleWorkload item will open
            */
            case 'open.createSampleWorkload':
                const { workspaceObjectId } = data as ItemCreateContext;
                return workloadClient.dialog.open({
                    workloadName: sampleWorkloadName,
                    dialogType: DialogType.IFrame,
                    route: {
                        path: `/sample-workload-create-dialog/${workspaceObjectId}`,
                    },
                    options: {
                        width: 360,
                        height: 340,
                        hasCloseButton: false
                    },
                });

            /**
             * This opens the Frontend-only experience, allowing to experiment with the UI without the need for CRUD operations.
             * This experience still allows saving the item, if the Backend is connected and registered
             */
            case 'open.createSampleWorkloadFrontendOnly':
                return workloadClient.page.open({
                    workloadName: sampleWorkloadName,
                    route: {
                        path: `/sample-workload-frontend-only`,
                    },
                });

            case 'sample.Action':
                return Controller.callNotificationOpen(
                    'Action executed',
                    'Action executed via API',
                    NotificationType.Success,
                    NotificationToastDuration.Medium,
                    workloadClient);

            default:
                throw new Error('Unknown action received');
        }
    });
}
