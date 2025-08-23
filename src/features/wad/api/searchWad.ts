import { useQuery } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";

import { wadQueryKeys } from "..";
import { wadCommands } from "../commands";
import type { SearchWadResponse } from "../types";

export type SearchWadContext = {
  wadId: string;
  query: string;
};

export const searchWad = ({ wadId, query }: SearchWadContext) =>
  core.invoke<SearchWadResponse>(wadCommands.searchWad, { wadId, query });

export const useSearchWad = ({ wadId, query }: SearchWadContext) => {
  return useQuery({
    queryKey: wadQueryKeys.wadSearch(wadId, query),
    queryFn: () => searchWad({ wadId, query }),
  });
};
