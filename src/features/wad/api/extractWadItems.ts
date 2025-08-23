import { useMutation } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";

import { wadCommands } from "../commands";

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
  core.invoke(wadCommands.extractWadItems, {
    wadId,
    actionId,
    parentItemId,
    items,
    extractDirectory,
  });

export const useExtractWadItems = () => {
  return useMutation({ mutationFn: extractWadItems });
};
