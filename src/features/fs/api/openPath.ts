import { useMutation } from '@tanstack/react-query';
import { core } from '@tauri-apps/api';

import { fsCommands } from '..';

export type UseOpenPath = {
  path: string;
};

export const openPath = ({ path }: UseOpenPath) => core.invoke(fsCommands.openPath, { path });

export const useOpenPath = () => {
  return useMutation({ mutationFn: openPath });
};
