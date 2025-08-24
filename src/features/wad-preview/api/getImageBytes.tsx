import { core } from "@tauri-apps/api";
import { wadPreviewCommands } from "../commands";

export const getImageBytes = async (wadId: string, itemId: string) => {
  const result = await core.invoke<number[]>(wadPreviewCommands.getImageBytes, {
    wadId,
    itemId,
  });

  return new Uint8Array(result);
};
