import { useMutation } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { PickDirectoryResponse, fsCommands } from '..';

export type PickDirectoryContext = {
  initialDirectory?: string;
};

export const pickDirectory = ({ initialDirectory }: PickDirectoryContext) =>
  invoke<PickDirectoryResponse>(fsCommands.pickDirectory, { initialDirectory });

export const usePickDirectory = () => {
  return useMutation({ mutationFn: pickDirectory });
};
