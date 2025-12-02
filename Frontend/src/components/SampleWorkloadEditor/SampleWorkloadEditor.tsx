import React, { useState, useEffect } from "react";
import { useLocation, useParams } from "react-router-dom";
import { Stack } from "@fluentui/react";
import {
  Button,
  Divider,
  Label,
  TabValue,
  MessageBar,
  MessageBarBody,
  MessageBarTitle,
  MessageBarActions
} from "@fluentui/react-components";

import { useTranslation } from "react-i18next";
import { initializeIcons } from "@fluentui/font-icons-mdl2";
import { AfterNavigateAwayData } from "@ms-fabric/workload-client";
import { ContextProps, PageProps } from "src/App";
import {
  callNavigationNavigate,
  callNavigationBeforeNavigateAway,
  callNavigationAfterNavigateAway,
  callThemeOnChange,
  callSettingsOnChange,
  callItemGet,
  callItemUpdate,
  callItemDelete
} from "../../controller/SampleWorkloadController";
import { Ribbon } from "../SampleWorkloadRibbon/SampleWorkloadRibbon";
import { convertGetItemResultToWorkloadItem } from "../../utils";
import {
  Item1ClientMetadata,
  ItemPayload,
  UpdateItemPayload,
  WorkloadItem,
} from "../../models/SampleWorkloadModel";
import "./../../styles.scss";
import { ItemMetadataNotFound } from "../../models/WorkloadExceptionsModel";
import { KqlExplorerComponent } from "../SampleWorkloadKqlExplorer/SampleWorkloadKqlExplorer";
import { KqlIngestorComponent } from "../SampleWorkloadKqlIngestor/SampleWorkloadKqlIngestor";
import { EventstreamComponent } from "../SampleWorkloadEventstream/SampleWorkloadEventstream";
import { ActivatorComponent } from "../SampleWorkloadActivator/SampleWorkloadActivator";

