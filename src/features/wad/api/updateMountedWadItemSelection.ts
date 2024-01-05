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
      if (parentItemId) {
        await queryClient.cancelQueries({
          queryKey: wadQueryKeys.mountedWadDirectoryItems(wadId, parentItemId),
        });
      } else {
        await queryClient.cancelQueries({ queryKey: wadQueryKeys.mountedWadItems(wadId) });
      }

      const previousItems = parentItemId
        ? queryClient.getQueryData<WadItem[]>(
            wadQueryKeys.mountedWadDirectoryItems(wadId, parentItemId),
          )
        : queryClient.getQueryData<WadItem[]>(wadQueryKeys.mountedWadItems(wadId));

      let items = [...(previousItems ?? [])];
      for (const itemSelection of itemSelections) {
        items[itemSelection.index].isSelected = itemSelection.isSelected;
      }

      if (parentItemId) {
        queryClient.setQueryData(wadQueryKeys.mountedWadDirectoryItems(wadId, parentItemId), items);
      } else {
        queryClient.setQueryData(wadQueryKeys.mountedWadItems(wadId), items);
      }

      return { previousItems };
    },
    onSuccess: (data) => {
      console.info(data);
    },
    onError: (_error, variables, context) => {
      if (!context?.previousItems) {
        return;
      }

      if (variables.parentItemId) {
        queryClient.setQueryData(
          wadQueryKeys.mountedWadDirectoryItems(variables.wadId, variables.parentItemId),
          context.previousItems,
        );
      } else {
        queryClient.setQueryData(
          wadQueryKeys.mountedWadItems(variables.wadId),
          context.previousItems,
        );
      }
    },
    onSettled: (_data, _error, variables) => {
      if (variables.parentItemId) {
        queryClient.invalidateQueries({
          queryKey: wadQueryKeys.mountedWadDirectoryItems(variables.wadId, variables.parentItemId),
        });
      } else {
        queryClient.invalidateQueries({ queryKey: wadQueryKeys.mountedWadItems(variables.wadId) });
      }
    },
  });
};
