import { tauri } from '@tauri-apps/api';
import React, { useMemo, useState } from 'react';
import { LuFileDown } from 'react-icons/lu';
import { toast } from 'react-toastify';
import { v4 as uuidv4 } from 'uuid';
import { set } from 'zod';

import { useExtractMountedWad } from '../../..';
import { Button, Icon, LoadingOverlay, Toast } from '../../../../../components';
import { queryClient } from '../../../../../lib/query';
import { useActionProgress, useActionProgressSubscription } from '../../../../actions';
import { usePickDirectory } from '../../../../fs';

type ExtractAllButtonProps = {
  wadId: string;
};

export const ExtractAllButton: React.FC<ExtractAllButtonProps> = ({ wadId }) => {
  const [actionId] = useState(uuidv4());
  const [isLoadingOverlayOpen, setIsLoadingOverlayOpen] = useState(false);

  const pickDirectory = usePickDirectory();
  const extractMountedWad = useExtractMountedWad();

  useActionProgressSubscription(actionId);
  const actionProgress = useActionProgress(actionId);

  const progress = useMemo(() => {
    if (pickDirectory.isSuccess && actionProgress.isSuccess && extractMountedWad.isPending) {
      return actionProgress.data.payload.progress;
    }

    return 0;
  }, [
    actionProgress.data?.payload.progress,
    actionProgress.isSuccess,
    extractMountedWad.isPending,
    pickDirectory.isSuccess,
  ]);

  return (
    <>
      <Button
        compact
        variant="ghost"
        onClick={() => {
          setIsLoadingOverlayOpen(true);

          pickDirectory.mutate(undefined, {
            onSuccess: (directory) => {
              extractMountedWad.mutate(
                { wadId, actionId, extractDirectory: directory.path },
                {
                  onSuccess: () => {},
                  onError: (error) => {
                    console.error(error);
                    toast.error(<Toast.Error message={error.message} />);
                  },
                  onSettled: () => {
                    setIsLoadingOverlayOpen(false);
                    queryClient.resetQueries();
                  },
                },
              );
            },
            onError: () => {
              setIsLoadingOverlayOpen(false);
            },
          });
        }}
      >
        <Icon size="md" icon={LuFileDown} />
      </Button>
      <LoadingOverlay
        open={isLoadingOverlayOpen}
        onOpenChange={() => {}}
        progress={progress * 100}
        message={actionProgress.data?.payload.message}
      />
    </>
  );
};
