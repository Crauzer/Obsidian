import React, { useState } from 'react';
import { isHotkeyPressed } from 'react-hotkeys-hook';
import AutoSizer from 'react-virtualized-auto-sizer';
import { Virtuoso } from 'react-virtuoso';

import { createArrayRange } from '../../../../utils/array';
import { useUpdateMountedWadItemSelection } from '../../api';
import { WadItem } from '../../types';
import { WadItemListRow } from './WadItemListRow';

export type WadItemListProps = {
  wadId: string;
  parentItemId?: string;
  data: WadItem[];
};

export const WadItemList: React.FC<WadItemListProps> = ({ wadId, parentItemId, data }) => {
  const [latestSelectedIndex, setLatestSelectedIndex] = useState<number | undefined>();

  const updateMountedWadItemSelection = useUpdateMountedWadItemSelection();

  const handleShiftClick = (index: number) => {
    const startIndex = Math.min(latestSelectedIndex ?? 0, index);
    const endIndex = Math.max(latestSelectedIndex ?? 0, index);

    updateMountedWadItemSelection.mutate({
      wadId,
      parentId: parentItemId,
      resetSelection: false,
      itemSelections: new Map(
        createArrayRange(endIndex - startIndex + 1, startIndex).map((x) => [data[x].id, true]),
      ),
    });
  };

  const onRowClicked = (index: number, _isRightClick: boolean) => {
    setLatestSelectedIndex(index);

    if (isHotkeyPressed('shift')) {
      handleShiftClick(index);
    } else if (isHotkeyPressed('ctrl')) {
      const item = data[index];

      updateMountedWadItemSelection.mutate({
        wadId,
        parentId: parentItemId,
        resetSelection: false,
        itemSelections: new Map([[item.id, !item.isSelected]]),
      });
    } else {
      updateMountedWadItemSelection.mutate({
        wadId,
        parentId: parentItemId,
        resetSelection: true,
        itemSelections: new Map([[data[index].id, true]]),
      });
    }
  };

  return (
    <div className="h-full flex-auto">
      <AutoSizer>
        {({ height, width }) => (
          <Virtuoso
            style={{ height, width }}
            data={data}
            itemContent={(index, item) => (
              <WadItemListRow
                item={item}
                wadId={wadId}
                parentItemId={parentItemId}
                index={index}
                onClick={(event) => onRowClicked(index, event.type === 'contextmenu')}
              />
            )}
          />
        )}
      </AutoSizer>
    </div>
  );
};
