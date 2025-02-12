import React from "react";
import { Tab, TabList } from '@fluentui/react-tabs';
import { Toolbar } from '@fluentui/react-toolbar';

import {
  SelectTabEvent, SelectTabData, TabValue,
  Menu, MenuItem, MenuList, MenuPopover, MenuTrigger,
  ToolbarButton, Button, MenuButton, Tooltip
} from '@fluentui/react-components';
import {
  Save24Regular,
  Chat24Regular,
  Edit24Regular,
  Share24Regular,
  Delete24Regular,
} from "@fluentui/react-icons";
import { Stack } from '@fluentui/react';

import { PageProps } from 'src/App';
import { callDialogOpenMsgBox } from '../../controller/SampleWorkloadController';
import './../../styles.scss';

const HomeTabToolbar = (props: RibbonProps) => {

  async function onSaveAsClicked() {
    // your code to save as here
    await props.saveItemCallback();
    return;
  }

  async function onDeleteClicked() {
    // don't call delete in Create mode
    if (!props.isDeleteEnabled) {
      return;
    }

    const msgBoxResult: string = await callDialogOpenMsgBox("Delete Item", "Are you sure about deleting this item?", ["Yes", "No"], props.workloadClient);
    if (msgBoxResult != "Yes") {
      return;
    }

    props.deleteItemCallback();

  }

  function getSaveButtonTooltipText(): string {
    return !props.isDeleteEnabled
      ? 'Save is not supported in Frontend-only'
      : (!props.isLakeHouseSelected
        ? 'Select a Lakehouse'
        : 'Save');
  }

  return (
    <Toolbar>

      <Tooltip
        content={getSaveButtonTooltipText()}
        relationship="label">
        <ToolbarButton
          disabled={!props.isSaveButtonEnabled}
          aria-label="Save"
          data-testid="item-editor-save-btn"
          icon={<Save24Regular />} onClick={onSaveAsClicked} />
      </Tooltip>

      <Tooltip
        content="Delete"
        relationship="label">
        <ToolbarButton
          aria-label="Delete"
          data-testid="item-editor-delete-btn"
          disabled={!props.isDeleteEnabled}

          icon={<Delete24Regular />}
          onClick={() => onDeleteClicked()} />
      </Tooltip>
    </Toolbar>
  );
};


const CollabButtons = (props: RibbonProps) => {
  return (
    <div className="collabContainer">
      <Stack horizontal>
        <Button size="small" icon={<Chat24Regular />}>Comments</Button>
        <Menu>
          <MenuTrigger disableButtonEnhancement>
            <MenuButton size="small" icon={<Edit24Regular />}>Editing</MenuButton>
          </MenuTrigger>
          <MenuPopover>
            <MenuList>
              <MenuItem>Editing</MenuItem>
              <MenuItem>Viewing</MenuItem>
            </MenuList>
          </MenuPopover>
        </Menu>
        <Button size="small" icon={<Share24Regular />} appearance="primary">Share</Button>
      </Stack>
    </div>
  );
}

export interface RibbonProps extends PageProps {
  saveItemCallback: () => Promise<void>;
  isLakeHouseSelected?: boolean;
  isSaveButtonEnabled?: boolean;
  isDeleteEnabled?: boolean;
  deleteItemCallback: () => void;
  itemObjectId?: string;
  onTabChange: (tabValue: TabValue) => void;
  isDirty: boolean;
}

export function Ribbon(props: RibbonProps) {
  const { onTabChange } = props;
  const [selectedValue, setSelectedValue] = React.useState<TabValue>('home');

  const onTabSelect = (_: SelectTabEvent, data: SelectTabData) => {
    setSelectedValue(data.value);
    onTabChange(data.value);
  };

  return (
    <div className="ribbon">
      <CollabButtons {...props} />
      <TabList defaultSelectedValue="home" onTabSelect={onTabSelect}>
        <Tab value="home" data-testid="home-tab-btn">Home</Tab>
      </TabList>

      <div className="toolbarContainer">
        {["home"].includes(selectedValue as string) && <HomeTabToolbar {...props} />}
      </div>

    </div>
  );
};
