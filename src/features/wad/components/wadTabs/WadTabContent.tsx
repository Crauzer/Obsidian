import clsx from 'clsx';
import { useMatch, useSearchParams } from 'react-router-dom';

import { CaretRightIcon } from '../../../../assets';
import { Breadcrumbs, Icon } from '../../../../components';
import { appRoutes } from '../../../../lib/router';
import { composeUrlQuery } from '../../../../utils';
import { useWadDirectoryItems, useWadDirectoryPathComponents, useWadItems } from '../../api';
import { WadItemList } from './WadItemList';

export type WadTabContentProps = {
  wadId: string;
};

export const WadTabContent: React.FC<WadTabContentProps> = ({ wadId }) => {
  const wadItemsQuery = useWadItems(wadId);

  if (wadItemsQuery.isSuccess) {
    return (
      <div className="flex flex-col gap-2 p-2">
        <div className="rounded border border-gray-600 bg-gray-800">
          <div className="border-b border-gray-600 p-1">
            <Breadcrumbs.Root>
              <PathBreadcrumbItem wadId={wadId} itemId="" name="" />
            </Breadcrumbs.Root>
          </div>
          <div className="p-1">
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
  const pathComponentsQuery = useWadDirectoryPathComponents({ wadId, itemId: selectedItemId });
  const itemsQuery = useWadDirectoryItems(wadId, selectedItemId);

  if (itemsQuery.isSuccess) {
    return (
      <div className="flex flex-col gap-2 p-2">
        <div className="rounded border border-gray-600 bg-gray-800">
          <div className="border-b border-gray-600 p-1">
            {pathComponentsQuery.isSuccess && (
              <Breadcrumbs.Root>
                {pathComponentsQuery.data.map(({ itemId, name }, index) => (
                  <PathBreadcrumbItem key={index} wadId={wadId} itemId={itemId} name={name} />
                ))}
              </Breadcrumbs.Root>
            )}
          </div>
          <div className="p-1">
            <WadItemList wadId={wadId} data={itemsQuery.data} />
          </div>
        </div>
      </div>
    );
  }

  return null;
};

type PathBreadcrumbItemProps = {
  wadId: string;
  itemId: string;
  name: string;
};

const PathBreadcrumbItem: React.FC<PathBreadcrumbItemProps> = ({ wadId, itemId, name }) => {
  const [searchParams] = useSearchParams();

  return (
    <Breadcrumbs.Item
      className={clsx({ 'font-bold text-obsidian-400': searchParams.get('itemId') === itemId })}
      title={name}
      href={composeUrlQuery(appRoutes.mountedWads, { wadId, itemId })}
    />
  );
};
