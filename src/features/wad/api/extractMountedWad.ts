import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadCommands } from '../commands';
import { ExtractMountedWadResponse } from '../types';

export type UseExtractMountedWadContext = {
  wadId: string;
};

export const extractMountedWad = ({ wadId }: UseExtractMountedWadContext) =>
  tauri.invoke<ExtractMountedWadResponse>(wadCommands.extractMountedWad, { wadId });

export const useExtractMountedWad = () => {
  return useMutation({ mutationFn: extractMountedWad });
};
