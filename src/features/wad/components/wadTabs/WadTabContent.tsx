import clsx from 'clsx';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';

import { ArchiveIcon } from '../../../../assets';
import { Breadcrumbs, Icon, Input, Tooltip } from '../../../../components';
import { appRoutes } from '../../../../lib/router';
import { composeUrlQuery } from '../../../../utils';
import { useWadDirectoryPathComponents, useWadParentItems } from '../../api';
import { WadItem, WadItemPathComponent } from '../../types';
import { WadItemList } from '../wadItemList';
import { WadTabToolbar } from './toolbar';

export type WadRootTabContentProps = { wadId: string };

export const WadRootTabContent: React.FC<WadRootTabContentProps> = ({ wadId }) => {
  const itemsQuery = useWadParentItems({ wadId, parentId: undefined });

  if (itemsQuery.isSuccess) {
    return (
      <WadTabContent
        wadId={wadId}
        parentItemId={undefined}
        items={itemsQuery.data}
        pathComponents={[]}
      />
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
  const pathComponentsQuery = useWadDirectoryPathComponents({ wadId, itemId: selectedItemId });
  const itemsQuery = useWadParentItems({ wadId, parentId: selectedItemId });

  console.info(selectedItemId);
  if (itemsQuery.isSuccess) {
    return (
      <WadTabContent
        wadId={wadId}
        parentItemId={selectedItemId}
        items={itemsQuery.data}
        pathComponents={pathComponentsQuery.data ?? []}
      />
    );
  }

  return null;
};

type WadTabContentProps = {
  wadId: string;
  parentItemId?: string;
  items: WadItem[];
  pathComponents: WadItemPathComponent[];
};

const WadTabContent: React.FC<WadTabContentProps> = ({
  wadId,
  parentItemId,
  items,
  pathComponents,
}) => {
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
            name={<Icon size="md" className="fill-obsidian-500" icon={ArchiveIcon} />}
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
        <WadItemList wadId={wadId} parentItemId={parentItemId} data={items} />
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
