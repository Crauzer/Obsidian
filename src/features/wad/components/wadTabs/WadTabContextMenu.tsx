import React from 'react';
import { useTranslation } from 'react-i18next';

import { ContextMenu, ContextMenuRootProps } from '../../../../components';

export type WadTabContextMenuProps = { wadId: string } & ContextMenuRootProps;

export const WadTabContextMenu: React.FC<WadTabContextMenuProps> = ({
  wadId,
  children,
  ...props
}) => {
  const [t] = useTranslation('mountedWads');

  return (
    <ContextMenu.Root {...props}>
      <ContextMenu.Trigger asChild>{children}</ContextMenu.Trigger>
      <ContextMenu.Content>
        <ContextMenu.Item>{t('tab.closeTooltip')}</ContextMenu.Item>
      </ContextMenu.Content>
    </ContextMenu.Root>
  );
};
