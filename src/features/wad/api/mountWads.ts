import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { wadCommands } from '../commands';
import { MountWadResponse } from '../types';

export const mountWads = () => tauri.invoke<MountWadResponse>(wadCommands.mountWads);

export const useMountWads = () => {
  return useMutation({
    mutationFn: mountWads,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
