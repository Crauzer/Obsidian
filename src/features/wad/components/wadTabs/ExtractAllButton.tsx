import React, { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { LuFileDown } from 'react-icons/lu';
import { toast } from 'react-toastify';
import { v4 as uuidv4 } from 'uuid';

import { useExtractMountedWad } from '../..';
import { Button, Icon, LoadingOverlay, Tooltip } from '../../../../components';
import { queryClient } from '../../../../lib/query';
import { actionsQueryKeys, useActionProgress } from '../../../actions';
import { usePickDirectory } from '../../../fs';
import { useSettings } from '../../../settings';

type ExtractAllButtonProps = {
  wadId: string;
};

export const ExtractAllButton: React.FC<ExtractAllButtonProps> = ({ wadId }) => {
  const [t] = useTranslation('mountedWads');

  const [actionId] = useState(uuidv4());
  const [isLoadingOverlayOpen, setIsLoadingOverlayOpen] = useState(false);

  const settings = useSettings({});
  const pickDirectory = usePickDirectory();
  const extractMountedWad = useExtractMountedWad();

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

  const extractWad = () => {
    setIsLoadingOverlayOpen(true);

    pickDirectory.mutate(
      { initialDirectory: settings.data?.defaultExtractionDirectory ?? undefined },
      {
        onSuccess: (directory) => {
          extractMountedWad.mutate(
            { wadId, actionId, extractDirectory: directory.path },
            {
              onSuccess: () => {
                toast.success(t('extraction.success'));
              },
              onSettled: () => {
                setIsLoadingOverlayOpen(false);
                queryClient.resetQueries({
                  queryKey: actionsQueryKeys.actionProgress(actionId),
                });
              },
            },
          );
        },
        onError: () => {
          setIsLoadingOverlayOpen(false);
        },
      },
    );
  };

  return (
    <>
      <Tooltip.Root>
        <Tooltip.Trigger asChild>
          <Button
            compact
            variant="ghost"
            onClick={() => {
              extractWad();
            }}
          >
            <Icon size="md" icon={LuFileDown} />
          </Button>
        </Tooltip.Trigger>
        <Tooltip.Content>{t('toolbar.extractAll.tooltip')}</Tooltip.Content>
      </Tooltip.Root>
      <LoadingOverlay
        open={isLoadingOverlayOpen}
        onOpenChange={() => {}}
        progress={progress * 100}
        message={actionProgress.data?.payload.message}
      />
    </>
  );
};
