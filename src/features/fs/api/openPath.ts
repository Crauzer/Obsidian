import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { fsCommands } from '..';

export type UseOpenPath = {
  path: string;
};

export const openPath = ({ path }: UseOpenPath) => tauri.invoke(fsCommands.openPath, { path });

export const useOpenPath = () => {
  return useMutation({ mutationFn: openPath });
};
