import { useMutation } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { hashtableCommands } from '../commands';

export const loadHashtables = () => tauri.invoke(hashtableCommands.loadHashtables);

export const useLoadHashTables = () => {
  return useMutation({ mutationFn: loadHashtables });
};
