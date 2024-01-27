import clsx from 'clsx';
import React, { MouseEventHandler, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';

import { FolderIcon } from '../../../../assets';
import { Icon } from '../../../../components';
import { WadItem } from '../../types';
import { getLeagueFileKindIcon, getLeagueFileKindIconColor } from '../../utils';
import { WadItemRowContextMenu } from './contextMenu';

export type WadItemListRowProps = {
  wadId: string;
  parentItemId?: string;
  item: WadItem;
  index: number;
  onClick?: MouseEventHandler;
};

export const WadItemListRow: React.FC<WadItemListRowProps> = ({
  wadId,
  parentItemId,
  item,
  onClick,
}) => {
  const [, setSearchParams] = useSearchParams();

  const handleDoubleClick = useCallback(() => {
    if (item.kind !== 'directory') {
      return;
    }

    // TODO: setting url query here isn't a good idea
    setSearchParams((params) => {
      params.set('itemId', item.id);
      return params;
    });
  }, [item.id, item.kind, setSearchParams]);

  return (
    <WadItemRowContextMenu wadId={wadId} parentItemId={parentItemId} item={item}>
      <div
        className={clsx(
          'text-md box-border flex select-none flex-row border py-1 pl-2 text-gray-50 hover:cursor-pointer',
          { 'hover:bg-gray-700/25': !item.isSelected },
          {
            'border-obsidian-500/40 bg-obsidian-700/40': item.isSelected,
            'border-transparent': !item.isSelected,
          },
        )}
        onClick={(e) => onClick?.(e)}
        onDoubleClick={handleDoubleClick}
        onContextMenu={() => {}}
      >
        <div className="flex flex-row items-center gap-2">
          {item.kind === 'directory' ? (
            <Icon size="md" className="fill-amber-500" icon={FolderIcon} />
          ) : (
            <Icon
              size="md"
              className={clsx(getLeagueFileKindIconColor(item.extensionKind))}
              icon={getLeagueFileKindIcon(item.extensionKind)}
            />
          )}
          {item.name}
        </div>
      </div>
    </WadItemRowContextMenu>
  );
};