export function SampleWorkloadEditor(props: PageProps) {
  const { workloadClient } = props;
  const pageContext = useParams<ContextProps>();
  const { pathname } = useLocation();
  const { i18n } = useTranslation();

  // initializing usage of FluentUI icons
  initializeIcons();

  // React state for WorkloadClient APIs
  const [sampleItem, setSampleItem] =
    useState<WorkloadItem<ItemPayload>>(undefined);
  const [eventstreamItemId, setEventstreamItemId] = useState<string>("");
  const [eventstreamDisplayName, setEventstreamDisplayName] = useState<string>("");
  const [, setEventhouseItemId] = useState<string>("");
  const [, setEventhouseDisplayName] = useState<string>("");
  const [kqlDatabaseItemId, setKqlDatabaseItemId] = useState<string>("");
  const [kqlDatabaseDisplayName, setKqlDatabaseDisplayName] = useState<string>("");
  const [kqlDatabaseQueryUrl, setKqlDatabaseQueryUrl] = useState<string>("");
  const [kqlDatabaseIngestionUrl, setKqlDatabaseIngestionUrl] = useState<string>("");

  const [isDirty, setDirty] = useState<boolean>(false);

  const [itemEditorErrorMessage, setItemEditorErrorMessage] = useState<string>("");
  document.body.dir = i18n.dir();

  const [selectedTab, setSelectedTab] = useState<TabValue>("home");

  useEffect(() => {
    // Controller callbacks registrations:
    // register Blocking in Navigate.BeforeNavigateAway (for a forbidden url)
    callNavigationBeforeNavigateAway(workloadClient);

    // register a callback in Navigate.AfterNavigateAway
    callNavigationAfterNavigateAway(afterNavigateCallBack, workloadClient);

    // register Theme.onChange
    callThemeOnChange(workloadClient);

    // register Settings.onChange
    callSettingsOnChange(workloadClient, i18n.changeLanguage);
  }, []);

  useEffect(() => {
    loadDataFromUrl(pageContext, pathname);
  }, [pageContext, pathname]);

  async function afterNavigateCallBack(_event: AfterNavigateAwayData): Promise<void> {
    //clears the data after navigation
    setSampleItem(undefined);
    return;
  }

  // callback functions called by UI controls below

  async function loadDataFromUrl(
    pageContext: ContextProps,
    pathname: string
  ): Promise<void> {
    if (pageContext.itemObjectId) {
      // for Edit scenario we get the itemObjectId and then load the item via the workloadClient SDK
      try {
        const getItemResult = await callItemGet(
          pageContext.itemObjectId,
          workloadClient
        );
        const item =
          convertGetItemResultToWorkloadItem<ItemPayload>(getItemResult);

        setSampleItem(item);

        // load extendedMetadata
        const item1Metadata: Item1ClientMetadata =
          item.extendedMetadata.item1Metadata;
        setEventstreamItemId(item1Metadata?.eventstreamItemId);
        setEventstreamDisplayName(item1Metadata?.eventstreamDisplayName);
        setEventhouseItemId(item1Metadata?.eventhouseItemId);
        setEventhouseDisplayName(item1Metadata?.eventhouseDisplayName);
        setKqlDatabaseItemId(item1Metadata?.kqlDatabaseItemId);
        setKqlDatabaseDisplayName(item1Metadata?.kqlDatabaseDisplayName);
        setKqlDatabaseQueryUrl(item1Metadata?.kqlDatabaseQueryUrl);
        setKqlDatabaseIngestionUrl(item1Metadata?.kqlDatabaseIngestionUrl);

        setItemEditorErrorMessage("");
      } catch (error) {
        clearItemData();
        if (error?.ErrorCode === ItemMetadataNotFound) {
          setItemEditorErrorMessage(error?.Message);
          return;
        }

        console.error(
          `Error loading the Item (object ID:${pageContext.itemObjectId}`,
          error
        );
      }
    } else {
      console.log(`non-editor context. Current Path: ${pathname}`);
      clearItemData();
    }
  }

  function clearItemData() {
    setSampleItem(undefined);
  }

  async function SaveItem() {
    // call ItemUpdate with the current payload contents
    // TODO implement update logic if needed
    let payload: UpdateItemPayload = {
      item1Metadata: {
      }
    };

    await callItemUpdate(sampleItem.id, payload, workloadClient);

    setDirty(false);
  }

  async function deleteCurrentItem() {
    if (sampleItem) {
      await deleteItem(sampleItem.id);
      // navigate to workspaces page after delete
      await callNavigationNavigate("host", `/groups/${sampleItem.workspaceId}`, workloadClient);
    }
  }

  async function deleteItem(itemId: string) {
    await callItemDelete(itemId, workloadClient);
  }

  function getItemObjectId() {
    const params = useParams<ContextProps>();
    return sampleItem?.id || params.itemObjectId;
  }

  // HTML page contents
  return (
    <Stack className="editor" data-testid="sample-workload-editor-inner">
      <Ribbon
        {...props}
        //  disable save when in Frontend-only
        isSaveButtonEnabled={
          sampleItem?.id !== undefined &&
          isDirty
        }
        saveItemCallback={SaveItem}
        isDeleteEnabled={sampleItem?.id !== undefined}
        deleteItemCallback={deleteCurrentItem}
        itemObjectId={getItemObjectId()}
        onTabChange={setSelectedTab}
        isDirty={isDirty}
      />

      <Stack className="main">
        {["home"].includes(selectedTab as string) && (
          <span>
            <img src="../../internalAssets/intuigence-full-logo.png" alt="Intuigence Logo" style={{ maxWidth: '300px', marginBottom: '20px' }} />
            {/* Crud item API usage example */}
            {itemEditorErrorMessage && (
              <MessageBar intent="error">
                <MessageBarBody className="message-bar-body">
                  <MessageBarTitle>
                    You cannot edit this item.
                  </MessageBarTitle>
                  {itemEditorErrorMessage}
                  <MessageBarActions>
                    <Button onClick={() => deleteItem(pageContext.itemObjectId)}>
                      Delete Item
                    </Button>
                  </MessageBarActions>
                </MessageBarBody>
              </MessageBar>
            )}
            {!itemEditorErrorMessage && (
              <div>
                <div style={{ display: 'flex', gap: '20px', marginBottom: '40px', maxWidth: '1200px' }}>
                  <div style={{ 
                    width: '350px', 
                    border: '1px solid #e0e0e0', 
                    borderRadius: '8px', 
                    padding: '24px',
                    backgroundColor: '#fafafa',
                    boxShadow: '0 2px 4px rgba(0,0,0,0.05)'
                  }}>
                    <h3 style={{ marginTop: 0, marginBottom: '20px', fontSize: '18px' }}>Find & understand information</h3>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Onboard</Label>
                      <Label style={{ display: 'block', color: '#666' }}>into new projects, assets, or systems</Label>
                    </div>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Explore</Label>
                      <Label style={{ display: 'block', color: '#666' }}>documents, graphs, and historical insights</Label>
                    </div>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Search</Label>
                      <Label style={{ display: 'block', color: '#666' }}>across data, drawings, procedures, and people</Label>
                    </div>
                  </div>
                  <div style={{ 
                    width: '350px', 
                    border: '1px solid #e0e0e0', 
                    borderRadius: '8px', 
                    padding: '24px',
                    backgroundColor: '#fafafa',
                    boxShadow: '0 2px 4px rgba(0,0,0,0.05)'
                  }}>
                    <h3 style={{ marginTop: 0, marginBottom: '20px', fontSize: '18px' }}>Automate your work</h3>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Orchestrate</Label>
                      <Label style={{ display: 'block', color: '#666' }}>industrial workflow processes</Label>
                    </div>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Automate</Label>
                      <Label style={{ display: 'block', color: '#666' }}>repetitive tasks and workflows</Label>
                    </div>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Respond</Label>
                      <Label style={{ display: 'block', color: '#666' }}>to requests, and changes—automatically</Label>
                    </div>
                  </div>
                  <div style={{ 
                    width: '350px', 
                    border: '1px solid #e0e0e0', 
                    borderRadius: '8px', 
                    padding: '24px',
                    backgroundColor: '#fafafa',
                    boxShadow: '0 2px 4px rgba(0,0,0,0.05)'
                  }}>
                    <h3 style={{ marginTop: 0, marginBottom: '20px', fontSize: '18px' }}>Create documents and content</h3>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Create</Label>
                      <Label style={{ display: 'block', color: '#666' }}>SOPs, reports, updates, and deliverables</Label>
                    </div>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Summarize</Label>
                      <Label style={{ display: 'block', color: '#666' }}>docs, threads, and engineering meetings</Label>
                    </div>
                    <div style={{ marginBottom: '16px' }}>
                      <Label style={{ display: 'block', fontWeight: 600 }}>Analyze</Label>
                      <Label style={{ display: 'block', color: '#666' }}>data and context to surface insights</Label>
                    </div>
                  </div>
                </div>
                <Divider alignContent="start" className="divider">
                  <b>Connection Details</b>
                </Divider>
                <div className="section" data-testid='item-metadata' >
                  {
                    sampleItem && (
                      <Label><b>KQL query url:</b> {kqlDatabaseQueryUrl}</Label>
                    )}
                  {
                    sampleItem && (
                      <Label><b>KQL ingestion url:</b> {kqlDatabaseIngestionUrl}</Label>
                    )}
                </div>
              </div>
            )}
          </span>
        )}
        {
          selectedTab == "eventStream" && (
            <span>
              <div className="section">
                <EventstreamComponent workloadClient={workloadClient} workspaceObjectId={sampleItem?.workspaceId} eventstreamDisplayName={eventstreamDisplayName} eventstreamItemId={eventstreamItemId} />
              </div>
            </span>
          )
        }
        {
          selectedTab == "kqlIngestor" && (
            <span>
              <div className="section">
                <KqlIngestorComponent workloadClient={workloadClient} kqlDatabaseDisplayName={kqlDatabaseDisplayName} kqlDatabaseItemId={kqlDatabaseItemId} kqlDatabaseIngestionUrl={kqlDatabaseIngestionUrl} />
              </div>
            </span>
          )
        }
        {
          selectedTab == "kqlExplorer" && (
            <span>
              <div className="section">
                <KqlExplorerComponent workloadClient={workloadClient} kqlDatabaseDisplayName={kqlDatabaseDisplayName} kqlDatabaseItemId={kqlDatabaseItemId} kqlDatabaseQueryUrl={kqlDatabaseQueryUrl} />
              </div>
            </span>
          )
        }
        {
          selectedTab == "activator" && (
            <span>
              <div className="section">
                <ActivatorComponent />
              </div>
            </span>
          )
        }
      </Stack>
    </Stack>
  );
}

// A sample Page for showcasing workloadClient.navigation Navigate/OnNavigate/OnBeforeNavigateAway/OnAfterNavigateAway amd page.Open
export function SamplePage({ workloadClient }: PageProps) {
  const pageContext = useParams<ContextProps>();
  const itemObjectId = pageContext.itemObjectId;
  return (
    <Stack className="editor">
      <Stack className="main">
        <Button
          onClick={() =>
            callNavigationNavigate("workload", "/sample-workload-editor/" + itemObjectId, workloadClient)
          }
        >
          Navigate Back
        </Button>
      </Stack>
    </Stack>
  );
}
