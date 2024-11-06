import { useQuery } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { WadItemPathComponent } from '../types';

export type UseWadDirectoryPathComponentsContext = {
  wadId: string;
  itemId: string;
};

export const getWadDirectoryPathComponents = ({
  wadId,
  itemId,
}: UseWadDirectoryPathComponentsContext) =>
  invoke<WadItemPathComponent[]>(wadCommands.getMountedWadDirectoryPathComponents, {
    wadId,
    itemId,
  });

export const useWadDirectoryPathComponents = ({
  wadId,
  itemId,
}: UseWadDirectoryPathComponentsContext) => {
  return useQuery({
    queryKey: wadQueryKeys.mountedWadDirectoryPathComponents(wadId, itemId),
    queryFn: () => getWadDirectoryPathComponents({ wadId, itemId }),
  });
};
