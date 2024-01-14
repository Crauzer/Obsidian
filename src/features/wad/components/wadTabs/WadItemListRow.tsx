import { writeText } from '@tauri-apps/api/clipboard';
import clsx from 'clsx';
import React, { MouseEventHandler, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { HiClipboardCopy } from 'react-icons/hi';
import { useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';

import { FolderIcon } from '../../../../assets';
import { ContextMenu, Icon, Toast } from '../../../../components';
import { toastAutoClose } from '../../../../utils/toast';
import { WadItem } from '../../types';
import { getLeagueFileKindIcon, getLeagueFileKindIconColor } from '../../utils';

export type WadItemListRowProps = {
  wadId: string;
  item: WadItem;
  index: number;
  onClick?: MouseEventHandler;
};

export const WadItemListRow: React.FC<WadItemListRowProps> = ({ item, onClick }) => {
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
    <WadItemListRowContextMenu item={item}>
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
            <Icon size="lg" className="fill-amber-500" icon={FolderIcon} />
          ) : (
            <Icon
              size="lg"
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
  item: WadItem;

  children?: React.ReactNode;
};

const WadItemListRowContextMenu: React.FC<WadItemListRowContextMenuProps> = ({
  item,

  children,
}) => {
  const [t] = useTranslation(['wad', 'common']);

  const handleCopyName = useCallback(async () => {
    await writeText(item.name);

    toast.info(<Toast.Info message={t('common:copied', { text: item.name })} />, {
      autoClose: toastAutoClose.veryShort,
    });
  }, [item.name, t]);

  const handleCopyPath = useCallback(async () => {
    await writeText(item.path);

    toast.info(<Toast.Info message={t('common:copied', { text: item.path })} />, {
      autoClose: toastAutoClose.veryShort,
    });
  }, [item.path, t]);

  return (
    <ContextMenu.Root>
      <ContextMenu.Trigger asChild>{children}</ContextMenu.Trigger>
      <ContextMenu.Content>
        <ContextMenu.Item>Test</ContextMenu.Item>
        <ContextMenu.Separator />
        <ContextMenu.Item className="flex flex-row items-center gap-2" onClick={handleCopyName}>
          <Icon icon={HiClipboardCopy} size="lg" />
          {t('contextMenu.copyName')}
        </ContextMenu.Item>
        <ContextMenu.Item className="flex flex-row items-center gap-2" onClick={handleCopyPath}>
          <Icon icon={HiClipboardCopy} size="lg" />
          {t('contextMenu.copyPath')}
        </ContextMenu.Item>
      </ContextMenu.Content>
    </ContextMenu.Root>
  );
};
