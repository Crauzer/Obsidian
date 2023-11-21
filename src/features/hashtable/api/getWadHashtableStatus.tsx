import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { WadHashtableStatus } from '..';
import { wadHashtableCommands } from '../commands';
import { wadHashtableQueryKeys } from '../queryKeys';

export const getWadHashtableStatus = () =>
  tauri.invoke<WadHashtableStatus>(wadHashtableCommands.getWadHashtableStatus);

export const useWadHashtableStatus = () => {
  return useQuery({
    queryKey: wadHashtableQueryKeys.wadHashtableStatus,
    queryFn: getWadHashtableStatus,
  });
};
