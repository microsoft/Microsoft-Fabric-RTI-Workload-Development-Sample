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
import { ItemMetadataNotFound} from "../../models/WorkloadExceptionsModel";

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
  const [operand1, setOperand1] = useState<number>(0);
  const [operand2, setOperand2] = useState<number>(0);
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
          item.extendedMetdata.item1Metadata;
        setOperand1(item1Metadata?.operand1);
        setOperand2(item1Metadata?.operand2);
                
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
    let payload: UpdateItemPayload = {
      item1Metadata: {
        lakehouse: null,
        operand1: operand1,
        operand2: operand2
      },
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
            <h2>Sample Item Editor</h2>
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
                <Divider alignContent="start">
                  {sampleItem ? "" : "New "}Item Details
                </Divider>
                <div className="section" data-testid='item-editor-metadata' >
                  {sampleItem && (
                    <Label>WorkspaceId Id: {sampleItem?.workspaceId}</Label>
                  )}
                  {sampleItem && <Label>Item Id: {sampleItem?.id}</Label>}
                  {sampleItem && (
                    <Label>Item Display Name: {sampleItem?.displayName}</Label>
                  )}
                  {sampleItem && (
                    <Label>Item Description: {sampleItem?.description}</Label>
                  )}
                </div>
              </div>
            )}
          </span>
        )}
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
