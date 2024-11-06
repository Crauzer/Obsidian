import { useQuery } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { WadItem } from '../types';

export type UseWadDirectoryItemsContext = {
  wadId: string;
  parentId?: string;
};

export const getWadParentItems = ({ wadId, parentId }: UseWadDirectoryItemsContext) =>
  invoke<WadItem[]>(wadCommands.getWadParentItems, { wadId, parentId });

export const useWadParentItems = ({ wadId, parentId }: UseWadDirectoryItemsContext) => {
  return useQuery({
    queryKey: wadQueryKeys.wadParentItems(wadId, parentId),
    queryFn: () => getWadParentItems({ wadId, parentId }),
  });
};
