import { writeText } from '@tauri-apps/api/clipboard';
import React, { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { VscCopy } from 'react-icons/vsc';
import { toast } from 'react-toastify';

import { ContextMenu, Icon, Toast } from '../../../../../components';
import { toastAutoClose } from '../../../../../utils/toast';
import { WadItem } from '../../../types';
import { ExtractItem } from './ExtractItem';
import { ExtractSelectedItem } from './ExtractSelectedItem';

type WadItemRowContextMenuProps = {
  wadId: string;
  parentItemId?: string;
  item: WadItem;

  children?: React.ReactNode;
};

export const WadItemRowContextMenu: React.FC<WadItemRowContextMenuProps> = ({
  wadId,
  parentItemId,
  item,

  children,
}) => {
  return (
    <ContextMenu.Root>
      <ContextMenu.Trigger asChild>{children}</ContextMenu.Trigger>
      <ContextMenu.Content>
        <ExtractItem wadId={wadId} parentItemId={parentItemId} item={item} />
        <ExtractSelectedItem wadId={wadId} parentItemId={parentItemId} item={item} />
        <ContextMenu.Separator />
        <CopyNameItem item={item} />
        <CopyPathItem item={item} />
      </ContextMenu.Content>
    </ContextMenu.Root>
  );
};

type CopyNameItemProps = {
  item: WadItem;
};

const CopyNameItem = ({ item }: CopyNameItemProps) => {
  const [t] = useTranslation(['wad', 'common']);

  const handleCopyName = useCallback(async () => {
    await writeText(item.name);

    toast.info(<Toast.Info message={t('common:copied')} />, {
      autoClose: toastAutoClose.veryShort,
    });
  }, [item.name, t]);

  return (
    <ContextMenu.Item className="flex flex-row items-center gap-2" onClick={handleCopyName}>
      <Icon icon={VscCopy} size="md" />
      {t('wad:contextMenu.copyName')}
    </ContextMenu.Item>
  );
};

type CopyPathItemProps = {
  item: WadItem;
};

export const CopyPathItem = ({ item }: CopyPathItemProps) => {
  const [t] = useTranslation(['wad', 'common']);

  const handleCopyPath = useCallback(async () => {
    await writeText(item.path);

    toast.info(<Toast.Info message={t('common:copied')} />, {
      autoClose: toastAutoClose.veryShort,
    });
  }, [item.path, t]);

  return (
    <ContextMenu.Item className="flex flex-row items-center gap-2" onClick={handleCopyPath}>
      <Icon icon={VscCopy} size="md" />
      {t('wad:contextMenu.copyPath')}
    </ContextMenu.Item>
  );
};
