import * as RadixContextMenu from '@radix-ui/react-context-menu';
import React from 'react';

type ContextMenuTriggerProps = RadixContextMenu.ContextMenuTriggerProps;

export const ContextMenuTrigger: React.FC<ContextMenuTriggerProps> = ({ children, ...props }) => {
  return <RadixContextMenu.Trigger {...props}>{children}</RadixContextMenu.Trigger>;
};
