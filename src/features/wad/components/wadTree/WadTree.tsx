import clsx from 'clsx';
import { useEffect } from 'react';
import { Virtuoso } from 'react-virtuoso';

import {
  CaretDownIcon,
  CaretRightIcon,
  FileIcon,
  FolderIcon,
  FolderOpenIcon,
} from '../../../../assets';
import { Icon } from '../../../../components';
import { useExpandWadTreeItem, useSelectWadTreeItem, useWadTree } from '../../api';
import { WadItem } from '../../types';

export type WadTreeProps = { wadId: string };

export const WadTree: React.FC<WadTreeProps> = ({ wadId }) => {
  const wadTreeQuery = useWadTree(wadId);
  const expandWadTreeItemMutation = useExpandWadTreeItem();
  const selectWadTreeItemMutation = useSelectWadTreeItem();

  useEffect(() => {
    if (wadTreeQuery.isSuccess) {
      console.info(wadTreeQuery.data);
    }
  }, [wadTreeQuery.isSuccess]);

  return (
    <Virtuoso
      className="min-h-[500px]"
      data={wadTreeQuery.data}
      itemContent={(index, item) => (
        <div
          className={clsx(
            'flex flex-row text-md text-gray-50  hover:cursor-pointer select-none',
            { 'hover:bg-gray-500/25': !item.isSelected },
            { 'bg-obsidian-500/30 outline outline-1 outline-obsidian-500/75': item.isSelected },
          )}
          onClick={() => {
            if (item.kind === 'directory') {
              expandWadTreeItemMutation.mutate(
                {
                  wadId,
                  itemId: item.id,
                  isExpanded: !item.isExpanded,
                },
                { onError: (error) => console.error(error) },
              );
            }
            selectWadTreeItemMutation.mutate({
              wadId,
              itemId: item.id,
              itemIndex: index,
              isSelected: true,
            });
          }}
        >
          {[...Array(item.level).keys()].map((i) => {
            return (
              <div
                key={i}
                className="after:content-[''] after:h-full after:block after:ml-[0.4rem] after:border-l after:border-l-gray-400/30 w-[25px] "
              />
            );
          })}
          <div className="flex flex-row items-center gap-2">
            {item.kind === 'directory' ? (
              <Icon size="sm" icon={item.isExpanded ? CaretDownIcon : CaretRightIcon} />
            ) : (
              <Icon className="invisible" size="sm" icon={CaretRightIcon} />
            )}
            {item.kind === 'directory' ? (
              item.isExpanded ? (
                <Icon size="md" className="fill-amber-500" icon={FolderOpenIcon} />
              ) : (
                <Icon size="md" className="fill-amber-500" icon={FolderIcon} />
              )
            ) : (
              <Icon size="md" className="fill-slate-400" icon={FileIcon} />
            )}
            {item.name}
          </div>
        </div>
      )}
    />
  );
};

export type WadTreeNodeProps = {
  item: WadItem;
};
