import { useMutation } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";
import { queryClient } from "../../../lib/query";
import { wadQueryKeys } from "..";
import { wadCommands } from "../commands";

export type UseReorderMountedWadContext = {
  sourceIndex: number;
  destIndex: number;
};

export const reorderMountedWad = ({
  sourceIndex,
  destIndex,
}: UseReorderMountedWadContext) =>
  core.invoke(wadCommands.moveMountedWad, { sourceIndex, destIndex });

export const useReorderMountedWad = () => {
  return useMutation({
    mutationFn: reorderMountedWad,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
