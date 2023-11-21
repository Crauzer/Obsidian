import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { PickFileResponse, fsCommands } from '..';

export const pickFile = () => tauri.invoke<PickFileResponse>(fsCommands.pickFile);

export const usePickFile = () => {
  return useMutation({ mutationFn: pickFile });
};
