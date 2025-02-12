import React, { useState, useEffect } from "react";
import { useLocation, useParams } from "react-router-dom";
import { Stack } from "@fluentui/react";
import {
  Button,
  Combobox,
  Divider,
  Field,
  Input,
  Label,
  Option,
  TabValue,
  MessageBar,
  MessageBarBody,
  MessageBarTitle,
  MessageBarActions
} from "@fluentui/react-components";

import { useTranslation } from "react-i18next";
import { initializeIcons } from "@fluentui/font-icons-mdl2";
import {
  PanelRightExpand20Regular,
  Database16Regular,
  TriangleRight20Regular,
} from "@fluentui/react-icons";
import { AfterNavigateAwayData } from "@ms-fabric/workload-client";
import { ContextProps, PageProps } from "src/App";
import {
  callNavigationNavigate,
  callNavigationBeforeNavigateAway,
  callNavigationAfterNavigateAway,
  callThemeOnChange,
  callLanguageGet,
  callSettingsOnChange,
  callDatahubOpen,
  callItemGet,
  callItemUpdate,
  callItemDelete,
  callGetItem1SupportedOperators,
  callItem1DoubleResult
} from "../../controller/SampleWorkloadController";
import { Ribbon } from "../SampleWorkloadRibbon/SampleWorkloadRibbon";
import { convertGetItemResultToWorkloadItem } from "../../utils";
import {
  Item1ClientMetadata,
  GenericItem,
  ItemPayload,
  UpdateItemPayload,
  WorkloadItem,
} from "../../models/SampleWorkloadModel";
import "./../../styles.scss";
import { ItemMetadataNotFound} from "../../models/WorkloadExceptionsModel";

