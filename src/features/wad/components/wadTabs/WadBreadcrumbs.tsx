import React, { useCallback, useMemo } from 'react';
import { FaChevronRight } from 'react-icons/fa';
import { FcFolder } from 'react-icons/fc';
import { MdDataArray } from 'react-icons/md';
import { useNavigate } from 'react-router-dom';
import { Virtuoso } from 'react-virtuoso';

import { useWadParentItems } from '../..';
import { ArchiveIcon } from '../../../../assets';
import { ActionIcon, Button, Icon, Menu, Spinner } from '../../../../components';
import { appRoutes } from '../../../../lib/router';
import { composeUrlQuery } from '../../../../utils';
import { WadItem, WadItemPathComponent } from '../../types';

export type WadBreadcrumbsProps = {
  wadId: string;
  pathComponents: WadItemPathComponent[];
};

export const WadBreadcrumbs: React.FC<WadBreadcrumbsProps> = ({ wadId, pathComponents }) => {
  return (
    <div className="flex w-full flex-row rounded border border-gray-600 bg-gray-900 p-1 shadow-inner">
      <WadBreadcrumb
        wadId={wadId}
        name={<Icon size="md" className="fill-obsidian-500" icon={ArchiveIcon} />}
      />
      {pathComponents.map((pathComponent, index) => {
        return (
          <WadBreadcrumb
            key={index}
            wadId={wadId}
            name={pathComponent.name}
            itemId={pathComponent.itemId}
          />
        );
      })}
    </div>
  );
};

type WadBreadcrumbProps = {
  wadId: string;
  name: React.ReactNode;
  itemId?: string;
};

const WadBreadcrumb: React.FC<WadBreadcrumbProps> = ({ wadId, name, itemId }) => {
  return (
    <div className="flex flex-row items-center rounded transition-colors hover:bg-gray-600/40">
      <Button
        compact
        as="a"
        variant="ghost"
        className="rounded-none rounded-l px-[6px] py-1"
        href={composeUrlQuery(appRoutes.mountedWads, { wadId, itemId: itemId })}
      >
        {name}
      </Button>
      <WadBreadcrumbChildrenMenu wadId={wadId} parentId={itemId} />
    </div>
  );
};

type WadBreadcrumbChildrenMenuProps = {
  wadId: string;
  parentId?: string;

  children?: React.ReactNode;
};

export const WadBreadcrumbChildrenMenu: React.FC<WadBreadcrumbChildrenMenuProps> = ({
  wadId,
  parentId,

  children,
}) => {
  const items = useWadParentItems({ wadId, parentId });

  const childrenDirectories = useMemo(() => {
    if (!items.isSuccess) {
      return undefined;
    }

    return items.data.filter((item) => item.kind === 'directory');
  }, [items.data, items.isSuccess]);

  return (
    <Menu.Root>
      <Menu.Trigger asChild>
        <ActionIcon
          size="xs"
          variant="ghost"
          className="h-full rounded-none rounded-r p-0 px-1 hover:bg-gray-600"
          icon={FaChevronRight}
        />
      </Menu.Trigger>
      <Menu.Content className="h-[350px]" side="bottom">
        {items.isFetching && <Spinner />}
        {childrenDirectories && childrenDirectories.length > 0 ? (
          <Virtuoso
            style={{ height: '100%' }}
            data={childrenDirectories}
            itemContent={(index, item) => (
              <WadBreadcrumbChildrenMenuItem wadId={wadId} item={item} />
            )}
          />
        ) : (
          <div className="flex h-full items-center justify-center">
            <div className="flex flex-col items-center gap-1">
              <Icon size="xl" icon={MdDataArray} />
              <p className="text-base text-gray-50">No children directories</p>
            </div>
          </div>
        )}
      </Menu.Content>
    </Menu.Root>
  );
};

export type WadBreadcrumbChildrenMenuItemProps = {
  wadId: string;
  item: WadItem;
};

export const WadBreadcrumbChildrenMenuItem: React.FC<WadBreadcrumbChildrenMenuItemProps> = ({
  wadId,
  item,
}) => {
  const navigate = useNavigate();

  const handleClick = () => {
    console.info(composeUrlQuery(appRoutes.mountedWads, { wadId, itemId: item.id }));
    navigate(composeUrlQuery(appRoutes.mountedWads, { wadId, itemId: item.id }));
  };

  return (
    <Menu.Item className="flex flex-row gap-2" onClick={handleClick}>
      <Icon size="md" className="fill-obsidian-500" icon={FcFolder} />
      <p className="text-base text-gray-50">{item.name}</p>
    </Menu.Item>
  );
};
