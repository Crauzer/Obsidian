import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { WadItem } from '../types';

export type UseWadDirectoryItemsContext = {
  wadId: string;
  itemId: string;
};

export const getWadDirectoryItems = ({ wadId, itemId }: UseWadDirectoryItemsContext) =>
  tauri.invoke<WadItem[]>(wadCommands.getMountedWadDirectoryItems, { wadId, itemId });

export const useWadDirectoryItems = (wadId: string, itemId: string) => {
  return useQuery({
    queryKey: wadQueryKeys.mountedWadDirectoryItems(wadId, itemId),
    queryFn: () => getWadDirectoryItems({ wadId, itemId }),
  });
};
