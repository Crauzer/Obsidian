import { useMutation, useQueryClient } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { WadItem, WadItemSelectionUpdate } from '../types';

export type UseUpdateMountedWadItemSelectionContext = {
  wadId: string;
  parentId?: string;
  resetSelection: boolean;
  itemSelections: Map<string, boolean>;
};

export type UpdateMountedWadItemSelectionContext = {
  wadId: string;
  resetSelection: boolean;
  itemSelections: Map<string, boolean>;
};

export const updateMountedWadItemSelection = ({
  wadId,
  resetSelection,
  itemSelections,
}: UpdateMountedWadItemSelectionContext) =>
  tauri.invoke(wadCommands.updateMountedWadItemSelection, {
    wadId,
    resetSelection,
    itemSelections,
  });

export const useUpdateMountedWadItemSelection = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      wadId,
      resetSelection,
      itemSelections,
    }: UseUpdateMountedWadItemSelectionContext) => {
      return updateMountedWadItemSelection({ wadId, resetSelection, itemSelections });
    },
    onSuccess: (_, { wadId, parentId }) => {
      queryClient.invalidateQueries({ queryKey: wadQueryKeys.wadParentItems(wadId, parentId) });
    },
  });
};
