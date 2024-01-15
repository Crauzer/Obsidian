import { useQuery } from '@tanstack/react-query';
import { listen } from '@tauri-apps/api/event';
import { useEffect } from 'react';

import { actionsQueryKeys } from '..';
import { queryClient } from '../../../lib/query';
import { ActionProgressEvent, actionProgressEventSchema } from '../types';

export const useActionProgress = (actionId: string) => {
  useEffect(() => {
    const unlisten = listen(actionId, (event) => {
      const actionEvent = actionProgressEventSchema.safeParse(event);
      if (actionEvent.success) {
        queryClient.setQueryData(actionsQueryKeys.actionProgress(actionId), actionEvent.data);
      } else {
        console.error('invalid action event', event, actionEvent.error);
      }
    });

    return () => {
      unlisten.then((x) => x());
    };
  }, [actionId]);

  return useQuery<ActionProgressEvent>({
    queryKey: actionsQueryKeys.actionProgress(actionId),
    staleTime: Infinity,
  });
};
