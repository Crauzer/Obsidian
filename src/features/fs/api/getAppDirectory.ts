import { useQuery } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { AppDirectoryResponse, fsCommands, fsQueryKeys } from '..';

export const getAppDirectory = () => invoke<AppDirectoryResponse>(fsCommands.getAppDirectory);

export const useAppDirectory = () => {
  return useQuery({ queryKey: fsQueryKeys.appDirectory, queryFn: getAppDirectory });
};
