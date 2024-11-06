import { useQuery } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { WadHashtableStatus } from '..';
import { wadHashtableCommands } from '../commands';
import { wadHashtableQueryKeys } from '../queryKeys';

export const getWadHashtableStatus = () =>
  invoke<WadHashtableStatus>(wadHashtableCommands.getWadHashtableStatus);

export const useWadHashtableStatus = () => {
  return useQuery({
    queryKey: wadHashtableQueryKeys.wadHashtableStatus,
    queryFn: getWadHashtableStatus,
  });
};
