import { useQuery } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { MountedWadsResponse } from '../types';

export const getMountedWads = () => invoke<MountedWadsResponse>(wadCommands.getMountedWads);

export const useMountedWads = () => {
  return useQuery({ queryKey: wadQueryKeys.mountedWads, queryFn: getMountedWads });
};
