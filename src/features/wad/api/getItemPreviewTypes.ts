import { useQuery } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";

import { wadCommands } from "../commands";
import { wadQueryKeys } from "../queryKeys";
import type { WadChunkPreviewType } from "../types";

export const getItemPreviewTypes = (wadId: string, itemId: string) =>
  core.invoke<WadChunkPreviewType[]>(wadCommands.getChunkPreviewTypes, {
    wadId,
    itemId,
  });

export const useItemPreviewTypes = ({
  wadId,
  itemId,
  enabled = true,
}: {
  wadId: string;
  itemId: string;
  enabled?: boolean;
}) => {
  return useQuery({
    queryKey: wadQueryKeys.itemPreviewTypes(wadId, itemId),
    queryFn: () => getItemPreviewTypes(wadId, itemId),
    enabled,
    placeholderData: [],
  });
};
