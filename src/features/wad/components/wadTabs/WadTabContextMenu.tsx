import React from 'react';
import { useTranslation } from 'react-i18next';
import { MdClose } from 'react-icons/md';

import { ContextMenu, ContextMenuRootProps, Icon } from '../../../../components';
import { useUnmountWad } from '../../api';

export type WadTabContextMenuProps = { wadId: string } & ContextMenuRootProps;

export const WadTabContextMenu: React.FC<WadTabContextMenuProps> = ({
  wadId,
  children,
  ...props
}) => {
  return (
    <ContextMenu.Root {...props}>
      <ContextMenu.Trigger>{children}</ContextMenu.Trigger>
      <ContextMenu.Content>
        <CloseItem wadId={wadId} />
      </ContextMenu.Content>
    </ContextMenu.Root>
  );
};

type CloseItemProps = {
  wadId: string;
};

export const CloseItem: React.FC<CloseItemProps> = ({ wadId }) => {
  const [t] = useTranslation('mountedWads');

  const unmountWadMutation = useUnmountWad();

  const handleClose = () => {
    unmountWadMutation.mutate({ wadId });
  };

  return (
    <ContextMenu.Item className="flex flex-row items-center gap-2" onClick={handleClose}>
      <Icon className="fill-obsidian-500" icon={MdClose} size="md" />
      {t('tab.closeTooltip')}
    </ContextMenu.Item>
  );
};
