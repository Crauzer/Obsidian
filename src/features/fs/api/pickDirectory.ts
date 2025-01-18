import { useMutation } from '@tanstack/react-query';
import { core } from '@tauri-apps/api';

import { PickDirectoryResponse, fsCommands } from '..';

export type PickDirectoryContext = {
  initialDirectory?: string;
};

export const pickDirectory = ({ initialDirectory }: PickDirectoryContext) =>
  core.invoke<PickDirectoryResponse>(fsCommands.pickDirectory, { initialDirectory });

export const usePickDirectory = () => {
  return useMutation({ mutationFn: pickDirectory });
};
