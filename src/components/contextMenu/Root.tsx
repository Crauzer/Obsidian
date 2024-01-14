import * as RadixContextMenu from '@radix-ui/react-context-menu';
import React from 'react';

export type ContextMenuRootProps = RadixContextMenu.ContextMenuProps;

export const ContextMenuRoot: React.FC<ContextMenuRootProps> = (props) => {
  return <RadixContextMenu.Root {...props} />;
};
