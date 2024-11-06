import { useMutation } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { wadCommands } from '../commands';

export type UseExpandWadTreeItemContext = {
  wadId: string;
  itemId: string;
  isExpanded: boolean;
};

export const expandWadTreeItem = ({ wadId, itemId, isExpanded }: UseExpandWadTreeItemContext) =>
  invoke(wadCommands.expandWadTreeItem, {
    wadId,
    itemId,
    isExpanded,
  });

export const useExpandWadTreeItem = () => {
  return useMutation({
    mutationFn: expandWadTreeItem,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.wadTree(variables.wadId) });
    },
  });
};
