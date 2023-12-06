import clsx from 'clsx';
import React from 'react';
import { useSearchParams } from 'react-router-dom';
import AutoSizer from 'react-virtualized-auto-sizer';
import { Virtuoso } from 'react-virtuoso';

import { FileIcon, FolderIcon } from '../../../../assets';
import { Icon } from '../../../../components';
import { useSelectWadTreeItem } from '../../api';
import { WadItem } from '../../types';
import { getLeagueFileKindIcon, getLeagueFileKindIconColor } from '../../utils';

export type WadItemListProps = {
  wadId: string;
  data: WadItem[];
};

export const WadItemList: React.FC<WadItemListProps> = ({ wadId, data }) => {
  const [_, setSearchParams] = useSearchParams();

  const selectWadTreeItemMutation = useSelectWadTreeItem();

  return (
    <div style={{ flex: '1 1 auto' }}>
      <AutoSizer>
        {({ height, width }) => (
          <Virtuoso
            style={{ height, width }}
            data={data}
            itemContent={(index, item) => {
              return (
                <div
                  className={clsx(
                    'text-md box-border flex select-none flex-row py-1 pl-2 text-gray-50 hover:cursor-pointer',
                    { 'hover:bg-gray-700/25': !item.isSelected },
                    {
                      'border border-obsidian-500/40 bg-obsidian-700/40': item.isSelected,
                    },
                  )}
                  onClick={() => {
                    selectWadTreeItemMutation.mutate({
                      wadId,
                      itemId: item.id,
                      itemIndex: index,
                      isSelected: true,
                    });
                  }}
                  onDoubleClick={() => {
                    if (item.kind !== 'directory') {
                      return;
                    }

                    // TODO: setting url query here isn't a good idea
                    setSearchParams((params) => {
                      params.set('itemId', item.id);
                      return params;
                    });
                  }}
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
            }}
          />
        )}
      </AutoSizer>
    </div>
  );
};
