import { useMutation, useQueryClient } from '@tanstack/react-query';
import { invoke } from '@tauri-apps/api/core';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';

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
  invoke(wadCommands.updateMountedWadItemSelection, {
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
