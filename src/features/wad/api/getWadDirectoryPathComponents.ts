import { useQuery } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";

import { wadQueryKeys } from "..";
import { wadCommands } from "../commands";
import type { WadItemPathComponent } from "../types";

export type UseWadDirectoryPathComponentsContext = {
  wadId: string;
  itemId: string;
};

export const getWadDirectoryPathComponents = ({
  wadId,
  itemId,
}: UseWadDirectoryPathComponentsContext) =>
  core.invoke<WadItemPathComponent[]>(
    wadCommands.getMountedWadDirectoryPathComponents,
    {
      wadId,
      itemId,
    },
  );

export const useWadDirectoryPathComponents = ({
  wadId,
  itemId,
}: UseWadDirectoryPathComponentsContext) => {
  return useQuery({
    queryKey: wadQueryKeys.mountedWadDirectoryPathComponents(wadId, itemId),
    queryFn: () => getWadDirectoryPathComponents({ wadId, itemId }),
  });
};
