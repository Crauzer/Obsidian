import { useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { toast } from 'react-toastify';
import { v4 as uuidv4 } from 'uuid';

import { actionsQueryKeys, useActionProgress } from '../../actions';
import { usePickDirectory } from '../../fs';
import { useSettings } from '../../settings';
import { useExtractWadItems } from '../api';

export type UseExtractWadItemsWithDirectoryContext = {
  wadId: string;
  parentId?: string;
  items: string[];
};

export const useExtractWadItemsWithDirectory = () => {
  const [t] = useTranslation('mountedWads');
  const queryClient = useQueryClient();

  const [actionId, setActionId] = useState<string>(uuidv4());
  const [isExtracting, setIsExtracting] = useState(false);

  const actionProgress = useActionProgress(actionId);
  const settings = useSettings();
  const { mutate: pickDirectoryMutate, isSuccess: pickDirectoryIsSuccess } = usePickDirectory();
  const { mutate: extractWadItemsMutate, isPending: extractWadItemsIsPending } =
    useExtractWadItems();

  const progress = useMemo(() => {
    if (pickDirectoryIsSuccess && actionProgress.isSuccess && extractWadItemsIsPending) {
      return actionProgress.data?.payload.progress;
    }

    return 0;
  }, [
    actionProgress.data?.payload.progress,
    actionProgress.isSuccess,
    extractWadItemsIsPending,
    pickDirectoryIsSuccess,
  ]);

  const extractWadItemsWithDirectory = ({
    wadId,
    parentId,
    items,
  }: UseExtractWadItemsWithDirectoryContext) => {
    setIsExtracting(true);

    pickDirectoryMutate(
      { initialDirectory: settings.data?.defaultExtractionDirectory },
      {
        onSuccess: ({ path: extractDirectory }) => {
          extractWadItemsMutate(
            {
              wadId,
              actionId,
              parentId,
              items,
              extractDirectory,
            },
            {
              onSuccess: () => {
                toast.success(t('exteraction.success'));
              },
              onSettled: () => {
                setIsExtracting(false);
                queryClient.resetQueries({
                  queryKey: actionsQueryKeys.actionProgress(actionId),
                });
              },
            },
          );
        },
        onError: () => {
          setIsExtracting(false);
        },
      },
    );
  };

  return {
    progress,
    message: actionProgress.data?.payload.message,
    isExtracting,
    extractWadItemsWithDirectory,
  };
};
