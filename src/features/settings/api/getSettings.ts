import { useQuery } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { Settings, settingsCommands, settingsQueryKeys } from '..';

export type UseSettingsParams<TData> = {
  select?: (data: Settings) => TData;
};

export const getSettings = () => invoke<Settings>(settingsCommands.getSettings);

export const useSettings = <TData = Settings>({ select }: UseSettingsParams<TData>) => {
  return useQuery({
    queryKey: settingsQueryKeys.settings,
    queryFn: getSettings,
    select,
  });
};
