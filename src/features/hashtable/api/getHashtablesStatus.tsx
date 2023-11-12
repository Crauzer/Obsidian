import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { hashtableCommands } from '../commands';
import { hashtableQueryKeys } from '../queryKeys';
import { HashtablesStatusResponse } from '../types';

export const getHashtablesStatus = () =>
  tauri.invoke<HashtablesStatusResponse>(hashtableCommands.getHashtablesStatus);

export const useHashtablesStatus = () => {
  return useQuery({ queryKey: hashtableQueryKeys.hashtablesStatus, queryFn: getHashtablesStatus });
};
