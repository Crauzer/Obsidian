import { useQuery } from '@tanstack/react-query';
import { core } from '@tauri-apps/api';

import { AppDirectoryResponse, fsCommands, fsQueryKeys } from '..';

export const getAppDirectory = () => core.invoke<AppDirectoryResponse>(fsCommands.getAppDirectory);

export const useAppDirectory = () => {
  return useQuery({ queryKey: fsQueryKeys.appDirectory, queryFn: getAppDirectory });
};
