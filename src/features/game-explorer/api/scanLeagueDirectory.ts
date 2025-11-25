import { useQuery } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";

import { gameExplorerCommands } from "../commands";
import { gameExplorerQueryKeys } from "../queryKeys";
import type {
  GameExplorerItem,
  GameExplorerPathComponent,
  GameExplorerStatusResponse,
  MountGameExplorerResponse,
} from "../types";

// Get game explorer status
export const getGameExplorerStatus = () =>
  core.invoke<GameExplorerStatusResponse>(
    gameExplorerCommands.getGameExplorerStatus,
  );

export const useGameExplorerStatus = () => {
  return useQuery({
    queryKey: gameExplorerQueryKeys.status,
    queryFn: getGameExplorerStatus,
  });
};

// Mount game explorer (load all WADs) - runs automatically when league directory is configured
export const mountGameExplorer = () =>
  core.invoke<MountGameExplorerResponse>(
    gameExplorerCommands.mountGameExplorer,
  );

export const useMountGameExplorer = (enabled = false) => {
  return useQuery({
    queryKey: gameExplorerQueryKeys.mount,
    queryFn: mountGameExplorer,
    enabled,
    staleTime: Number.POSITIVE_INFINITY, // Don't refetch automatically
    retry: false, // Don't retry on failure
  });
};

// Get game explorer items
export type GetGameExplorerItemsParams = {
  parentId?: string;
};

export const getGameExplorerItems = ({
  parentId,
}: GetGameExplorerItemsParams) =>
  core.invoke<GameExplorerItem[]>(gameExplorerCommands.getGameExplorerItems, {
    parentId,
  });

export const useGameExplorerItems = (parentId?: string, enabled = true) => {
  return useQuery({
    queryKey: gameExplorerQueryKeys.items(parentId),
    queryFn: () => getGameExplorerItems({ parentId }),
    enabled,
  });
};

// Get path components
export const getGameExplorerPathComponents = (itemId: string) =>
  core.invoke<GameExplorerPathComponent[]>(
    gameExplorerCommands.getGameExplorerPathComponents,
    { itemId },
  );

export const useGameExplorerPathComponents = (itemId?: string) => {
  return useQuery({
    queryKey: gameExplorerQueryKeys.pathComponents(itemId),
    queryFn: () => {
      if (!itemId) {
        return Promise.resolve([]);
      }
      return getGameExplorerPathComponents(itemId);
    },
    enabled: !!itemId,
  });
};
