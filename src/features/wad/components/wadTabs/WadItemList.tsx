import React from 'react';
import AutoSizer from 'react-virtualized-auto-sizer';
import { Virtuoso } from 'react-virtuoso';

import { WadItem } from '../../types';
import { WadItemListRow } from './WadItemListRow';

export type WadItemListProps = {
  wadId: string;
  data: WadItem[];
};

export const WadItemList: React.FC<WadItemListProps> = ({ wadId, data }) => {
  return (
    <div style={{ flex: '1 1 auto' }}>
      <AutoSizer>
        {({ height, width }) => (
          <Virtuoso
            style={{ height, width }}
            data={data}
            itemContent={(index, item) => (
              <WadItemListRow item={item} wadId={wadId} index={index} />
            )}
          />
        )}
      </AutoSizer>
    </div>
  );
};
