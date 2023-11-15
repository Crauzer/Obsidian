import { useQuery } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { actionsCommands, actionsQueryKeys } from '..';
import { ActionProgress } from '../types';

export const getActionProgress = (actionId: string) =>
  tauri.invoke<ActionProgress>(actionsCommands.getActionProgress, { actionId });

export const useActionProgress = (actionId: string) => {
  return useQuery({
    queryKey: actionsQueryKeys.actionProgress(actionId),
    queryFn: () => getActionProgress(actionId),
  });
};
