import { useCallback, useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { LuFolderSync } from 'react-icons/lu';
import { VscFileSymlinkDirectory } from 'react-icons/vsc';
import { Id as ToastId, toast } from 'react-toastify';
import { v4 as uuidv4 } from 'uuid';

import { CDragonLogoIcon, ToolboxIcon } from '../../../assets';
import { ActionIcon, Button, Link, Popover, Spinner, Toast, Tooltip } from '../../../components';
import {
  apiErrorSchema,
  getApiErrorExtension,
  wadHashtablesMissingExtensionSchema,
} from '../../../types/error';
import { env } from '../../../utils';
import { useActionProgress } from '../../actions';
import { useAppDirectory, useOpenPath } from '../../fs';
import { useLoadWadHashtables, useWadHashtableStatus } from '../../hashtable';
import { ToolboxContent } from '../../toolbox';
import { useRefreshHashtables } from '../hooks';

export const Infobar = () => {
  const [t] = useTranslation('common');

  const appDirectory = useAppDirectory();
  const openPath = useOpenPath();

  const handleOpenAppDirectory = () => {
    if (!appDirectory.isSuccess) {
      return;
    }

    openPath.mutate({ path: appDirectory.data.appDirectory });
  };

  return (
    <div className="flex min-h-[32px] flex-row border-t border-gray-600 bg-gray-800">
      {env.DEV && (
        <Popover.Root>
          <Popover.Trigger asChild>
            <ActionIcon size="md" variant="ghost" icon={ToolboxIcon} />
          </Popover.Trigger>
          <Popover.Content className="w-[300px]" side="top" sideOffset={8}>
            <ToolboxContent />
          </Popover.Content>
        </Popover.Root>
      )}
      <WadHashtablesIcon />
      {appDirectory.isSuccess && (
        <Tooltip.Root>
          <Tooltip.Trigger asChild>
            <ActionIcon
              size="md"
              variant="ghost"
              icon={VscFileSymlinkDirectory}
              onClick={handleOpenAppDirectory}
            />
          </Tooltip.Trigger>
          <Tooltip.Content>{t('infobar.appDirectory')}</Tooltip.Content>
        </Tooltip.Root>
      )}
    </div>
  );
};

const WadHashtablesIcon = () => {
  const [t] = useTranslation('common');
  const { handleRefresh, loadHashtablesMutation } = useRefreshHashtables();
  const wadHashtableStatus = useWadHashtableStatus();

  useEffect(() => {
    if (!wadHashtableStatus.isSuccess) {
      return;
    }

    if (!wadHashtableStatus.data.isLoaded && loadHashtablesMutation.isIdle) {
      handleRefresh();
    }
  }, [
    handleRefresh,
    loadHashtablesMutation.isIdle,
    wadHashtableStatus.data?.isLoaded,
    wadHashtableStatus.isSuccess,
  ]);

  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>
        {loadHashtablesMutation.isPending ? (
          <Button compact variant="ghost">
            <Spinner className="h-5 w-5" />
          </Button>
        ) : (
          <ActionIcon size="md" variant="ghost" icon={LuFolderSync} onClick={handleRefresh} />
        )}
      </Tooltip.Trigger>
      <Tooltip.Content>{t('infobar.refreshHashtables')}</Tooltip.Content>
    </Tooltip.Root>
  );
};
