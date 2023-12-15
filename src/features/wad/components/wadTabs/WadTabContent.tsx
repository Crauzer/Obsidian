import { tauri } from '@tauri-apps/api';
import clsx from 'clsx';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { LuFileDown, LuFileStack } from 'react-icons/lu';
import { useSearchParams } from 'react-router-dom';
import { v4 as uuidv4 } from 'uuid';

import { ArchiveIcon } from '../../../../assets';
import {
  Breadcrumbs,
  Button,
  Icon,
  Input,
  Toolbar,
  ToolbarRootProps,
  Tooltip,
} from '../../../../components';
import { appRoutes } from '../../../../lib/router';
import { composeUrlQuery } from '../../../../utils';
import { usePickDirectory } from '../../../fs';
import { useWadDirectoryItems, useWadDirectoryPathComponents, useWadItems } from '../../api';
import { wadCommands } from '../../commands';
import { WadItem, WadItemPathComponent } from '../../types';
import { WadItemList } from './WadItemList';

export type WadRootTabContentProps = { wadId: string };

export const WadRootTabContent: React.FC<WadRootTabContentProps> = ({ wadId }) => {
  const itemsQuery = useWadItems(wadId);

  if (itemsQuery.isSuccess) {
    return <WadTabContent wadId={wadId} items={itemsQuery.data} pathComponents={[]} />;
  }

  return null;
};

export type WadDirectoryTabContentProps = {
  wadId: string;
  selectedItemId: string;
};

export const WadDirectoryTabContent: React.FC<WadDirectoryTabContentProps> = ({
  wadId,
  selectedItemId,
}) => {
  const pathComponentsQuery = useWadDirectoryPathComponents({ wadId, itemId: selectedItemId });
  const itemsQuery = useWadDirectoryItems(wadId, selectedItemId);

  if (itemsQuery.isSuccess) {
    return (
      <WadTabContent
        wadId={wadId}
        items={itemsQuery.data}
        pathComponents={pathComponentsQuery.data ?? []}
      />
    );
  }

  return null;
};

type WadTabContentProps = {
  wadId: string;
  items: WadItem[];
  pathComponents: WadItemPathComponent[];
};

const WadTabContent: React.FC<WadTabContentProps> = ({ wadId, items, pathComponents }) => {
  const [t] = useTranslation('mountedWads');

  return (
    <div className="flex h-full flex-col gap-2">
      <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-900">
        <div className="flex flex-row border-b border-gray-600 bg-gray-800">
          <WadTabToolbar className="w-1/2" wadId={wadId} />
          <Input className="m-1 w-1/2 flex-1" />
        </div>
        <Breadcrumbs.Root className="border-b border-gray-600 bg-gray-800 p-1 font-fira-mono text-sm leading-6">
          <PathBreadcrumbItem
            itemId=""
            name={<Icon size="lg" className="fill-obsidian-500" icon={ArchiveIcon} />}
            path={t('path.root')}
            href={composeUrlQuery(appRoutes.mountedWads, { wadId })}
          />
          {pathComponents.map(({ itemId, name, path }, index) => (
            <PathBreadcrumbItem
              key={index}
              itemId={itemId}
              name={name}
              path={path}
              href={composeUrlQuery(appRoutes.mountedWads, { wadId, itemId })}
            />
          ))}
        </Breadcrumbs.Root>
        <WadItemList wadId={wadId} data={items} />
      </div>
    </div>
  );
};

type PathBreadcrumbItemProps = {
  itemId: string;
  name: React.ReactNode;
  path: React.ReactNode;
  href: string;
};

const PathBreadcrumbItem: React.FC<PathBreadcrumbItemProps> = ({ itemId, name, path, href }) => {
  const [searchParams] = useSearchParams();

  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>
        <Breadcrumbs.Item
          className={clsx('font-mono', {
            'font-bold text-obsidian-400': searchParams.get('itemId') === itemId,
          })}
          href={href}
        >
          {name}
        </Breadcrumbs.Item>
      </Tooltip.Trigger>
      <Tooltip.Content side="bottom">{path}</Tooltip.Content>
    </Tooltip.Root>
  );
};

type WadTabToolbarProps = { wadId: string } & ToolbarRootProps;

const WadTabToolbar: React.FC<WadTabToolbarProps> = ({ wadId, ...props }) => {
  const [extractAllActionId] = useState(uuidv4());

  const pickDirectory = usePickDirectory();

  return (
    <Toolbar.Root {...props}>
      <Toolbar.Button asChild>
        <Button
          compact
          variant="ghost"
          onClick={() => {
            pickDirectory.mutate(undefined, {
              onSuccess: (directory) => {
                tauri.invoke(wadCommands.extractMountedWad, {
                  wadId,
                  actionId: extractAllActionId,
                  extractDirectory: directory.path,
                });
              },
            });
          }}
        >
          <Icon size="md" icon={LuFileDown} />
        </Button>
      </Toolbar.Button>
      <Toolbar.Button asChild>
        <Button compact variant="ghost">
          <Icon size="md" icon={LuFileStack} />
        </Button>
      </Toolbar.Button>
    </Toolbar.Root>
  );
};
