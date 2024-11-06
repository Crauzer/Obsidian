import { useMutation } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { wadCommands } from '../commands';
import { MountWadResponse } from '../types';

export type MountWadsContext = {
  wadPaths?: string[];
};

export const mountWads = ({ wadPaths }: MountWadsContext) =>
  invoke<MountWadResponse>(wadCommands.mountWads, { wadPaths });

export const useMountWads = () => {
  return useMutation({
    mutationFn: mountWads,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
