import { useMutation } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadCommands } from '../commands';

export type UseExtractMountedWadContext = {
  wadId: string;
  actionId: string;
  extractDirectory: string;
};

export const extractMountedWad = ({
  wadId,
  actionId,
  extractDirectory,
}: UseExtractMountedWadContext) =>
  invoke(wadCommands.extractMountedWad, {
    wadId,
    actionId,
    extractDirectory,
  });

export const useExtractMountedWad = () => {
  return useMutation({ mutationFn: extractMountedWad });
};
