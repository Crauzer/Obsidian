import { useMutation } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { PickFileResponse, fsCommands } from '..';

export const pickFile = () => invoke<PickFileResponse>(fsCommands.pickFile);

export const usePickFile = () => {
  return useMutation({ mutationFn: pickFile });
};
