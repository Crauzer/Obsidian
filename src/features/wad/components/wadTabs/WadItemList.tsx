import clsx from 'clsx';
import { useSearchParams } from 'react-router-dom';
import { Virtuoso } from 'react-virtuoso';

import { FileIcon, FolderIcon } from '../../../../assets';
import { Icon } from '../../../../components';
import { useSelectWadTreeItem } from '../../api';
import { WadItem } from '../../types';

export type WadItemListProps = {
  wadId: string;
  data: WadItem[];
};

export const WadItemList: React.FC<WadItemListProps> = ({ wadId, data }) => {
  const [_, setSearchParams] = useSearchParams();

  const selectWadTreeItemMutation = useSelectWadTreeItem();

  return (
    <div className="flex flex-col gap-2">
      <Virtuoso
        className="h-full min-h-[500px]"
        data={data}
        itemContent={(index, item) => {
          return (
            <div
              tabIndex={0}
              className={clsx(
                'text-md flex select-none flex-row  pl-2 text-gray-50 hover:cursor-pointer',
                { 'hover:bg-gray-500/25': !item.isSelected },
                { 'border border-obsidian-500/75 bg-obsidian-500/30': item.isSelected },
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
                  <Icon size="md" className="fill-amber-500" icon={FolderIcon} />
                ) : (
                  <Icon size="md" className="fill-slate-400" icon={FileIcon} />
                )}
                {item.name}
              </div>
            </div>
          );
        }}
      />
    </div>
  );
};
