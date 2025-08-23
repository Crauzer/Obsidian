import { useQuery } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";

import type { WadHashtableStatus } from "..";
import { wadHashtableCommands } from "../commands";
import { wadHashtableQueryKeys } from "../queryKeys";

export const getWadHashtableStatus = () =>
  core.invoke<WadHashtableStatus>(wadHashtableCommands.getWadHashtableStatus);

export const useWadHashtableStatus = () => {
  return useQuery({
    queryKey: wadHashtableQueryKeys.wadHashtableStatus,
    queryFn: getWadHashtableStatus,
  });
};
