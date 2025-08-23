import { useMutation } from "@tanstack/react-query";
import { core } from "@tauri-apps/api";
import { queryClient } from "../../../lib/query";
import { wadQueryKeys } from "..";
import { wadCommands } from "../commands";
import type { MountWadResponse } from "../types";

export type MountWadsContext = {
  wadPaths?: string[];
};

export const mountWads = ({ wadPaths }: MountWadsContext) =>
  core.invoke<MountWadResponse>(wadCommands.mountWads, { wadPaths });

export const useMountWads = () => {
  return useMutation({
    mutationFn: mountWads,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWads });
    },
  });
};
