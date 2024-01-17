import React from 'react';

import { ContextMenu, ContextMenuRootProps } from '../../../../components';

export type WadTabContextMenuProps = { wadId: string } & ContextMenuRootProps;

export const WadTabContextMenu: React.FC<WadTabContextMenuProps> = ({
  wadId,
  children,
  ...props
}) => {
  return (
    <ContextMenu.Root {...props}>
      <ContextMenu.Trigger asChild>{children}</ContextMenu.Trigger>
      <ContextMenu.Content>
        <ContextMenu.Item></ContextMenu.Item>
      </ContextMenu.Content>
    </ContextMenu.Root>
  );
};
