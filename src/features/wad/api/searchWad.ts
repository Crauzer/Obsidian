import { useQuery } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { SearchWadResponse } from '../types';

export type SearchWadContext = {
  wadId: string;
  query: string;
};

export const searchWad = ({ wadId, query }: SearchWadContext) =>
  invoke<SearchWadResponse>(wadCommands.searchWad, { wadId, query });

export const useSearchWad = ({ wadId, query }: SearchWadContext) => {
  return useQuery({
    queryKey: wadQueryKeys.wadSearch(wadId, query),
    queryFn: () => searchWad({ wadId, query }),
  });
};
