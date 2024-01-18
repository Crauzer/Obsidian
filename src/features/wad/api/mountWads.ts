import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { wadCommands } from '../commands';
import { MountWadResponse } from '../types';

export type MountWadsContext = {
  wadPaths?: string[];
};

export const mountWads = ({ wadPaths }: MountWadsContext) =>
  tauri.invoke<MountWadResponse>(wadCommands.mountWads, { wadPaths });

export const useMountWads = () => {
  return useMutation({
    mutationFn: mountWads,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
