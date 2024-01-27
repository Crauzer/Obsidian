import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { WadItem } from '../types';

export type UseWadDirectoryItemsContext = {
  wadId: string;
  parentId?: string;
};

export const getWadParentItems = ({ wadId, parentId }: UseWadDirectoryItemsContext) =>
  tauri.invoke<WadItem[]>(wadCommands.getWadParentItems, { wadId, parentId });

export const useWadParentItems = ({ wadId, parentId }: UseWadDirectoryItemsContext) => {
  return useQuery({
    queryKey: wadQueryKeys.wadParentItems(wadId, parentId),
    queryFn: () => getWadParentItems({ wadId, parentId }),
  });
};
