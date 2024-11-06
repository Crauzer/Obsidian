import { useMutation } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadCommands } from '../commands';

export type ExtractWadItemsContext = {
  wadId: string;
  actionId: string;
  parentItemId?: string;
  items: string[];
  extractDirectory: string;
};

export const extractWadItems = ({
  wadId,
  actionId,
  parentItemId,
  items,
  extractDirectory,
}: ExtractWadItemsContext) =>
  invoke(wadCommands.extractWadItems, {
    wadId,
    actionId,
    parentItemId,
    items,
    extractDirectory,
  });

export const useExtractWadItems = () => {
  return useMutation({ mutationFn: extractWadItems });
};
