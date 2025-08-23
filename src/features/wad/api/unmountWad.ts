import { useMutation } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";
import { queryClient } from "../../../lib/query";
import { wadQueryKeys } from "..";
import { wadCommands } from "../commands";
import type { MountedWadsResponse } from "../types";

export type UseUnmountWadContext = {
  wadId: string;
};

export const unmountWad = ({ wadId }: UseUnmountWadContext) =>
  core.invoke(wadCommands.unmountWad, { wadId });

export const useUnmountWad = () => {
  return useMutation({
    mutationFn: unmountWad,
    onMutate: async ({ wadId }) => {
      await queryClient.cancelQueries({ queryKey: wadQueryKeys.mountedWads });

      const previousData = queryClient.getQueryData<MountedWadsResponse>(
        wadQueryKeys.mountedWads,
      );
      if (previousData) {
        queryClient.setQueryData(wadQueryKeys.mountedWads, {
          wads: previousData.wads.filter(
            (mountedWad) => mountedWad.id != wadId,
          ),
        } satisfies MountedWadsResponse);
      }

      return { previousData };
    },
    onError: (_error, _variables, context) => {
      if (context?.previousData) {
        queryClient.setQueryData(
          wadQueryKeys.mountedWads,
          context.previousData,
        );
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
