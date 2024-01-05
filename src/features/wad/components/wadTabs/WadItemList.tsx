import React, { useState } from 'react';
import { isHotkeyPressed } from 'react-hotkeys-hook';
import AutoSizer from 'react-virtualized-auto-sizer';
import { Virtuoso } from 'react-virtuoso';

import { createArrayRange } from '../../../../utils/array';
import { useUpdateMountedWadItemSelection } from '../../api';
import { WadItem, WadItemSelectionUpdate } from '../../types';
import { WadItemListRow } from './WadItemListRow';

export type WadItemListProps = {
  wadId: string;
  parentItemId?: string;
  data: WadItem[];
};

export const WadItemList: React.FC<WadItemListProps> = ({ wadId, parentItemId, data }) => {
  const [latestSelectedIndex, setLatestSelectedIndex] = useState<number | undefined>();

  const updateMountedWadItemSelection = useUpdateMountedWadItemSelection();

  const onRowClicked = (index: number) => {
    setLatestSelectedIndex(index);

    if (isHotkeyPressed('shift')) {
      const startIndex = Math.min(latestSelectedIndex ?? 0, index);
      const endIndex = Math.max(latestSelectedIndex ?? 0, index);

      updateMountedWadItemSelection.mutate({
        wadId,
        parentItemId,
        resetSelection: false,
        itemSelections: createArrayRange(
          endIndex - startIndex + 1,
          startIndex,
        ).map<WadItemSelectionUpdate>((x) => {
          return {
            index: x,
            isSelected: true,
          };
        }),
      });
    } else if (isHotkeyPressed('ctrl')) {
      updateMountedWadItemSelection.mutate({
        wadId,
        parentItemId,
        resetSelection: false,
        itemSelections: [{ index, isSelected: !data[index].isSelected }],
      });
    } else {
      updateMountedWadItemSelection.mutate({
        wadId,
        parentItemId,
        resetSelection: true,
        itemSelections: [{ index, isSelected: true }],
      });
    }
  };

  return (
    <div style={{ flex: '1 1 auto' }}>
      <AutoSizer>
        {({ height, width }) => (
          <Virtuoso
            style={{ height, width }}
            data={data}
            itemContent={(index, item) => (
              <WadItemListRow
                item={item}
                wadId={wadId}
                index={index}
                onClick={() => onRowClicked(index)}
              />
            )}
          />
        )}
      </AutoSizer>
    </div>
  );
};
