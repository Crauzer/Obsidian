import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { Settings, settingsCommands, settingsQueryKeys } from '..';

export type UseSettingsParams<TData> = {
  select?: (data: Settings) => TData;
};

export const getSettings = () => tauri.invoke<Settings>(settingsCommands.getSettings);

export const useSettings = <TData = Settings>({ select }: UseSettingsParams<TData>) => {
  return useQuery({
    queryKey: settingsQueryKeys.settings,
    queryFn: getSettings,
    select,
  });
};
