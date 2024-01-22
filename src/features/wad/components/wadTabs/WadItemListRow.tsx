import { writeText } from '@tauri-apps/api/clipboard';
import clsx from 'clsx';
import React, { MouseEventHandler, forwardRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { FaSave } from 'react-icons/fa';
import { HiClipboardCopy } from 'react-icons/hi';
import { useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';

import { useExtractWadItemsWithDirectory } from '../..';
import { FolderIcon } from '../../../../assets';
import { ContextMenu, Icon, LoadingOverlay, Toast } from '../../../../components';
import { toastAutoClose } from '../../../../utils/toast';
import { WadItem } from '../../types';
import { getLeagueFileKindIcon, getLeagueFileKindIconColor } from '../../utils';

export type WadItemListRowProps = {
  wadId: string;
  parentId?: string;
  item: WadItem;
  index: number;
  onClick?: MouseEventHandler;
};

export const WadItemListRow: React.FC<WadItemListRowProps> = ({
  wadId,
  parentId,
  item,
  onClick,
}) => {
  const [, setSearchParams] = useSearchParams();

  const handleDoubleClick = useCallback(() => {
    if (item.kind !== 'directory') {
      return;
    }

    // TODO: setting url query here isn't a good idea
    setSearchParams((params) => {
      params.set('itemId', item.id);
      return params;
    });
  }, [item.id, item.kind, setSearchParams]);

  return (
    <WadItemListRowContextMenu wadId={wadId} parentId={parentId} item={item}>
      <div
        className={clsx(
          'text-md box-border flex select-none flex-row border py-1 pl-2 text-gray-50 hover:cursor-pointer',
          { 'hover:bg-gray-700/25': !item.isSelected },
          {
            'border-obsidian-500/40 bg-obsidian-700/40': item.isSelected,
            'border-transparent': !item.isSelected,
          },
        )}
        onClick={(e) => onClick?.(e)}
        onDoubleClick={handleDoubleClick}
        onContextMenu={() => {}}
      >
        <div className="flex flex-row items-center gap-2">
          {item.kind === 'directory' ? (
            <Icon size="md" className="fill-amber-500" icon={FolderIcon} />
          ) : (
            <Icon
              size="md"
              className={clsx(getLeagueFileKindIconColor(item.extensionKind))}
              icon={getLeagueFileKindIcon(item.extensionKind)}
            />
          )}
          {item.name}
        </div>
      </div>
    </WadItemListRowContextMenu>
  );
};

type WadItemListRowContextMenuProps = {
  wadId: string;
  parentId?: string;
  item: WadItem;

  children?: React.ReactNode;
};

const WadItemListRowContextMenu: React.FC<WadItemListRowContextMenuProps> = ({
  wadId,
  parentId,
  item,

  children,
}) => {
  const [t] = useTranslation(['wad', 'common']);

  const handleCopyName = useCallback(async () => {
    await writeText(item.name);

    toast.info(<Toast.Info message={t('common:copied')} />, {
      autoClose: toastAutoClose.veryShort,
    });
  }, [item.name, t]);

  const handleCopyPath = useCallback(async () => {
    await writeText(item.path);

    toast.info(<Toast.Info message={t('common:copied')} />, {
      autoClose: toastAutoClose.veryShort,
    });
  }, [item.path, t]);

  return (
    <ContextMenu.Root>
      <ContextMenu.Trigger asChild>{children}</ContextMenu.Trigger>
      <ContextMenu.Content>
        <ExtractItem wadId={wadId} parentId={parentId} item={item} />
        <ContextMenu.Separator />
        <ContextMenu.Item className="flex flex-row items-center gap-2" onClick={handleCopyName}>
          <Icon icon={HiClipboardCopy} size="md" />
          {t('wad:contextMenu.copyName')}
        </ContextMenu.Item>
        <ContextMenu.Item className="flex flex-row items-center gap-2" onClick={handleCopyPath}>
          <Icon icon={HiClipboardCopy} size="md" />
          {t('wad:contextMenu.copyPath')}
        </ContextMenu.Item>
      </ContextMenu.Content>
    </ContextMenu.Root>
  );
};

type ExtractItemProps = {
  wadId: string;
  parentId?: string;
  item: WadItem;
};

const ExtractItem = forwardRef<HTMLDivElement, ExtractItemProps>(
  ({ wadId, parentId, item }, ref) => {
    const [t] = useTranslation('wad');

    const { progress, message, isExtracting, extractWadItemsWithDirectory } =
      useExtractWadItemsWithDirectory();

    return (
      <>
        <ContextMenu.Item
          ref={ref}
          className="flex flex-row items-center gap-2"
          onClick={() => {
            extractWadItemsWithDirectory({
              wadId,
              parentId,
              items: [item.id],
            });
          }}
        >
          <Icon icon={FaSave} size="md" />
          {t('contextMenu.extract')}
        </ContextMenu.Item>
        <LoadingOverlay
          open={isExtracting}
          onOpenChange={() => {}}
          progress={progress * 100}
          message={message}
        />
      </>
    );
  },
);
