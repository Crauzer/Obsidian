import clsx from 'clsx';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { LuFileDown, LuFileStack } from 'react-icons/lu';
import { useSearchParams } from 'react-router-dom';

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
import { useWadDirectoryItems, useWadDirectoryPathComponents, useWadItems } from '../../api';
import { WadItemList } from './WadItemList';

export type WadTabContentProps = {
  wadId: string;
};

export const WadTabContent: React.FC<WadTabContentProps> = ({ wadId }) => {
  const [t] = useTranslation('mountedWads');

  const wadItemsQuery = useWadItems(wadId);

  if (wadItemsQuery.isSuccess) {
    return (
      <div className="flex h-full flex-col gap-2">
        <Input />
        <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-900">
          <Breadcrumbs.Root className="border-b border-gray-600 bg-gray-800 p-1 font-fira-mono text-sm leading-6">
            <PathBreadcrumbItem
              itemId=""
              name={<Icon size="lg" className="fill-obsidian-500" icon={ArchiveIcon} />}
              path={t('path.root')}
              href={composeUrlQuery(appRoutes.mountedWads, { wadId })}
            />
          </Breadcrumbs.Root>
          <div className="flex h-full">
            <WadItemList wadId={wadId} data={wadItemsQuery.data} />
          </div>
        </div>
      </div>
    );
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
  const [t] = useTranslation('mountedWads');

  const pathComponentsQuery = useWadDirectoryPathComponents({ wadId, itemId: selectedItemId });
  const itemsQuery = useWadDirectoryItems(wadId, selectedItemId);

  if (itemsQuery.isSuccess) {
    return (
      <div className="flex h-full flex-col gap-2">
        <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-900">
          <div className="flex flex-row rounded border border-gray-600 bg-gray-800">
            <WadTabToolbar className="w-1/2 rounded" />
            <Input className="m-1 w-1/2 flex-1" />
          </div>
          {pathComponentsQuery.isSuccess && (
            <Breadcrumbs.Root className="border-b border-gray-600 bg-gray-800 p-1 font-fira-mono text-sm leading-6">
              <PathBreadcrumbItem
                itemId=""
                name={<Icon size="lg" className="fill-obsidian-500" icon={ArchiveIcon} />}
                path={t('path.root')}
                href={composeUrlQuery(appRoutes.mountedWads, { wadId })}
              />
              {pathComponentsQuery.data.map(({ itemId, name, path }, index) => (
                <PathBreadcrumbItem
                  key={index}
                  itemId={itemId}
                  name={name}
                  path={path}
                  href={composeUrlQuery(appRoutes.mountedWads, { wadId, itemId })}
                />
              ))}
            </Breadcrumbs.Root>
          )}
          <WadItemList wadId={wadId} data={itemsQuery.data} />
        </div>
      </div>
    );
  }

  return null;
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

type WadTabToolbarProps = ToolbarRootProps;

const WadTabToolbar: React.FC<WadTabToolbarProps> = (props) => {
  return (
    <Toolbar.Root {...props}>
      <Toolbar.Button asChild>
        <Button compact variant="ghost">
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
