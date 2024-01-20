import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadCommands } from '../commands';

export type ExtractWadChunksContext = {
  wadId: string;
  actionId: string;
  chunkPathHashes: string[];
  extractDirectory: string;
};

export const extractWadChunks = ({
  wadId,
  actionId,
  chunkPathHashes,
  extractDirectory,
}: ExtractWadChunksContext) =>
  tauri.invoke(wadCommands.extractWadChunks, {
    wadId,
    actionId,
    chunkPathHashes,
    extractDirectory,
  });

export const useExtractWadChunks = () => {
  return useMutation({ mutationFn: extractWadChunks });
};
