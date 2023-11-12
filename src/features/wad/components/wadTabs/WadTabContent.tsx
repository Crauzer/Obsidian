import clsx from 'clsx';
import { useSearchParams } from 'react-router-dom';

import { ArchiveIcon } from '../../../../assets';
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
      <div className="flex h-full flex-col gap-2 p-2">
        <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-800">
          <Breadcrumbs.Root className="font-fira-mono border-b border-gray-600 p-1 text-sm leading-6">
            <PathBreadcrumbItem
              itemId=""
              name={<Icon size="lg" className="fill-obsidian-500" icon={ArchiveIcon} />}
              href={composeUrlQuery(appRoutes.mountedWads, { wadId })}
            />
          </Breadcrumbs.Root>
          <WadItemList wadId={wadId} data={wadItemsQuery.data} />
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
      <div className="flex h-full flex-col gap-2 p-2">
        <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-800">
          {pathComponentsQuery.isSuccess && (
            <Breadcrumbs.Root className="font-fira-mono border-b border-gray-600 p-1 text-sm leading-6">
              <PathBreadcrumbItem
                itemId=""
                name={<Icon size="lg" className="fill-obsidian-500" icon={ArchiveIcon} />}
                href={composeUrlQuery(appRoutes.mountedWads, { wadId })}
              />
              {pathComponentsQuery.data.map(({ itemId, name }, index) => (
                <PathBreadcrumbItem
                  key={index}
                  itemId={itemId}
                  name={name}
                  href={composeUrlQuery(appRoutes.mountedWads, { wadId, itemId })}
                />
              ))}
            </Breadcrumbs.Root>
          )}
          <div className="flex h-full">
            <WadItemList wadId={wadId} data={itemsQuery.data} />
          </div>
        </div>
      </div>
    );
  }

  return null;
};

type PathBreadcrumbItemProps = {
  itemId: string;
  name: React.ReactNode;
  href: string;
};

const PathBreadcrumbItem: React.FC<PathBreadcrumbItemProps> = ({ itemId, name, href }) => {
  const [searchParams] = useSearchParams();

  return (
    <Breadcrumbs.Item
      className={clsx('font-mono', {
        'font-bold text-obsidian-400': searchParams.get('itemId') === itemId,
      })}
      title={name}
      href={href}
    />
  );
};