export function SampleWorkloadEditor(props: PageProps) {
  const sampleWorkloadBEUrl = process.env.WORKLOAD_BE_URL;
  const { workloadClient } = props;
  const pageContext = useParams<ContextProps>();
  const { pathname } = useLocation();
  const { i18n } = useTranslation();

  // initializing usage of FluentUI icons
  initializeIcons();

  // React state for WorkloadClient APIs
  const [operand1ValidationMessage, setOperand1ValidationMessage] =
    useState<string>("");
  const [operand2ValidationMessage, setOperand2ValidationMessage] =
    useState<string>("");
  const [selectedLakehouse, setSelectedLakehouse] =
    useState<GenericItem>(undefined);
  const [sampleItem, setSampleItem] =
    useState<WorkloadItem<ItemPayload>>(undefined);
  const [operand1, setOperand1] = useState<number>(0);
  const [operand2, setOperand2] = useState<number>(0);
  const [operator, setOperator] = useState<string | null>(null);
  const [isDirty, setDirty] = useState<boolean>(false);
  const [supportedOperators, setSupportedOperators] = useState<string[]>([]);
  const [hasLoadedSupportedOperators, setHasLoadedSupportedOperators] = useState(false);
  
  const [, setLang] = useState<string>('en-US');
  const [itemEditorErrorMessage, setItemEditorErrorMessage] = useState<string>("");
  document.body.dir = i18n.dir();

  const INT32_MIN = -2147483648;
  const INT32_MAX = 2147483647;


  const [selectedTab, setSelectedTab] = useState<TabValue>("home");

  useEffect(() => {
    callLanguageGet(workloadClient).then((lang) => setLang(lang));

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

  
  // Effect to load supported operators once on component mount
  useEffect(() => {
    loadSupportedOperators();
  }, []); 

  useEffect(() => {
    if (hasLoadedSupportedOperators) {
      loadDataFromUrl(pageContext, pathname);
    }
  }, [hasLoadedSupportedOperators, pageContext, pathname]);


  async function loadSupportedOperators(): Promise<void> {
    try {
      const operators = await callGetItem1SupportedOperators(sampleWorkloadBEUrl, workloadClient);
      setSupportedOperators(operators);
      setHasLoadedSupportedOperators(true);
    } catch (error) {
      console.error(`Error loading supported operators: ${error}`);
      setHasLoadedSupportedOperators(false);
    }
  }

  async function afterNavigateCallBack(_event: AfterNavigateAwayData): Promise<void> {
    //clears the data after navigation
    setSelectedLakehouse(undefined);
    setSampleItem(undefined);
    return;
  }

  // callback functions called by UI controls below







  async function onCallNavigate(path: string) {
    await callNavigationNavigate("workload", path, workloadClient);
  }


  async function onCallDatahubLakehouse() {
    const result = await callDatahubOpen(
      ["Lakehouse"],
      "Select a Lakehouse to use for Sample Workload",
      false,
      workloadClient
    );
    if (result) {
      setSelectedLakehouse(result);
      setDirty(true);
    }
  }

  async function onOperand1InputChanged(value: number) {
    setOperand1ValidationMessage("");
    setOperand1(value);
    setDirty(true);
  }

  async function onOperand2InputChanged(value: number) {
    setOperand2ValidationMessage("");
    setOperand2(value);
    setDirty(true);
  }

  function onOperatorInputChanged(value: string | null) {
    setOperator(value);
    setDirty(true);
  }

  function validateOperandsBeforeDouble() {
    var valid = true;
    if (operand1 < INT32_MIN/2 || operand1 > INT32_MAX/2) {
      setOperand1ValidationMessage("Operand 1 may lead to overflow if doubled");
      valid = false;
    }
    if (operand2 < INT32_MIN/2 || operand2 > INT32_MAX/2) {
      setOperand2ValidationMessage("Operand 2 may lead to overflow if doubled");
      valid = false;
    }
    return valid;
  }

  async function onDoubleButtonClick() {
    if (sampleItem && validateOperandsBeforeDouble()) {
      const result = await callItem1DoubleResult(
        sampleWorkloadBEUrl,
        workloadClient,
        sampleItem.workspaceId,
        sampleItem.id
      );
      if (result) {
        // Update both operands
        setOperand1(result.Operand1);
        setOperand2(result.Operand2);
      }
    }
  }

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
        setSelectedLakehouse(item1Metadata?.lakehouse);
        setOperand1(item1Metadata?.operand1);
        setOperand2(item1Metadata?.operand2);
        
        const loadedOperator = item1Metadata?.operator;
        const isValidOperator = loadedOperator && supportedOperators.includes(loadedOperator);
        setOperator(isValidOperator ? loadedOperator : null);
        
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
        lakehouse: selectedLakehouse,
        operand1: operand1,
        operand2: operand2,
        operator: operator,
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

  function isDisabledDoubleResultButton(): boolean {
    return isDirty || operator == "0" || sampleItem == undefined;
  }

  // HTML page contents
  return (
    <Stack className="editor" data-testid="sample-workload-editor-inner">
      <Ribbon
        {...props}
        isLakeHouseSelected={selectedLakehouse != undefined}
        //  disable save when in Frontend-only
        isSaveButtonEnabled={
          sampleItem?.id !== undefined &&
          selectedLakehouse != undefined &&
          isDirty &&
          !!operator
        }
        saveItemCallback={SaveItem}
        isDeleteEnabled={sampleItem?.id !== undefined}
        deleteItemCallback={deleteCurrentItem}
        itemObjectId={getItemObjectId()}
        onTabChange={setSelectedTab}
        isDirty={isDirty}
      />

      <Stack className="main">
        {["jobs", "home"].includes(selectedTab as string) && (
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
                <Divider alignContent="start">Selected Lakehouse Details</Divider>
                <div className="section">
                  <Stack horizontal>
                    <Field
                      label="Lakehouse"
                      orientation="horizontal"
                      className="field"
                    >
                      <Input
                        size="small"
                        placeholder="Lakehouse Name"
                        style={{ marginLeft: "10px" }}
                        value={
                          selectedLakehouse ? selectedLakehouse.displayName : ""
                        }
                      />
                    </Field>
                    <Button
                      style={{ width: "24px", height: "24px" }}
                      icon={<Database16Regular />}
                      appearance="primary"
                      onClick={() => onCallDatahubLakehouse()}
                      data-testid="item-editor-lakehouse-btn"
                    />
                  </Stack>
                  <Field
                    label="Lakehouse ID"
                    orientation="horizontal"
                    className="field"
                  >
                    <Input
                      size="small"
                      placeholder="Lakehouse ID"
                      value={selectedLakehouse ? selectedLakehouse.id : ""}
                      data-testid="lakehouse-id-input"
                    />
                  </Field>
                </div>
                <Divider alignContent="start">Calculation definition</Divider>
                <div className="section">
                  <Field
                    label="Operand 1"
                    validationMessage={operand1ValidationMessage}
                    orientation="horizontal"
                    className="field"
                  >
                    <Input
                      size="small"
                      type="number"
                      placeholder="Value of the 1st operand"
                      value={operand1.toString()}
                      onChange={(e) =>
                        onOperand1InputChanged(parseInt(e.target.value))
                      }
                      data-testid="operand1-input"
                    />
                  </Field>
                  <Field
                    label="Operand 2"
                    validationMessage={operand2ValidationMessage}
                    orientation="horizontal"
                    className="field"
                  >
                    <Input
                      size="small"
                      type="number"
                      placeholder="value of the 2nd operand"
                      value={operand2.toString()}
                      onChange={(e) =>
                        onOperand2InputChanged(parseInt(e.target.value))
                      }
                      data-testid="operand2-input"
                    />
                  </Field>
                  <Field
                    label="Operator"
                    orientation="horizontal"
                    className="field"
                  >
                    <Combobox
                      key={pageContext.itemObjectId}
                      data-testid="operator-combobox"
                      placeholder="Operator"
                      value={operator ?? ''}
                      onOptionSelect={(_, opt) =>
                        onOperatorInputChanged(opt.optionValue)
                      }
                    >
                      {supportedOperators.map((option) => (
                        <Option key={option} data-testid={option} value={option}>{option}</Option>
                      ))}
                    </Combobox>
                  </Field>
                  <Button
                    appearance="primary"
                    icon={<TriangleRight20Regular />}
                    disabled={isDisabledDoubleResultButton()}
                    onClick={() => onDoubleButtonClick()}
                  >
                    Double the operands
                  </Button>
                </div>
                <Divider alignContent="start">Authentication</Divider>
                <div className="section">
                  <Button
                    appearance="primary"
                    icon={<PanelRightExpand20Regular />}
                    onClick={() => onCallNavigate(`/Authentication/${sampleItem.id}`)}
                  >
                    Navigate to Authentication Page
                  </Button>
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
