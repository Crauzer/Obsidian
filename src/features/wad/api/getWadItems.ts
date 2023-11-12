import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { WadItem } from '../types';

export type UseWadItemsContext = {
  wadId: string;
};

export const getWadItems = ({ wadId }: UseWadItemsContext) =>
  tauri.invoke<WadItem[]>(wadCommands.getWadItems, { wadId });

export const useWadItems = (wadId: string) => {
  return useQuery({
    queryKey: wadQueryKeys.mountedWadItems(wadId),
    queryFn: () => getWadItems({ wadId }),
  });
};
