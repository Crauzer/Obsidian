import { useEffect, useRef, useState } from 'react';
import { Id as ToastId, toast } from 'react-toastify';
import { v4 as uuidv4 } from 'uuid';

import { ActionIcon, Button, Icon, Popover, Toast } from '..';
import { TableSyncIcon, ToolboxIcon } from '../../assets';
import { useActionProgress, useActionProgressSubscription } from '../../features/actions';
import { useLoadWadHashtables, useWadHashtableStatus } from '../../features/hashtable';
import { ToolboxContent } from '../../features/toolbox';
import { env } from '../../utils';

export const Infobar = () => {
  return (
    <div className="flex min-h-[32px] flex-row  border border-t border-gray-600 bg-gray-800">
      {env.DEV && (
        <Popover.Root>
          <Popover.Trigger asChild>
            <ActionIcon size="lg" variant="ghost" icon={ToolboxIcon} />
          </Popover.Trigger>
          <Popover.Content className="w-[300px]" side="top" sideOffset={8}>
            <ToolboxContent />
          </Popover.Content>
        </Popover.Root>
      )}
      <WadHashtablesBar />
    </div>
  );
};

const WadHashtablesBar = () => {
  const hashtablesLoadingToastId = useRef<ToastId>('');

  const [actionId, setActionId] = useState(uuidv4());

  const loadHashtablesMutation = useLoadWadHashtables();
  const wadHashtableStatus = useWadHashtableStatus();

  useEffect(() => {
    if (!wadHashtableStatus.isSuccess) {
      return;
    }

    if (!wadHashtableStatus.data.isLoaded && loadHashtablesMutation.isIdle) {
      hashtablesLoadingToastId.current = toast.info('Loading hashtables...', { autoClose: false });
      loadHashtablesMutation.mutate(
        { actionId },
        {
          onSuccess: () => {
            toast.update(hashtablesLoadingToastId.current, {
              type: 'success',
              render: 'Hashtables loaded!',
              autoClose: 2500,
            });
          },
          onError: (error) => {
            toast.update(hashtablesLoadingToastId.current, {
              type: 'error',
              render: <Toast.Error title="Failed to load hashtables" message={error.message} />,
            });
          },
        },
      );
    }
  }, [
    actionId,
    hashtablesLoadingToastId,
    loadHashtablesMutation,
    wadHashtableStatus.data?.isLoaded,
    wadHashtableStatus.isSuccess,
  ]);

  useActionProgressSubscription(actionId);

  return (
    <div className="relative flex flex-row items-center">
      <Button size="lg" variant="ghost" onClick={() => {}}>
        <Icon className="fill-gray-50" size="lg" icon={TableSyncIcon} />
        <Test actionId={actionId} />
      </Button>
      <span className="absolute right-0 top-0 flex h-3 w-3">
        <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-obsidian-400 opacity-75"></span>
        <span className="relative inline-flex h-3 w-3 rounded-full bg-obsidian-500"></span>
      </span>
    </div>
  );
};

const Test = ({ actionId }: { actionId: string }) => {
  const actionProgress = useActionProgress(actionId);

  if (actionProgress.isSuccess) {
    return <span>{actionProgress.data.payload.message}</span>;
  }

  return <></>;
};
