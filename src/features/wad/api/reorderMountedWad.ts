import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { wadCommands } from '../commands';

export type UseReorderMountedWadContext = {
  sourceIndex: number;
  destIndex: number;
};

export const reorderMountedWad = ({ sourceIndex, destIndex }: UseReorderMountedWadContext) =>
  tauri.invoke(wadCommands.moveMountedWad, { sourceIndex, destIndex });

export const useReorderMountedWad = () => {
  return useMutation({
    mutationFn: reorderMountedWad,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
