import { useQuery } from '@tanstack/react-query';
import { core } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { MountedWadsResponse } from '../types';

export const getMountedWads = () => core.invoke<MountedWadsResponse>(wadCommands.getMountedWads);

export const useMountedWads = () => {
  return useQuery({ queryKey: wadQueryKeys.mountedWads, queryFn: getMountedWads });
};
