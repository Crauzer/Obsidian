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
import { ExtractAllButton } from './ExtractAllButton';
import { WadBreadcrumbs } from './WadBreadcrumbs';
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
        <div className="flex w-full flex-row gap-2 border-b border-gray-600 bg-gray-800 p-2">
          <ExtractAllButton wadId={wadId} />
          <WadBreadcrumbs wadId={wadId} pathComponents={pathComponents} />
        </div>
        <WadItemList wadId={wadId} parentItemId={parentItemId} data={items} />
      </div>
    </div>
  );
};
