import { useMutation, useQueryClient } from '@tanstack/react-query';
import { tauri } from '@tauri-apps/api';

import { wadQueryKeys } from '..';
import { wadCommands } from '../commands';
import { WadItem, WadItemSelectionUpdate } from '../types';

export type UpdateMountedWadItemSelectionContext = {
  wadId: string;
  parentItemId?: string;
  resetSelection: boolean;
  itemSelections: WadItemSelectionUpdate[];
};

export const updateMountedWadItemSelection = ({
  wadId,
  parentItemId,
  resetSelection,
  itemSelections,
}: UpdateMountedWadItemSelectionContext) =>
  tauri.invoke(wadCommands.updateMountedWadItemSelection, {
    wadId,
    parentItemId,
    resetSelection,
    itemSelections,
  });

export const useUpdateMountedWadItemSelection = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateMountedWadItemSelection,
    onMutate: async ({ wadId, parentItemId, itemSelections }) => {
      await queryClient.cancelQueries({
        queryKey: wadQueryKeys.wadParentItems(wadId, parentItemId),
      });

      const previousItems = queryClient.getQueryData<WadItem[]>(
        wadQueryKeys.wadParentItems(wadId, parentItemId),
      );

      let items = [...(previousItems ?? [])];
      for (const itemSelection of itemSelections) {
        items[itemSelection.index].isSelected = itemSelection.isSelected;
      }

      queryClient.setQueryData(wadQueryKeys.wadParentItems(wadId, parentItemId), items);

      return { previousItems };
    },
    onError: (_error, variables, context) => {
      if (!context?.previousItems) {
        return;
      }

      queryClient.setQueryData(
        wadQueryKeys.wadParentItems(variables.wadId, variables.parentItemId),
        context.previousItems,
      );
    },
    onSettled: (_data, _error, variables) =>
      queryClient.invalidateQueries({
        queryKey: wadQueryKeys.wadParentItems(variables.wadId, variables.parentItemId),
      }),
  });
};
