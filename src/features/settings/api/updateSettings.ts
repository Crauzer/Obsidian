import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { Settings, settingsCommands, settingsQueryKeys } from '..';
import { queryClient } from '../../../lib/query';

export type UseUpdateSettingsContext = {
  settings: Settings;
};

export const updateSettings = ({ settings }: UseUpdateSettingsContext) =>
  tauri.invoke(settingsCommands.updateSettings, { settings });

export const useUpdateSettings = () => {
  return useMutation({
    mutationFn: updateSettings,
    onMutate: async ({ settings }) => {
      await queryClient.cancelQueries({ queryKey: settingsQueryKeys.settings });

      const previousData = queryClient.getQueryData<Settings>(settingsQueryKeys.settings);
      queryClient.setQueryData(settingsQueryKeys.settings, settings);

      return { previousData };
    },
    onError: (_error, _variables, context) => {
      if (context?.previousData) {
        queryClient.setQueryData(settingsQueryKeys.settings, context.previousData);
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: settingsQueryKeys.settings });
    },
  });
};
