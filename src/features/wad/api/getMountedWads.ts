import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { MountedWadsResponse } from '../types';

export const getMountedWads = () => tauri.invoke<MountedWadsResponse>(wadCommands.getMountedWads);

export const useMountedWads = () => {
  return useQuery({ queryKey: wadQueryKeys.mountedWads, queryFn: getMountedWads });
};
