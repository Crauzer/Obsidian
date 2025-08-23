import { useMutation } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";
import { queryClient } from "../../../lib/query";
import { type Settings, settingsCommands, settingsQueryKeys } from "..";

export type UseUpdateSettingsContext = {
  settings: Settings;
};

export const updateSettings = ({ settings }: UseUpdateSettingsContext) =>
  core.invoke(settingsCommands.updateSettings, { settings });

export const useUpdateSettings = () => {
  return useMutation({
    mutationFn: updateSettings,
    onMutate: async ({ settings }) => {
      await queryClient.cancelQueries({ queryKey: settingsQueryKeys.settings });

      const previousData = queryClient.getQueryData<Settings>(
        settingsQueryKeys.settings,
      );
      queryClient.setQueryData(settingsQueryKeys.settings, settings);

      return { previousData };
    },
    onError: (_error, _variables, context) => {
      if (context?.previousData) {
        queryClient.setQueryData(
          settingsQueryKeys.settings,
          context.previousData,
        );
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: settingsQueryKeys.settings });
    },
  });
};
