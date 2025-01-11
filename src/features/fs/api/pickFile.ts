import { useMutation } from '@tanstack/react-query';
import { core } from '@tauri-apps/api';

import { PickFileResponse, fsCommands } from '..';

export const pickFile = () => core.invoke<PickFileResponse>(fsCommands.pickFile, { app: core });

export const usePickFile = () => {
  return useMutation({ mutationFn: pickFile });
};
