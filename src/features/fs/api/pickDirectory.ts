import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { PickDirectoryResponse, fsCommands } from '..';

export const pickDirectory = () => tauri.invoke<PickDirectoryResponse>(fsCommands.pickDirectory);

export const usePickDirectory = () => {
  return useMutation({ mutationFn: pickDirectory });
};
