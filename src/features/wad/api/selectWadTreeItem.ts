import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { wadCommands } from '../commands';
import { WadItem } from '../types';

export type UseSelectWadTreeItemContext = {
  wadId: string;
  itemIndex: number;
  itemId: string;
  isSelected: boolean;
};

export const selectWadTreeItem = ({ wadId, itemId, isSelected }: UseSelectWadTreeItemContext) =>
  tauri.invoke(wadCommands.selectWadTreeItem, {
    wadId,
    itemId,
    isSelected,
  });

export const useSelectWadTreeItem = () => {
  return useMutation({
    mutationFn: selectWadTreeItem,
    onMutate: async ({ wadId, itemIndex, itemId, isSelected }) => {
      await queryClient.cancelQueries({
        queryKey: [wadQueryKeys.wadItems(wadId), wadQueryKeys.mountedWadItems(wadId)],
      });

      const previousWadItems = queryClient.getQueryData<WadItem[]>(wadQueryKeys.wadItems(wadId));
      const previousWadDirectoryItems = queryClient.getQueryData<WadItem[]>(
        wadQueryKeys.mountedWadDirectoryItems(wadId, itemId),
      );

      const mapWadItem = (item: WadItem, index: Number) => {
        if (index === itemIndex) {
          item.isSelected = isSelected;
        } else if (isSelected) {
          item.isSelected = false;
        }

        return item;
      };

      if (previousWadItems) {
        queryClient.setQueryData(
          wadQueryKeys.mountedWadItems(wadId),
          previousWadItems.map(mapWadItem),
        );
      }
      if (previousWadDirectoryItems) {
        queryClient.setQueryData(
          wadQueryKeys.mountedWadDirectoryItems(wadId, itemId),
          previousWadDirectoryItems.map(mapWadItem),
        );
      }

      return { previousWadItems, previousWadDirectoryItems };
    },
    onError: (_error, variables, context) => {
      if (context?.previousWadItems) {
        queryClient.setQueryData(wadQueryKeys.wadItems(variables.wadId), context.previousWadItems);
      }
      if (context?.previousWadDirectoryItems) {
        queryClient.setQueryData(
          wadQueryKeys.mountedWadDirectoryItems(variables.wadId, variables.itemId),
          context.previousWadDirectoryItems,
        );
      }
    },
    onSettled: (_data, _error, variables) => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWadItems(variables.wadId) });
      queryClient.invalidateQueries({
        queryKey: wadQueryKeys.mountedWadDirectoryItems(variables.wadId, variables.itemId),
      });
    },
  });
};
