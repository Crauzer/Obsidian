import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { flattenWadTree } from '../factories';
import { WadTreeResponse } from '../types';

export const getWadTree = (wadId: string) =>
  tauri.invoke<WadTreeResponse>(wadCommands.getWadTree, { wadId });

export const useWadTree = (wadId: string) => {
  return useQuery({
    queryKey: wadQueryKeys.wadTree(wadId),
    queryFn: async () => {
      const wadTree = await getWadTree(wadId);

      return flattenWadTree(wadTree.items);
    },
  });
};
