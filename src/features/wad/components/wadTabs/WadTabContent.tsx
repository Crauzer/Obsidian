import clsx from 'clsx';
import { t } from 'i18next';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';

import { ArchiveIcon } from '../../../../assets';
import { Breadcrumbs, Icon, Input, Tooltip } from '../../../../components';
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
        <Input />
        <div className="flex h-full flex-col rounded border border-gray-600 bg-gray-900">
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
