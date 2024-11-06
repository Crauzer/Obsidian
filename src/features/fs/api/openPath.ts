import { useMutation } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { fsCommands } from '..';

export type UseOpenPath = {
  path: string;
};

export const openPath = ({ path }: UseOpenPath) => invoke(fsCommands.openPath, { path });

export const useOpenPath = () => {
  return useMutation({ mutationFn: openPath });
};
