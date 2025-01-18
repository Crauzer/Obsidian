import { useMutation } from '@tanstack/react-query';
import { core } from '@tauri-apps/api';

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
  core.invoke(wadCommands.extractMountedWad, {
    wadId,
    actionId,
    extractDirectory,
  });

export const useExtractMountedWad = () => {
  return useMutation({ mutationFn: extractMountedWad });
};
