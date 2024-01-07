import clsx from 'clsx';
import React, { useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';

import { FolderIcon } from '../../../../assets';
import { Icon } from '../../../../components';
import { WadItem } from '../../types';
import { getLeagueFileKindIcon, getLeagueFileKindIconColor } from '../../utils';

export type WadItemListRowProps = {
  wadId: string;
  item: WadItem;
  index: number;
  onClick?: () => void;
};

export const WadItemListRow: React.FC<WadItemListRowProps> = ({ item, onClick }) => {
  const [_, setSearchParams] = useSearchParams();

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
    <div
      className={clsx(
        'text-md box-border flex select-none flex-row border py-1 pl-2 text-gray-50 hover:cursor-pointer',
        { 'hover:bg-gray-700/25': !item.isSelected },
        {
          'border-obsidian-500/40 bg-obsidian-700/40': item.isSelected,
          'border-transparent': !item.isSelected,
        },
      )}
      onClick={() => onClick?.()}
      onDoubleClick={handleDoubleClick}
    >
      <div className="flex flex-row items-center gap-2">
        {item.kind === 'directory' ? (
          <Icon size="lg" className="fill-amber-500" icon={FolderIcon} />
        ) : (
          <Icon
            size="lg"
            className={clsx(getLeagueFileKindIconColor(item.extensionKind))}
            icon={getLeagueFileKindIcon(item.extensionKind)}
          />
        )}
        {item.name}
      </div>
    </div>
  );
};
