import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { Settings, settingsCommands, settingsQueryKeys } from '..';

export const getSettings = () => tauri.invoke<Settings>(settingsCommands.getSettings);

export const useSettings = () => {
  return useQuery({
    queryKey: settingsQueryKeys.settings,
    queryFn: getSettings,
  });
};
