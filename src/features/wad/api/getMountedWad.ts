import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { MountedWad } from '../types';

export const getMountedWad = (wadId: string) =>
  tauri.invoke<MountedWad>(wadCommands.getMountedWad, { wadId });

export const useMountedWad = (wadId: string) => {
  return useQuery({
    queryKey: wadQueryKeys.mountedWad(wadId),
    queryFn: () => getMountedWad(wadId),
  });
};
