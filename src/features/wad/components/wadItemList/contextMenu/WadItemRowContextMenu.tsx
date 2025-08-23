import { writeText } from '@tauri-apps/plugin-clipboard-manager';
import React, { useCallback, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { PiEyeDuotone } from 'react-icons/pi';
import { VscCopy } from 'react-icons/vsc';
import { toast } from 'react-toastify';
import { Skeleton } from '~/components';
import { useItemPreviewTypes } from '~/features/wad';

import { ContextMenu, Icon, Toast } from '../../../../../components';
import { toastAutoClose } from '../../../../../utils/toast';
import { useWadContext } from '../../../providers';
import { WadFileItem, WadItem } from '../../../types';
import { isLeagueFilePreviewable } from '../../../utils';
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
  const [open, setOpen] = useState(false);

  const previewTypesQuery = useItemPreviewTypes({
    wadId,
    itemId: item.id,
    enabled: open,
  });

  const handleOpenChange = useCallback((open: boolean) => {
    setOpen(open);
  }, []);

  return (
    <ContextMenu.Root onOpenChange={handleOpenChange}>
      <ContextMenu.Trigger>{children}</ContextMenu.Trigger>
      <ContextMenu.Content>
        {item.kind === 'file' && previewTypesQuery.isLoading && <Skeleton className="w-full" />}
        {item.kind === 'file' && previewTypesQuery.isSuccess && (
          <ContextMenu.Sub>
            <ContextMenu.SubTrigger>Preview as</ContextMenu.SubTrigger>
            <ContextMenu.Portal>
              <ContextMenu.SubContent></ContextMenu.SubContent>
            </ContextMenu.Portal>
          </ContextMenu.Sub>
        )}
        {item.kind === 'file' && (
          <>
            <PreviewItem item={item} />
            <ContextMenu.Separator />
          </>
        )}
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

type PreviewItemProps = {
  item: WadFileItem;
};

const PreviewItem = ({ item }: PreviewItemProps) => {
  const { changeCurrentPreviewItemId } = useWadContext();
  const [t] = useTranslation(['wad', 'common']);

  const handleClick = useCallback(() => {
    changeCurrentPreviewItemId(item.id);
  }, [changeCurrentPreviewItemId, item.id]);

  return (
    <ContextMenu.Item
      disabled={!isLeagueFilePreviewable(item.extensionKind)}
      className="flex flex-row items-center gap-2"
      onClick={handleClick}
    >
      <Icon icon={PiEyeDuotone} size="md" />
      {t('wad:contextMenu.preview')}
    </ContextMenu.Item>
  );
};
