import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { wadCommands } from '../commands';
import { MountWadResponse } from '../types';

export const mountWad = () => tauri.invoke<MountWadResponse>(wadCommands.mountWad);

export const useMountWad = () => {
  return useMutation({
    mutationFn: mountWad,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
